﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using APM.Api.Interfaces;
using APM.Domain;

namespace APM.Api.Controllers
{
    [Produces("application/json")]
    [ApiVersion("0.1")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class EventController : Controller
    {
        private readonly IStoreRepository _storeRepository;

        public EventController(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        /// <summary>
        /// Returns a single event (with codes)
        /// </summary>
        /// <param name="eventName">The name of the event to return</param>
        /// <returns>200 containing an Event</returns>
        [HttpGet("{eventName}")]
        public async Task<IActionResult> Get(string eventName)
        {
            //get all codes
            var allCodes = await _storeRepository.GetCodes();

            //filter codes on EventName
            var codesInEvent = allCodes
                .Where(x => x.EventName.ToLower() == eventName.ToLower())
                .ToList();

            //all of this assumes all codes in the same EventName have common values
            var owner = codesInEvent.FirstOrDefault().Owner;
            var expiry = codesInEvent.FirstOrDefault().Expiry;
            var url = Helpers.Helpers.EventNameToEventUrl(eventName);
            var codes = codesInEvent;

            //create Event object
            var evnt = new Event()
            {
                Codes = codes,
                EventName = eventName,
                Expiry = expiry,
                Owner = owner,
                Url = url
            };

            return Ok(evnt);
        }
    }
}