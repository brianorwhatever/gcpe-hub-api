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
                message.Id = Guid.NewGuid();
                message.Timestamp = DateTime.Now;
                var dbMessage = new Message { IsActive = true };
                dbContext.Entry(dbMessage).CurrentValues.SetValues(message);
                if (message.IsPublished)
                {
                    InsertAndBumpSortOrders(dbMessage);
                    EnsureHighlightIsUnique(dbMessage);
                }
                dbContext.Message.Add(dbMessage);
                dbContext.SaveChanges();
                return CreatedAtRoute("GetMessage", new { id = dbMessage.Id }, mapper.Map<Models.Message>(dbMessage));
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create message", ex);
            }
        }

        private void InsertAndBumpSortOrders(Message dbMessage)
        {
            IList<Message> messagesToBump = dbContext.Message.Where(m => m.IsPublished && m.IsActive).OrderBy(m => m.SortOrder).ToList();
            for (int i = 0; i < messagesToBump.Count; i++)
            {
                messagesToBump[i].SortOrder = i + 1;
            }
            dbMessage.SortOrder = 0;
        }

        private void EnsureHighlightIsUnique(Message dbMessage)
        {
            if (dbMessage.IsHighlighted )
            {
                var oldHighlights = dbContext.Message.Where(m => m.IsHighlighted && m.IsPublished && m.Id != dbMessage.Id);
                foreach (Message oldHighlight in oldHighlights)
                {
                    oldHighlight.IsHighlighted = false;
                }
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
                    int oldSortOrder = dbMessage.SortOrder;
                    bool wasPublished = dbMessage.IsPublished;
                    message.Timestamp = DateTime.Now;
                    message.Id = id;
                    dbContext.Entry(dbMessage).CurrentValues.SetValues(message);
                    if (message.IsPublished)
                    {
                        if (!wasPublished)
                        {
                            InsertAndBumpSortOrders(dbMessage);
                        }
                        else if (message.SortOrder != oldSortOrder)
                        {
                            bool up = message.SortOrder < oldSortOrder;
                            IQueryable<Message> messages = dbContext.Message.Where(m => m.IsPublished && m.IsActive);
                            Message messageToSwap = (up ? messages.Where(m => m.SortOrder < oldSortOrder).OrderByDescending(m => m.SortOrder)
                                                        : messages.Where(m => m.SortOrder > oldSortOrder).OrderBy(m => m.SortOrder)).FirstOrDefault();
                            if (messageToSwap != null) // null if there are no other messages below or above (e.g Unit tests)
                            {
                                dbMessage.SortOrder = messageToSwap.SortOrder;
                                messageToSwap.SortOrder = oldSortOrder;
                            }
                        }
                        EnsureHighlightIsUnique(dbMessage);
                    }
                    dbContext.Message.Update(dbMessage);
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
