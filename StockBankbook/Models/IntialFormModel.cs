using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockBankbook.Models
{
    public class IntialFormModel
    {
        public Bankbook Bankbook { get; set; }
        public List<HeldStockList> HeldStockList { get; set; }
    }
}