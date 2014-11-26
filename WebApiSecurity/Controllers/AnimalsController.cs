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
            return Ok(_animals);
        }

        public IHttpActionResult Put(string animal)
        {
            _animals.Add(animal);
            return Ok(animal);
        }

        public IHttpActionResult Delete(string animal)
        {
            _animals.Remove(animal);
            return Ok(animal);
        }

    }
}
