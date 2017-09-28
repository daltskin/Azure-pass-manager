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
    public class EventsController : Controller
    {
        private readonly IStoreRepository _storeRepository;

        public EventsController(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        [HttpGet("{owner}")]
        public async Task<IActionResult> Get(string owner)
        {
            var codes = await _storeRepository.GetCodes();

            var events = codes
                .Where(x => x.Owner.ToLower() == owner.ToLower())
                .Select(x => x.EventName)
                .Distinct()
                .ToList();

            return Ok(events);
        }
        
    }
}