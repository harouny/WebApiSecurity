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
                Assert.IsTrue(response.IsSuccessStatusCode, "Get Token Request Failed");
                var token = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                Assert.IsTrue(token["access_token"] != null && token["access_token"].Value<string>().Length > 20, "access_token is missing");
                Assert.IsTrue(token["token_type"] != null && token["token_type"].Value<string>() == "bearer", "token_type is missing or invalid");
                Assert.IsTrue(token["expires_in"] != null && token["expires_in"].Value<int>() > 0, "expires_in is missing");
                Assert.IsTrue(token["refresh_token"] != null && token["refresh_token"].Value<string>().Length > 20, "refresh_token is missing");
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
                    var response = client.PutAsJsonAsync("api/animals", "donkey").Result.LogToConsole();
                    Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
                }
            }
        }



        [TestMethod]
        public void ClientCanRenewAccessTokenUsingRefreshTokens()
        {
            using (WebApp.Start<Startup>(BaseAddress))
            {
                var tokenResponse = RequestAccessToken(userName: "user", password: "user");
                var tokenObject = JObject.Parse(tokenResponse.Content.ReadAsStringAsync().Result);
                var accessToken = tokenObject["access_token"].Value<string>();
                var refreshToken = tokenObject["refresh_token"].Value<string>();
                using (var client = new HttpClient
                {
                    BaseAddress = new Uri(BaseAddress)
                })
                {
                    var values = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                        new KeyValuePair<string, string>("client_id", "secret"),
                        new KeyValuePair<string, string>("client_secret", "secret"),
                        new KeyValuePair<string, string>("refresh_token", refreshToken)
                    };
                    var content = new FormUrlEncodedContent(values);
                    var response = client.PostAsync("token", content).Result.LogToConsole();
                    Assert.IsTrue(response.IsSuccessStatusCode, "Renew Access Token Failed");
                    var token = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    Assert.IsTrue(token["access_token"] != null && token["access_token"].Value<string>().Length > 20, "access_token is missing");
                    Assert.IsTrue(token["token_type"] != null && token["token_type"].Value<string>() == "bearer", "token_type is missing or invalid");
                    Assert.IsTrue(token["expires_in"] != null && token["expires_in"].Value<int>() > 0, "expires_in is missing");
                    Assert.IsTrue(token["refresh_token"] != null && token["refresh_token"].Value<string>().Length > 20, "refresh_token is missing");
                    Assert.AreNotEqual(accessToken, token["access_token"].Value<string>());
                }
            }
        }



        [TestMethod]
        public void ApiReturnsErrorIfClientTriesToRenewTokenThatDoesNotBelongToHim()
        {
            using (WebApp.Start<Startup>(BaseAddress))
            {
                var tokenResponse = RequestAccessToken(userName: "user", password: "user");
                var tokenObject = JObject.Parse(tokenResponse.Content.ReadAsStringAsync().Result);
                var refreshToken = tokenObject["refresh_token"].Value<string>();
                using (var client = new HttpClient
                {
                    BaseAddress = new Uri(BaseAddress)
                })
                {
                    var values = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                        new KeyValuePair<string, string>("client_id", "secret2"),
                        new KeyValuePair<string, string>("client_secret", "secret2"),
                        new KeyValuePair<string, string>("refresh_token", refreshToken)
                    };
                    var content = new FormUrlEncodedContent(values);
                    var response = client.PostAsync("token", content).Result.LogToConsole();
                    Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest, "status code should be bad request");
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
                    var response = client.PutAsJsonAsync("api/animals", "donkey").Result.LogToConsole();
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
                    var response = client.PutAsJsonAsync("api/animals", "donkey").Result.LogToConsole();
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
                    new KeyValuePair<string, string>("Password", password),
                    new KeyValuePair<string, string>("client_id", "secret"),
                    new KeyValuePair<string, string>("client_secret", "secret")
                };
                var content = new FormUrlEncodedContent(values);
                return client.PostAsync("token", content).Result.LogToConsole();
            }
        }


    }
}
