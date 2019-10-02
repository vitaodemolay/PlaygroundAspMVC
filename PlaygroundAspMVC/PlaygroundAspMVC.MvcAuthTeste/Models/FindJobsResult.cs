using System.Collections.Generic;

namespace PlaygroundAspMVC.MvcAuthTeste.Models
{
    public class FindJobsResult
    {
        public IList<JobOpportunity> data { get; set; }

        public FindJobsResult(IList<JobOpportunity> data)
        {
            this.data = data;
        }
    }
}