﻿using LinqToTwitter;
using LinqToTwitter.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorDemo.Services
{
    public class BlazorAuthorizer : AspNetAuthorizer
    {
        public HttpClient httpClient;
        HttpContextAccessor httpContextAccessor;
        string authUrl;

        public BlazorAuthorizer(
            HttpClient httpClient,
            HttpContextAccessor httpContextAccessor)
        {
            this.httpClient = httpClient;
            this.httpContextAccessor = httpContextAccessor;
        }

        // StateChanged is an event handler other pages
        // can subscribe to 
        public event EventHandler StateChanged;
        public void StateHasChanged()
        {
            // This will update any subscribers
            // that the state has changed
            // so they can update themselves
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public string GetPathBase()
        {
            var request = httpContextAccessor.HttpContext.Request;
            var host = request.Host.ToUriComponent();
            var pathBase = request.PathBase.ToUriComponent();
            return $@"{request.Scheme}://{host}{pathBase}/";
        }

        public async Task<ActionResult> Begin()
        {
            this.CredentialStore = new InMemoryCredentialStore();
            this.CredentialStore.ConsumerKey = Environment.GetEnvironmentVariable("TwitterConsumerKey");
            this.CredentialStore.ConsumerSecret = Environment.GetEnvironmentVariable("TwitterConsumerSecret");

            // to pass parameters that you can read in Complete(), via Request.QueryString, when Twitter returns
            // var parameters = new Dictionary<string, string> { { "my_custom_param", "val" } };
            // return await BeginAuthorizationAsync(new Uri(GetPathBase()), parameters);

            return (RedirectResult)await BeginAuthorizationAsync(new Uri(GetPathBase()));
        }

        public async Task<ActionResult> BeginAuthorizationAsync(
            Uri callback, Dictionary<string, string> parameters = null)
        {
            if (GoToTwitterAuthorization == null)
                GoToTwitterAuthorization = authUrl => { this.authUrl = authUrl; };

            Callback = callback;

            await base.BeginAuthorizeAsync(callback, parameters).ConfigureAwait(false);

            return new RedirectResult(authUrl, false);
        }
    }
}
