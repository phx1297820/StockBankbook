using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;
using Microsoft.SqlServer.Server;
using StockBankbook.Models;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Web.Mvc;
using System.Web.Security;

namespace StockBankbook.Models
{
    public class IndexDisplayModel
    {
        public  List<HeldStockList> HeldStockList { get; set; }
        public int NowBalance { get; set; }
        public int NowStockValue { get; set; }
        public int NowAccountValue { get; set; }
        public int TotalCost { get; set; }
        public string ROI { get; set; }

    }
}