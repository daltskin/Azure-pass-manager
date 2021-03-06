﻿using APM.Domain;
using APM.Web.Interfaces;
using APM.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace APM.Web.Repositories
{
    public class ApiRepository : IApiRepository
    {
        private readonly AppSettings _appSettings;

        public ApiRepository(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public async Task<bool> StoreCodeBatch(CodeBatch codeBatch)
        {
            //setup HttpClient with content
            var httpClient = GetHttpClient();

            //setup body
            var content = new StreamContent(new MemoryStream(codeBatch.File));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");

            //construct full API endpoint uri
            DateTimeFormatInfo dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            var parameters = new Dictionary<string, string>
            {
                { "Expiry", codeBatch.Expiry.ToString(dtfi)},
                { "EventName", codeBatch.EventName },
                { "Owner", codeBatch.Owner }
            };
            var apiBaseUrl = $"{_appSettings.APIBaseUrl}/codes";
            var apiUri = QueryHelpers.AddQueryString(apiBaseUrl, parameters);

            //make request
            var responseMessage = await httpClient.PostAsync(apiUri, content);

            return responseMessage.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Event>> GetEventsByOwner(string owner)
        {
            //setup HttpClient with content
            var httpClient = GetHttpClient();

            //construct full API endpoint uri
            var parameters = new Dictionary<string, string>
            {
                { "Owner", owner }
            };
            var apiBaseUrl = $"{_appSettings.APIBaseUrl}/events";
            var apiUri = QueryHelpers.AddQueryString(apiBaseUrl, parameters);

            //make request
            var responseMessage = await httpClient.GetAsync(apiUri);

            if (responseMessage.IsSuccessStatusCode)
            {
                //cast to array of items
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var responseArray = JArray.Parse(responseString);
                var items = new List<Event>();
                foreach (var response in responseArray)
                {
                    var item = JsonConvert.DeserializeObject<Event>(response.ToString());
                    items.Add(item);
                }

                return items;
            }
            else
            {
                return null;
            }
        }

        public async Task<Event> GetEventByEventName(string eventName)
        {
            //setup HttpClient with content
            var httpClient = GetHttpClient();

            //construct full API endpoint uri
            var parameters = new Dictionary<string, string>
            {
                { "EventName", eventName }
            };
            var apiBaseUrl = $"{_appSettings.APIBaseUrl}/event";
            var apiUri = QueryHelpers.AddQueryString(apiBaseUrl, parameters);

            //make request
            var responseMessage = await httpClient.GetAsync(apiUri);

            if (responseMessage.IsSuccessStatusCode)
            {
                //cast to item
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var evnt = JsonConvert.DeserializeObject<Event>(responseString.ToString());

                return evnt;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteEventByEventName(string eventName)
        {
            //setup HttpClient with content
            var httpClient = GetHttpClient();

            //construct full API endpoint uri
            var parameters = new Dictionary<string, string>
            {
                { "EventName", eventName }
            };
            var apiBaseUrl = $"{_appSettings.APIBaseUrl}/event";
            var apiUri = QueryHelpers.AddQueryString(apiBaseUrl, parameters);

            //make request
            var responseMessage = await httpClient.DeleteAsync(apiUri);

            return responseMessage.IsSuccessStatusCode;
        }

        public async Task<Code> ClaimCode(string eventName)
        {
            //setup HttpClient with content
            var httpClient = GetHttpClient();

            //construct full API endpoint uri
            var parameters = new Dictionary<string, string>
            {
                { "EventName", eventName }
            };
            var apiBaseUrl = $"{_appSettings.APIBaseUrl}/claim";
            var apiUri = QueryHelpers.AddQueryString(apiBaseUrl, parameters);

            //make request
            var responseMessage = await httpClient.GetAsync(apiUri);

            if (responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.ReasonPhrase == "NoContent")
                {
                    return null;
                }

                //cast to item
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var code = JsonConvert.DeserializeObject<Code>(responseString.ToString());

                return code;
            }
            else
            {
                return null;
            }
        }

        public async Task<Code> GetCode(string eventName, string promoCode)
        {
            //setup HttpClient with content
            var httpClient = GetHttpClient();

            //construct full API endpoint uri
            var parameters = new Dictionary<string, string>
            {
                { "EventName", eventName },
                { "PromoCode", promoCode }
            };
            var apiBaseUrl = $"{_appSettings.APIBaseUrl}/code";
            var apiUri = QueryHelpers.AddQueryString(apiBaseUrl, parameters);

            //make request
            var responseMessage = await httpClient.GetAsync(apiUri);

            if (responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.ReasonPhrase == "NoContent")
                {
                    return null;
                }

                //cast to item
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var code = JsonConvert.DeserializeObject<Code>(responseString.ToString());

                return code;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> UpdateCode(Code code)
        {
            //setup HttpClient with content
            var httpClient = GetHttpClient();

            //construct full API endpoint uri
            DateTimeFormatInfo dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            var parameters = new Dictionary<string, string>
            {
                { "PromoCode", code.PromoCode },
                { "Expiry", code.Expiry.ToString(dtfi) },
                { "Claimed", code.Claimed.ToString() },
                { "EventName", code.EventName },
                { "Owner", code.Owner },
            };
            var apiBaseUrl = $"{_appSettings.APIBaseUrl}/code";
            var apiUri = QueryHelpers.AddQueryString(apiBaseUrl, parameters);

            //make request
            var responseMessage = await httpClient.PutAsync(apiUri, null);

            return responseMessage.IsSuccessStatusCode;
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            var apiUrlBase = _appSettings.APIBaseUrl;
            httpClient.BaseAddress = new Uri(apiUrlBase);
            return httpClient;
        }
    }
}
