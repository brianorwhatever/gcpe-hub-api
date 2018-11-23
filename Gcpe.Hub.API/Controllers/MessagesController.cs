using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.Data.Entity;
using Gcpe.Hub.API.Data;
using Gcpe.Hub.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Gcpe.Hub.API.ViewModels;

namespace Gcpe.Hub.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private readonly HubDbContext _dbContext;
        private readonly ILogger<MessagesController> _logger;
        private readonly IMapper _mapper;

        public MessagesController(HubDbContext dbContext,
            ILogger<MessagesController> logger,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult GetAll()
        {
            try
            {
                var messages = _dbContext.Message.ToList();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get messages: {ex}");
                return BadRequest("Failed to get messages");
            }
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult CreateMessage(MessageViewModel message)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    _dbContext.Message.Add(_mapper.Map<MessageViewModel, Message>(message));
                    _dbContext.SaveChanges();
                    return CreatedAtRoute("GetMessage", new { id = message.Id }, message);
                }
                else
                {
                    return BadRequest(ModelState);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create message: {ex}");
            }

            return BadRequest("Failed to create message");
        }

        [HttpGet("{id}")]
        [Produces(typeof(MessageViewModel))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetMessage(string id)
        {
            try
            {
                var message = _dbContext.Message.Find(id);
                if(message != null)
                {
                    return Ok(_mapper.Map<Message, MessageViewModel>(message));
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to retrieve message: {ex}");
            }
            return BadRequest("Failed to retrieve message");
        }

        [HttpPut("{id}")]
        [Produces(typeof(MessageViewModel))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateMessage(string id, MessageViewModel message)
        {
            try
            {
                var oldMessage = _dbContext.Message.Find(id);
                if(oldMessage == null)
                {
                    return NotFound($"Message not found with id: {id}");
                }
                _dbContext.Message.Update(_mapper.Map<MessageViewModel, Message>(message));
                _dbContext.SaveChanges();
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update message: {ex}");
            }

            return BadRequest("Failed to update message");
        }
    }
}
