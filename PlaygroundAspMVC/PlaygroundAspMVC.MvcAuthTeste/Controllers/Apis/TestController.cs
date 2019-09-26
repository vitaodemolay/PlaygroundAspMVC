using System.Collections.Generic;
using System.Web.Http;

namespace PlaygroundAspMVC.MvcAuthTeste.Controllers.Apis
{
    public class TestController : ApiController
    {
        [Authorize]
        [HttpGet]
        [Route("api/Test/auth/")]
        public IEnumerable<string> auth()
        {
            return new string[] { "value1 - new", "value2 - new" };
        }

        
          // GET: api/Test
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Test/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Test
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Test/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Test/5
        public void Delete(int id)
        {
        }
    }
}
