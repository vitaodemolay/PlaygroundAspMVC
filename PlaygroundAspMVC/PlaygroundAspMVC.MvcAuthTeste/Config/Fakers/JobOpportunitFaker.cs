using Bogus;
using PlaygroundAspMVC.MvcAuthTeste.Models;
using System.Collections.Generic;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Fakers
{
    public static class JobOpportunitFaker
    {
        public static IList<JobOpportunity> CreateJob(int quantity)
        {
            var faker = new Faker<JobOpportunity>()
                .StrictMode(false)
                .Rules((f, c) =>
                {
                    f.Locale = "pt-BR";
                    c.Id = f.UniqueIndex;
                    c.Description = f.Lorem.Sentence(wordCount: 10);
                    c.Publication = f.Date.Recent();
                });

            return faker.Generate(quantity);
        }

    }
}