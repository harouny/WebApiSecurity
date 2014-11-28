using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public void ClientCanGetTokenFromApi()
        {
            using (WebApp.Start<Startup>(BaseAddress))
            {
                var response = RequestAccessToken(userName: "user", password: "user");
                response.LogToConsole();
                Assert.IsTrue(response.IsSuccessStatusCode, "Get Token Request Failed");
                var token = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                Assert.IsTrue(token["access_token"] != null && token["access_token"].Value<string>().Length > 20, "access_token is missing");
                Assert.IsTrue(token["token_type"] != null && token["token_type"].Value<string>() == "bearer", "token_type is missing or invalid");
                Assert.IsTrue(token["expires_in"] != null && token["expires_in"].Value<int>() > 0, "expires_in is missing");
            } 
        }

        [TestMethod]
        public void ClientCanAuthinticateWithToken()
        {
            using (WebApp.Start<Startup>(BaseAddress))
            {
                var tokenResponse = RequestAccessToken(userName: "user", password: "user");
                var tokenObject = JObject.Parse(tokenResponse.Content.ReadAsStringAsync().Result);
                var accessTokn = tokenObject["access_token"].Value<string>();
                var tokenType = tokenObject["token_type"].Value<string>();
                using (var client = new HttpClient
                {
                    BaseAddress = new Uri(BaseAddress)
                })
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessTokn);
                    var response = client.PutAsJsonAsync("api/animals", "donkey").Result;
                    Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
                }
            }
        }

        [TestMethod]
        public void ApiReturnsUnauthorizedIfTokenIsInvalid()
        {
            using (WebApp.Start<Startup>(BaseAddress))
            {
                using (var client = new HttpClient
                {
                    BaseAddress = new Uri(BaseAddress)
                })
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", "invalid token");
                    var response = client.PutAsJsonAsync("api/animals", "donkey").Result;
                    Assert.AreEqual(response.StatusCode, HttpStatusCode.Unauthorized);
                }
            }
        }

        [TestMethod]
        public void ApiReturnsUnauthorizedIfTokenIsNotProvided()
        {
            using (WebApp.Start<Startup>(BaseAddress))
            {
                using (var client = new HttpClient
                {
                    BaseAddress = new Uri(BaseAddress)
                })
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.PutAsJsonAsync("api/animals", "donkey").Result;
                    Assert.AreEqual(response.StatusCode, HttpStatusCode.Unauthorized);
                }
            }
        }


        private static HttpResponseMessage RequestAccessToken(string userName, string password)
        {
            using (var client = new HttpClient
            {
                BaseAddress = new Uri(BaseAddress)
            })
            {
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", userName),
                    new KeyValuePair<string, string>("Password", password)
                };
                var content = new FormUrlEncodedContent(values);
                return client.PostAsync("token", content).Result;
            }
        }


    }
}
