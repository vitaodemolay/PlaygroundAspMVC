using PlaygroundAspMVC.MvcAuthTeste.Config.Fakers;
using PlaygroundAspMVC.MvcAuthTeste.Models;
using System.Collections.Generic;
using System.Web.Http;

namespace PlaygroundAspMVC.MvcAuthTeste.Controllers.Apis
{
    public class JobOpportunitController : ApiController
    {
        
        [HttpGet]
        [Route("api/JobOpportunity/findRecentJobs/")]
        public IEnumerable<JobOpportunity> findRecentJobs()
        {
            return JobOpportunitFaker.CreateJob(100);
        }


        [HttpGet]
        [Route("api/JobOpportunity/findJobs/{query}")]
        public IEnumerable<JobOpportunity> findRecentJobs(string query)
        {
            return JobOpportunitFaker.CreateJob(150);
        }
    }
}
