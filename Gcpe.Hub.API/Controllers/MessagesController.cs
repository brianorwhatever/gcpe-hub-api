using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.API.Helpers;
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

        [HttpGet]
        [Produces(typeof(IEnumerable<Models.Message>))]
        [ProducesResponseType(400)]
        public IActionResult GetAllMessages([FromQuery(Name = "IsPublished")] bool IsPublished = true)
        {
            try
            {
                var dbMessages = dbContext.Message.Where(m => m.IsPublished == IsPublished).OrderBy(p => p.SortOrder).ToList();
                return Ok(mapper.Map<List<Models.Message>>(dbMessages));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to retrieve messages", ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Models.Message), 201)]
        [ProducesResponseType(400)]
        public IActionResult AddMessage(Models.Message message)
        {
            try
            {
                if (message.Id != Guid.Empty)
                {
                    throw new ValidationException("Invalid parameter (id)");
                }
                var dbMessage = mapper.Map<Message>(message);
                dbMessage.Id = Guid.NewGuid();
                dbContext.Message.Add(dbMessage);
                if (dbMessage.IsHighlighted && dbMessage.IsPublished)
                {
                    var oldHighlight = dbContext.Message.Where(m => m.IsHighlighted == true && m.IsPublished == true).First();
                    oldHighlight.IsHighlighted = false;
                }
                dbContext.SaveChanges();
                return CreatedAtRoute("GetMessage", new { id = dbMessage.Id }, mapper.Map<Models.Message>(dbMessage));
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to create message", ex);
            }
        }

        [HttpGet("{id}", Name = "GetMessage")]
        [Produces(typeof(Models.Message))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetMessage(Guid id)
        {
            try
            {
                var dbMessage = dbContext.Message.Find(id);
                if (dbMessage != null)
                {
                    return Ok(mapper.Map<Models.Message>(dbMessage));
                }
                return NotFound($"Message not found with id: {id}");
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to retrieve message", ex);
            }
        }

        [HttpPut("{id}")]
        [Produces(typeof(Models.Message))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateMessage(Guid id, Models.Message message)
        {
            try
            {
                var dbMessage = dbContext.Message.Find(id);
                if (dbMessage != null)
                {
                    dbMessage = mapper.Map(message, dbMessage);
                    dbMessage.Timestamp = DateTime.Now;
                    dbMessage.Id = id;
                    dbContext.Message.Update(dbMessage);
                    if (dbMessage.IsHighlighted && dbMessage.IsPublished)
                    {
                        var oldHighlight = dbContext.Message.Where(m => m.IsHighlighted == true && m.IsPublished == true).First();
                        oldHighlight.IsHighlighted = false;
                    }

                    dbContext.SaveChanges();
                    return Ok(mapper.Map<Models.Message>(dbMessage));
                }
                return NotFound($"Message not found with id: {id}");
            }
            catch (Exception ex)
            {
                return this.BadRequest(logger, "Failed to update message", ex);
            }
        }
    }
}
