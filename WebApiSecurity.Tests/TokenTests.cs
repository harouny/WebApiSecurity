using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace WebApiSecurity.Tests
{
    [TestClass]
    public class TokenTests
    {
        private const string BaseAddress = "http://localhost:9000/";

        [TestMethod]
        public void CanGetToken()
        {
            using (WebApp.Start<Startup>(BaseAddress))
            {
                var client = new HttpClient()
                {
                    BaseAddress = new Uri(BaseAddress)
                };
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>( "grant_type", "password" ), 
                    new KeyValuePair<string, string>( "username", "user" ), 
                    new KeyValuePair<string, string> ( "Password", "user" )
                };
                var content = new FormUrlEncodedContent(values);
                var response =
                    client.PostAsync("token", content).Result;
                response.LogToConsole();
                Assert.IsTrue(response.IsSuccessStatusCode, "Get Token Request Failed");
                var token = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                Assert.IsTrue(token["access_token"] != null && token["access_token"].Value<string>().Length > 20, "access_token is missing");
                Assert.IsTrue(token["token_type"] != null && token["token_type"].Value<string>() == "bearer", "token_type is missing or invalid");
                Assert.IsTrue(token["expires_in"] != null && token["expires_in"].Value<int>() > 0, "expires_in is missing");
            } 
        }
    }
}
