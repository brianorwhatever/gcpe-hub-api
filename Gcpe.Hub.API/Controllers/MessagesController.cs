﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.API.ViewModels;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Gcpe.Hub.API.ViewModels;
using Gcpe.Hub.API.Helpers;

namespace Gcpe.Hub.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private readonly HubDbContext dbContext;
        private readonly ILogger<MessagesController> logger;
        private readonly IMapper mapper;

        public MessagesController(HubDbContext dbContext, ILogger<MessagesController> logger, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult GetAll([FromQuery(Name = "IsPublished")] Boolean IsPublished = true)
        {
            try
            {
                var messages = dbContext.Message.Where(m => m.IsPublished == IsPublished).ToList();
                mapper.Map<List<Message>, List<MessageViewModel>>(messages);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to get messages: {ex}");
                return BadRequest("Failed to get messages");
            }
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult Post(MessageViewModel message)
        {
            try
            {
                message.Id = Guid.NewGuid();
                dbContext.Message.Add(mapper.Map<MessageViewModel, Message>(message));
                dbContext.SaveChanges();
                return CreatedAtRoute("GetMessage", new { id = message.Id }, message);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create message: {ex}");
                return BadRequest("Failed to create message");
            }
        }

        [HttpGet("{id}", Name = "GetMessage")]
        [Produces(typeof(MessageViewModel))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Get(Guid id)
        {
            try
            {
                var message = dbContext.Message.Find(id);
                if(message != null)
                {
                    return Ok(mapper.Map<Message, MessageViewModel>(message));
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to retrieve message: {ex}");
                return BadRequest("Failed to retrieve message");
            }
        }

        [HttpPut("{id}")]
        [Produces(typeof(MessageViewModel))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Put(Guid id, MessageViewModel message)
        {
            try
            {
                var oldMessage = dbContext.Message.Find(id);
                if(oldMessage == null)
                {
                    return NotFound($"Message not found with id: {id}");
                }
                message.Id = id;
                dbContext.Entry(oldMessage).CurrentValues.SetValues(mapper.Map<MessageViewModel, Message>(message));
                dbContext.SaveChanges();
                return Ok(message);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to update message: {ex}");
                return BadRequest("Failed to update message");
            }
        }
    }
}
