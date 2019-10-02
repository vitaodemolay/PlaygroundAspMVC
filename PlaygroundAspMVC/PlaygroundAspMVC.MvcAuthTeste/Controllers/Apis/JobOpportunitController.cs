using PlaygroundAspMVC.MvcAuthTeste.Config.Fakers;
using PlaygroundAspMVC.MvcAuthTeste.Models;
using System.Web.Http;

namespace PlaygroundAspMVC.MvcAuthTeste.Controllers.Apis
{
    public class JobOpportunitController : ApiController
    {

        [HttpGet]
        [Route("api/JobOpportunity/findRecentJobs/")]
        public FindJobsResult findRecentJobs()
        {
            return new FindJobsResult(JobOpportunitFaker.CreateJob(100));
        }


        [HttpGet]
        [Route("api/JobOpportunity/findJobs/{query}")]
        public FindJobsResult findRecentJobs(string query)
        {
            return new FindJobsResult(JobOpportunitFaker.CreateJob(150));
        }
    }
}
