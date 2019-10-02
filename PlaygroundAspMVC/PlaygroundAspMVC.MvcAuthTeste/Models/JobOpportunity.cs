using Newtonsoft.Json;
using System;

namespace PlaygroundAspMVC.MvcAuthTeste.Models
{
    public class JobOpportunity
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("descricao")]
        public string Description { get; set; }

        [JsonProperty("publicacao")]
        public DateTime Publication { get; set; }
    }
}