using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Web;
using Frodo.Common;
using Frodo.Core;
using NodaTime;
using RestSharp;
using RestSharp.Authenticators;

namespace Frodo.Integration.Toggl
{
    internal sealed class TogglClient
    {
        private const string ApiBaseUrl = "https://www.toggl.com/api/v8";

        private readonly RestClient _client;

        public TogglClient(User user)
        {
            _client = new RestClient(ApiBaseUrl)
            {
                Authenticator = new HttpBasicAuthenticator(user.TogglApiToken, "api_token")
            };
        }

        public List<TogglTimeEntry> GetTimeEntries(OffsetDateTime start, OffsetDateTime end)
        {
            var startIso8601Str = start.ToIso8601String();
            var endIso8601Str = end.ToIso8601String();

            var request = new RestRequest
            {
                Resource = $"{ApiBaseUrl}/time_entries?start_date={HttpUtility.UrlEncode(startIso8601Str)}&end_date={HttpUtility.UrlEncode(endIso8601Str)}",
                Method = Method.GET
            };

            return Execute<List<TogglTimeEntry>>(request, HttpStatusCode.OK);
        }

        /// <summary>
        /// Executes a RestRequest and returns the deserialized response. If
        /// the response hasn't got the specified expected response code or if an
        /// exception was thrown during execution a TogglApiException will be
        /// thrown.
        /// </summary>
        /// <typeparam name="T">Request return type</typeparam>
        /// <param name="request">request to execute</param>
        /// <param name="expectedResponseCode">The expected HTTP response code</param>
        /// <returns>deserialized response of request</returns>
        public T Execute<T>(RestRequest request, HttpStatusCode expectedResponseCode) where T : new()
        {
            // Won't throw exception.
            var response = _client.Execute<T>(request);

            ValidateResponse(response);

            return response.Data;
        }

        /// <summary>
        /// Throws exception with details if request was not unsucessful
        /// </summary>
        /// <param name="response"></param>
        private static void ValidateResponse(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed || response.ErrorException != null || response.StatusCode == HttpStatusCode.BadRequest)
                throw new TogglApiException(
                    $"RestSharp response status: {response.ResponseStatus} - HTTP response: {response.StatusCode} - {response.StatusDescription} - {response.Content}");
        }
    }
}