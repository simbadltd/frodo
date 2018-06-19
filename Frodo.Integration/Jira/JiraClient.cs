using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Frodo.Common;
using Frodo.Core;
using Frodo.Integration.Jira.JiraModel;
using NodaTime;
using RestSharp;
using RestSharp.Authenticators;
using Version = Frodo.Integration.Jira.JiraModel.Version;

namespace Frodo.Integration.Jira
{
    /// <summary>
    /// Class used for all interaction with the Jira API. See 
    /// http://docs.atlassian.com/jira/REST/latest/ for documentation of the
    /// Jira API.
    /// </summary>
    //todo[kk]: extract Tempo client from JiraClient
    public class JiraClient
    {
        private readonly RestClient _client;
        private readonly string _token;

        /// <summary>
        /// Constructs a JiraClient.
        /// </summary>
        /// <param name="account">Jira account information</param>
        public JiraClient(JiraAccount account)
        {
            _client = new RestClient(account.ServerUrl)
            {
                Authenticator = new HttpBasicAuthenticator(account.User, account.Password)
            };

            _token = account.TempoApiToken;
        }
	    /// <summary>
	    /// Throws exception with details if request was not unsucessful
	    /// </summary>
	    /// <param name="response"></param>
	    private static void ValidateResponse(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed || response.ErrorException != null || response.StatusCode == HttpStatusCode.BadRequest)
                throw new JiraApiException(
	                $"RestSharp response status: {response.ResponseStatus} - HTTP response: {response.StatusCode} - {response.StatusDescription} - {response.Content}");
        }

        public bool AddTempoWorklog(TempoWorklog tempoWorklog)
        {
            var request = new RestRequest
            {
                Resource = $"{ResourceUrls.TempoWorklog()}",
                Method = Method.POST,
                RequestFormat = DataFormat.Json,
            };

            request.AddHeader("Authorization", $"Bearer {_token}");
            request.AddBody(tempoWorklog);

            // No response expected
            var response = _client.Execute(request);

            ValidateResponse(response);

            return response.StatusCode == HttpStatusCode.OK;
        }

        /*public bool AddWorklog(string issuekey, TimeEntry timeEntry)
        {
            var request = new RestRequest
            {
                Resource = $"{ResourceUrls.IssueWorklog(issuekey)}",
                Method = Method.POST,
                RequestFormat = DataFormat.Json,
            };

            var worklog = new Worklog
            {
                comment = timeEntry.Description,
                started = ToIso8601String(timeEntry.Start),
                timeSpentSeconds = (int) Math.Round(timeEntry.Duration.TotalSeconds, MidpointRounding.AwayFromZero),
            };

            request.AddBody(worklog);

            // No response expected
            var response = _client.Execute(request);

            ValidateResponse(response);

            return response.StatusCode == HttpStatusCode.Created;
        }*/
    }
}
