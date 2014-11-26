using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebApiSecurity.Controllers
{
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
        
        public IHttpActionResult Get()
        {
            LogUser();
            return Ok(_animals);
        }

        public IHttpActionResult Put(string animal)
        {
            LogUser();
            _animals.Add(animal);
            return Ok(animal);
        }

        public IHttpActionResult Delete(string animal)
        {
            LogUser();
            _animals.Remove(animal);
            return Ok(animal);
        }

        private void LogUser()
        {
            var msg = "--------------" + Environment.NewLine;

            if (User != null && User.Identity != null)
            {
                msg += "User: " + User.Identity.Name + Environment.NewLine;
                msg += "AuthType: " + User.Identity.AuthenticationType + Environment.NewLine;
            }
            else
            {
                msg += "Not Authenticated" + Environment.NewLine;
            }
            msg += "--------------" + Environment.NewLine;
            System.Diagnostics.Debugger.Log(0, GetType().Name, msg);
            Console.WriteLine(msg);
        }

    }
}
