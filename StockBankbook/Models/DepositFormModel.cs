using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockBankbook.Models
{
    public class DepositFormModel
    {
        public bool IO { get; set; }
        public string Summary { get; set; }
        public string Stock { get; set; }
        public string Detail { get; set; }
        public int Price { get; set; }
        public DateTime Date { get; set; }
    }
}