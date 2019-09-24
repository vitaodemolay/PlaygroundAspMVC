using System;

namespace PubliAX.Web.Models.Order
{
    public class PedidoProducaoViewModel
    {
        public Int64 ORCAMENTO { get; set; }
        public Int64 LANCAMENTO { get; set; }
        public Int64 COD_FORNEC { get; set; }
        public double VALOR { get; set; }
        public DateTime VENCIMENTO { get; set; }
        public int linenumber { get; set; }
        public string status { get; set; }
        public string description { get; set; }
    }
}