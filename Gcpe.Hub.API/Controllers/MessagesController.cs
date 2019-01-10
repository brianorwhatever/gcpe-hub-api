using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gcpe.Hub.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Produces("application/json")]
    public class MessagesController : BaseController
    {
        private readonly HubDbContext dbContext;
        private readonly IMapper mapper;
        static DateTime? lastModified = null;
        static DateTime lastModifiedNextCheck = DateTime.Now;

        public MessagesController(HubDbContext dbContext, ILogger<MessagesController> logger, IMapper mapper) : base(logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        // Bumps the sort order of all published messages from firstSortOrder to lastSortOrder
        private int BumpSortOrders(int direction, int firstSortOrder, int? lastSortOrder)
        {
            IQueryable<Message> messages = dbContext.Message.Where(m => m.IsPublished && m.IsActive && m.SortOrder >= firstSortOrder);
            if (lastSortOrder != null)
            {
                messages = messages.Where(m => m.SortOrder <= lastSortOrder);
            }

            foreach (Message bumpedMessage in messages)
            {
                bumpedMessage.SortOrder = bumpedMessage.SortOrder + direction;
            }

            return messages.Count();
        }

        [HttpGet]
        [Produces(typeof(IEnumerable<Models.Message>))]
        [ProducesResponseType(304)]
        [ProducesResponseType(400)]
        [ResponseCache(Duration = 5)]
        public IActionResult GetAllMessages([FromQuery(Name = "IsPublished")] bool IsPublished = true)
        {
            try
            {
                IQueryable<Message> dbMessages = dbContext.Message;

                IActionResult res = HandleModifiedSince(ref lastModified, ref lastModifiedNextCheck, () => dbMessages.OrderByDescending(p => p.Timestamp).FirstOrDefault()?.Timestamp);
                return res ?? Ok(mapper.Map<List<Models.Message>>(dbMessages.Where(m => m.IsPublished == IsPublished && m.IsActive).OrderBy(p => p.SortOrder).ToList()));
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to retrieve messages", ex);
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
                dbMessage.IsActive = true;

                if (dbMessage.IsPublished)
                {
                    dbMessage.SortOrder = 0;
                    BumpSortOrders(1, 0, null);
                }
                dbMessage.Id = Guid.NewGuid();
                dbMessage.Timestamp = DateTime.Now;
                dbContext.Message.Add(dbMessage);
                if (dbMessage.IsHighlighted && dbMessage.IsPublished)
                {
                    var oldHighlights = dbContext.Message.Where(m => m.IsHighlighted == true && m.IsPublished == true);
                    foreach (Message oldHighlight in oldHighlights)
                    {
                        oldHighlight.IsHighlighted = false;
                    }
                }
                dbContext.SaveChanges();
                return CreatedAtRoute("GetMessage", new { id = dbMessage.Id }, mapper.Map<Models.Message>(dbMessage));
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create message", ex);
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
                if (dbMessage != null && dbMessage.IsActive)
                {
                    return Ok(mapper.Map<Models.Message>(dbMessage));
                }
                return NotFound($"Message not found with id: {id}");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to retrieve message", ex);
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
                if (dbMessage != null && dbMessage.IsActive)
                {
                    if (!dbMessage.IsPublished && message.IsPublished)
                    {
                        message.SortOrder = 0;
                        BumpSortOrders(1, 0, null);
                    }
                    else
                    {
                        // going up!
                        if (dbMessage.SortOrder > message.SortOrder)
                        {
                            BumpSortOrders(1, message.SortOrder, dbMessage.SortOrder);
                        }
                        // going down¡
                        else if (dbMessage.SortOrder < message.SortOrder)
                        {
                            BumpSortOrders(-1, dbMessage.SortOrder, message.SortOrder);
                        }

                    }
                    dbMessage = mapper.Map(message, dbMessage);
                    dbMessage.Timestamp = DateTime.Now;
                    dbMessage.Id = id;
                    dbContext.Message.Update(dbMessage);
                    if (dbMessage.IsHighlighted && dbMessage.IsPublished)
                    {
                        var oldHighlights = dbContext.Message.Where(m => m.IsHighlighted == true && m.IsPublished == true && m.Id != dbMessage.Id);
                        foreach (Message oldHighlight in oldHighlights)
                        {
                            oldHighlight.IsHighlighted = false;
                        }
                    }

                    dbContext.SaveChanges();
                    return Ok(mapper.Map<Models.Message>(dbMessage));
                }
                return NotFound($"Message not found with id: {id}");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to update message", ex);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteMessage(Guid id)
        {
            try
            {
                var dbMessage = dbContext.Message.Find(id);
                if (dbMessage != null && dbMessage.IsActive)
                {
                    dbMessage.IsActive = false;
                    dbMessage.Timestamp = DateTime.Now;
                    dbContext.Message.Update(dbMessage);
                    dbContext.SaveChanges();
                    return new NoContentResult();
                }
                return NotFound($"Message not found with id: {id}");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to delete message", ex);
            }
        }
    }
}
