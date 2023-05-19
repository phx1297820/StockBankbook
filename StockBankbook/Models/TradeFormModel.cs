using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockBankbook.Models
{
    public class TradeFormModel
    {
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public System.DateTime Date { get; set; }
        public bool IO { get; set; }
    }
}
