﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace WebApiSecurity.Controllers
{
    [RoutePrefix("api")]
    public class AnimalsController : ApiController
    {
        private static List<string> _animals;
        public AnimalsController()
        {
            if (_animals == default (List<string>))
            {
                _animals =  (new[] { "cat", "dog", "mouse" }).ToList();
            }
        }

        [Route("animals", Name = "animals")]
        public IHttpActionResult Get()
        {
            LogCurrentUser();
            return Ok(_animals);
        }

        [Route("animals")]
        [Authorize]
        public IHttpActionResult Put([FromBody]string animal)
        {
            LogCurrentUser();
            _animals.Add(animal);
            return Created(Url.Link("animal", new { animal }), animal);
        }

        [Route("animals")]
        [Authorize]
        public IHttpActionResult Delete([FromBody]string animal)
        {
            LogCurrentUser();
            _animals.Remove(animal);
            return Ok(animal);
        }

        [Route("animals/{animal}", Name = "animal")]
        public IHttpActionResult Get(string animal)
        {
            LogCurrentUser();
            return Ok(_animals.ToList().First(obj => obj == animal));
        }

        private void LogCurrentUser()
        {
            var message = new StringBuilder();
            message.AppendLine("--------------");
            if (User != null && User.Identity != null)
            {
                message.AppendLine("User: " + User.Identity.Name);
                message.AppendLine("AuthType: " + User.Identity.AuthenticationType);
            }
            else
            {
               message.AppendLine("Not Authenticated");
            }
           message.AppendLine("--------------");
            System.Diagnostics.Debugger.Log(0, GetType().Name, message.ToString());
            Console.WriteLine(message.ToString());
        }

    }
}
