using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.API.Data;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gcpe.Hub.API.Controllers
{
    // TODO: Re-enable this ==> [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
    [Produces("application/json")]
    public class NewsReleasesController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly ILogger<NewsReleasesController> _logger;
        private readonly IMapper _mapper;

        public NewsReleasesController(IRepository repository,
            ILogger<NewsReleasesController> logger,
            IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [NonAction]
        public IEnumerable<NewsRelease> GetResultsPage(NewsReleaseParams newsReleaseParams)
        {
            var newsReleases = _repository.GetAllReleases();
            var pagedNewsReleases = PagedList<NewsRelease>.Create(newsReleases, newsReleaseParams.PageNumber, newsReleaseParams.PageSize);
            return pagedNewsReleases;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult GetAll([FromQuery] NewsReleaseParams newsReleaseParams)
        {
            try
            {
                var count = _repository.GetAllReleases().Count();
                var pagedNewsReleases = this.GetResultsPage(newsReleaseParams);
                Response.AddPagination(newsReleaseParams.PageNumber, newsReleaseParams.PageSize, count, 10);

                return Ok(pagedNewsReleases);
            }
            catch (Exception ex)
            {
                return this.BadRequest(_logger, "Failed to get releases", ex);
            }
        }

        [HttpGet("{id}", Name = "GetRelease")]
        [Produces(typeof(Models.NewsRelease))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetById(string id)
        {
            try
            {
                var dbRelease = _repository.GetReleaseByKey(id);

                if (dbRelease != null)
                {
                    return Ok(_mapper.Map<Models.NewsRelease>(dbRelease));
                }
                else return NotFound();
            }
            catch (Exception ex)
            {
                return this.BadRequest(_logger, "Failed to get release", ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult Post([FromBody]Models.NewsRelease release)
        {
            try
            {
                if (release == null)
                {
                    throw new ValidationException();
                }
                var newDbRelease = _mapper.Map<Models.NewsRelease>(release);

                _repository.AddEntity(newDbRelease);
                // can assume that this always works against an in memory dataset
                return StatusCode(201);

                // can be un-commented when working with a db
                // return CreatedAtRoute("GetRelease", new { id = newRelease.Id }, newRelease);
                //if (_repository.SaveAll())
                //{
                //    return CreatedAtRoute("GetRelease", new { id = newRelease.Id }, newRelease);
                //}
            }
            catch (Exception ex)
            {
                return this.BadRequest(_logger, "Failed to save a new release", ex);
            }
        }

        [HttpPut("{id}")]
        [Produces(typeof(Models.NewsRelease))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Put(string id, [FromBody] Models.NewsRelease release)
        {
            try
            {
                var dbRelease = _repository.GetReleaseByKey(id);
                if (dbRelease == null)
                {
                    return NotFound($"Could not find a release with an id of {id}");
                }
                dbRelease = _mapper.Map(release, dbRelease);
                dbRelease.Timestamp = DateTimeOffset.Now;
                _repository.Update(id, dbRelease);
                return Ok(release);
            }
            catch (Exception ex)
            {
                return this.BadRequest(_logger, "Couldn't update release", ex);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult Delete(string id)
        {
            try
            {
                var dbRelease = _repository.GetReleaseByKey(id);
                if (dbRelease == null)
                {
                    return NotFound($"Could not find release with id of {id}");
                }
                _repository.Delete(dbRelease);
                return Ok();
            }
            catch (Exception ex)
            {
                return this.BadRequest(_logger, "Could not delete release", ex);
            }
        }

    }
}

