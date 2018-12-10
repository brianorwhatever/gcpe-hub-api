using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.API.ViewModels;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        private IActionResult GetBadRequest(string message, Exception ex)
        {
            logger.LogError($"{message}: {ex}");
            return BadRequest(message);
        }

        [HttpGet]
        [Produces(typeof(MessageViewModel))]
        [ProducesResponseType(400)]
        public IActionResult GetAll([FromQuery(Name = "IsPublished")] Boolean IsPublished = true)
        {
            try
            {
                var messages = dbContext.Message.Where(m => m.IsPublished == IsPublished).OrderBy(p => p.SortOrder).ToList();
                return Ok(mapper.Map<List<Message>, List<MessageViewModel>>(messages));
            }
            catch (Exception ex)
            {
                return GetBadRequest("Failed to retrieve messages", ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(MessageViewModel), 201)]
        [ProducesResponseType(400)]
        public IActionResult Post(MessageViewModel messageVM)
        {
            try
            {
                if (messageVM.Id != Guid.Empty)
                {
                    throw new ValidationException("Invalid parameter (id)");
                }
                var message = mapper.Map<MessageViewModel, Message>(messageVM);
                message.Id = Guid.NewGuid();
                dbContext.Message.Add(message);
                dbContext.SaveChanges();
                return CreatedAtRoute("GetMessage", new { id = message.Id }, mapper.Map<Message, MessageViewModel>(message));
            }
            catch (Exception ex)
            {
                return GetBadRequest("Failed to create message", ex);
            }
        }

        [HttpGet("{id}", Name = "GetMessage")]
        [Produces(typeof(MessageViewModel))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Get(Guid id)
        {
            try
            {
                var message = dbContext.Message.Find(id);
                if (message != null)
                {
                    return Ok(mapper.Map<Message, MessageViewModel>(message));
                }
                return NotFound($"Message not found with id: {id}");
            }
            catch (Exception ex)
            {
                return GetBadRequest("Failed to retrieve message", ex);
            }
        }

        [HttpPut("{id}")]
        [Produces(typeof(MessageViewModel))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Put(Guid id, MessageViewModel messageVM)
        {
            try
            {
                Message dbMessage = dbContext.Message.Find(id);
                if (dbMessage != null)
                {
                    dbMessage = mapper.Map(messageVM, dbMessage);
                    dbMessage.Timestamp = DateTime.Now;
                    dbMessage.Id = id;
                    dbContext.Message.Update(dbMessage);

                    dbContext.SaveChanges();
                    return Ok(mapper.Map<Message, MessageViewModel>(dbMessage));
                }
                return NotFound($"Message not found with id: {id}");
            }
            catch (Exception ex)
            {
                return GetBadRequest("Failed to update message", ex);
            }
        }
    }
}
