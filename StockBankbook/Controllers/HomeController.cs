using Microsoft.Ajax.Utilities;
using Microsoft.SqlServer.Server;
using StockBankbook.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using System.Web.Security;
using System.Xml.Linq;

namespace StockBankbook.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        StockBankbookDBEntities db = new StockBankbookDBEntities();

        //首頁------------------------------------------------------------------------------------
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index_GetData()//取出目前持有股票的所有代碼
        {
            var data = new IndexDisplayModel();
            if (db.HeldStockList.Count() != 0)
            {
                data.HeldStockList = db.HeldStockList.ToList();
                data.NowStockValue = Convert.ToInt32(db.HeldStockList.Sum(x => x.AverageCost * x.Quantity));
            }
            if (db.Bankbook.Count() != 0)
            {
                data.NowBalance = db.Bankbook.OrderByDescending(x => x.Id).FirstOrDefault().Balance;
                data.TotalCost = db.Bankbook.Where(x => x.Reason.Contains("[存提款]") && x.Price > 0).Sum(x => x.Price);
            }
            data.NowAccountValue = data.NowBalance + data.NowStockValue;
            data.ROI = Math.Round(Convert.ToDouble((data.NowAccountValue - data.TotalCost)) / data.TotalCost, 4).ToString("P2");
            return Json(data);
        }

        [HttpPost]
        public ActionResult Index_SaveStockList(string[] tableData)
        {
            for (var i = 0; i < tableData.Length; i += 2)
            {
                var StockSymbol = tableData[i];
                var StockName = tableData[i + 1];
                db.HeldStockList.Where(x => x.Symbol == StockSymbol).FirstOrDefault().Name = StockName;
            }
            db.SaveChanges();
            return Json(new { success = true });
        }
        //登入------------------------------------------------------------------------------------

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string UserId, string Password)
        {
            //驗證帳密   
            if (UserId != "12345" || Password != "12345")
            {
                ViewBag.Message = "帳號or密碼錯誤，請重新確認登入";
                return View();
            }

            Session["Welcome"] = $"{UserId} 您好";

            FormsAuthentication.RedirectFromLoginPage(UserId, true);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }

        //股票交易------------------------------------------------------------------------------------

        [HttpGet]
        public ActionResult Trade()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Trade(TradeFormModel data)
        {
            var OldStock = db.HeldStockList.Where(x => x.Symbol == data.Symbol).FirstOrDefault();//檢查目前的持有股票
            var NewStock = new HeldStockList();//建立一支新股票
            var NewTrade = new Bankbook();//建立一筆交易紀錄
            int LastBalance = (db.Bankbook.Count() > 0) ?
                              db.Bankbook.Where(x => x.Id == db.Bankbook.Count()).FirstOrDefault().Balance : 0;
            //檢查目前是否持有此股票
            if (OldStock == null)
            {
                if (data.IO == true)//未持有，購入
                {
                    //(HeldStockList)新增一支新的股票
                    NewStock.Symbol = data.Symbol;
                    NewStock.Name = "尚未輸入";
                    NewStock.Quantity = data.Quantity;
                    NewStock.AverageCost = data.Price;
                    db.HeldStockList.Add(NewStock);
                    //(Bankbook)新增一筆扣款紀錄
                    NewTrade.Id = db.Bankbook.Count() + 1;
                    NewTrade.Reason = "[股票買賣] " + data.Symbol + " (買入) " + data.Quantity.ToString() + "股";
                    NewTrade.Price = -1 * Convert.ToInt32(data.Quantity * data.Price);
                    NewTrade.Balance = LastBalance + NewTrade.Price;
                    NewTrade.Date = DateTime.Today;
                    if (NewTrade.Balance < 0)
                    {
                        ViewBag.Message = "前筆交易失敗，餘額不足，請重新確認";
                        return View();
                    }
                    else
                    {
                        db.Bankbook.Add(NewTrade);
                    }
                }
                else//未持有，售出=>交易失敗
                {
                    ViewBag.Message = "前筆交易失敗，持有股票不足，請重新確認";
                    return View();
                }
            }
            else
            {
                if (data.IO == true)//已持有，購入
                {
                    //(HeldStockList)增加原有的股票
                    OldStock.AverageCost = (OldStock.AverageCost * OldStock.Quantity + data.Price * data.Quantity) /
                                           (OldStock.Quantity + data.Quantity);
                    OldStock.Quantity += data.Quantity;
                    //(Bankbook)新增一筆扣款紀錄
                    NewTrade.Id = db.Bankbook.Count() + 1;
                    NewTrade.Reason = "[股票買賣] " + data.Symbol + " (買入) " + data.Quantity + "股";
                    NewTrade.Price = -1 * Convert.ToInt32(data.Quantity * data.Price);
                    NewTrade.Balance = LastBalance + NewTrade.Price;
                    NewTrade.Date = DateTime.Today;
                    if (NewTrade.Balance < 0)
                    {
                        ViewBag.Message = "前筆交易失敗，餘額不足，請重新確認";
                        return View();
                    }
                    else
                    {
                        db.Bankbook.Add(NewTrade);
                    }
                }
                else//已持有，售出
                {
                    //(HeldStockList)減少原有的股票
                    if (OldStock.Quantity > data.Quantity)//持有股票足夠，且還有剩
                    {
                        OldStock.Quantity -= data.Quantity;
                    }
                    else if (OldStock.Quantity == data.Quantity)//持有股票足夠，且已賣完
                    {
                        db.HeldStockList.Remove(OldStock);
                    }
                    else//持有股票不足=>交易失敗
                    {
                        ViewBag.Message = "前筆交易失敗，持有股票不足，請重新確認";
                        return View();
                    }

                    //(Bankbook)新增一筆入款紀錄
                    NewTrade.Id = db.Bankbook.Count() + 1;
                    NewTrade.Reason = "[股票買賣] " + data.Symbol + " (賣出) " + data.Quantity + "股";
                    NewTrade.Price = Convert.ToInt32(data.Quantity * data.Price);
                    NewTrade.Balance = LastBalance + NewTrade.Price;
                    NewTrade.Date = DateTime.Today;
                    db.Bankbook.Add(NewTrade);
                }
            }
            db.SaveChanges();
            return View();
        }

        //現金交易------------------------------------------------------------------------------------

        [HttpGet]
        public ActionResult Deposit()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Deposit(DepositFormModel data)
        {
            Bankbook NewTrade = new Bankbook();
            int LastBalance = (db.Bankbook.Count() > 0) ?
                              db.Bankbook.Where(x => x.Id == db.Bankbook.Count()).FirstOrDefault().Balance : 0;
            if (data.IO == true)
            {
                NewTrade.Id = db.Bankbook.Count() + 1;
                NewTrade.Reason = "[" + data.Summary + "] " + data.Stock + data.Detail + " (存入) ";
                NewTrade.Price = data.Price;
                NewTrade.Balance = LastBalance + data.Price;
                NewTrade.Date = DateTime.Today;
            }
            else
            {
                NewTrade.Id = db.Bankbook.Count() + 1;
                NewTrade.Reason = "[" + data.Summary + "] " + data.Stock + data.Detail + " (領出) ";
                NewTrade.Price = -1 * data.Price;
                NewTrade.Balance = LastBalance + data.Price;
                NewTrade.Date = DateTime.Today;
            }

            if (NewTrade.Balance < 0)
            {
                ViewBag.Message = "前筆交易失敗，餘額不足，請重新確認";
                return View();
            }
            else
            {
                db.Bankbook.Add(NewTrade);
                db.SaveChanges();
                return View();
            }
        }

        [HttpGet]
        public ActionResult Deposit_GetStock()//取出目前持有股票的所有代碼
        {
            List<string> data = db.HeldStockList.Select(x => x.Symbol.Trim()).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //歷史紀錄---------------------------------------------------------------------------------------

        [HttpGet]
        public ActionResult History()
        {
            return View();
        }

        [HttpPost]
        public ActionResult History_GetHistory()
        {
            List<Bankbook> data = db.Bankbook.OrderByDescending(x => x.Id).ToList();
            return Json(data);
        }

        [HttpPost]
        public ActionResult Hostory_SaveHistory(string result)
        {
            var deleteLogId = Convert.ToInt32(result);
            var deleteData = db.Bankbook.Where(x => x.Id == deleteLogId).FirstOrDefault();
            //要考慮刪除的是不是股票交易紀錄，如果是，要修改HeldStockList
            if (deleteData.Reason.Contains("[股票買賣]"))
            {
                var firstSymbolWord = deleteData.Reason.IndexOf(']') + 2;
                var lastSymbolWord = deleteData.Reason.IndexOf('(') - 2;
                var SymbolString = deleteData.Reason.Substring(firstSymbolWord, lastSymbolWord - firstSymbolWord + 1);
                var firstQualityWord = deleteData.Reason.IndexOf(')') + 2;
                var lastQualityWord = deleteData.Reason.LastIndexOf('股') - 1;
                var QualityString = deleteData.Reason.Substring(firstQualityWord, lastQualityWord - firstQualityWord + 1);
                if (deleteData.Price > 0)//刪除一筆入帳(售出股票)的交易紀錄
                {
                    var ExitStatus = false;
                    foreach (var HeldStock in db.HeldStockList)
                    {
                        if (HeldStock.Symbol.Trim() == SymbolString)//還有剩餘的
                        {
                            ExitStatus = true;
                            break;
                        }
                    }
                    if (ExitStatus == true)
                    {
                        db.HeldStockList.Where(x => x.Symbol.Trim() == SymbolString).FirstOrDefault().Quantity
                        += Convert.ToInt32(QualityString);
                    }
                    else
                    {
                        var originalStock = new HeldStockList();
                        originalStock.Symbol = SymbolString;
                        originalStock.Name = "尚未輸入";
                        originalStock.Quantity = Convert.ToInt32(QualityString);
                        originalStock.AverageCost = deleteData.Price / Convert.ToInt32(QualityString);
                        db.HeldStockList.Add(originalStock);
                    }
                }
                else if (deleteData.Price < 0)//刪除一筆出帳(購入股票)的交易紀錄
                {
                    foreach (var HeldStock in db.HeldStockList)
                    {
                        if (HeldStock.Symbol.Trim() == SymbolString)
                        {
                            var originalTotalCost = HeldStock.AverageCost * HeldStock.Quantity;
                            HeldStock.Quantity -= Convert.ToInt32(QualityString);
                            if (HeldStock.Quantity <= 0)
                            {
                                db.HeldStockList.Remove(HeldStock);
                            }
                            HeldStock.AverageCost = (originalTotalCost - deleteData.Price) / HeldStock.Quantity;
                            break;
                        }
                    }
                }
            }
            db.Bankbook.Remove(deleteData);
            //重製後面的資料
            var StartIndex = Convert.ToInt32(result) + 1;
            var LastIndex = db.Bankbook.OrderByDescending(x => x.Id).FirstOrDefault().Id;
            if (StartIndex <= LastIndex)
            {
                for (var i = StartIndex; i <= LastIndex; i++)
                {
                    var originalData = db.Bankbook.Where(x => x.Id == i).FirstOrDefault();
                    var resetData = new Bankbook();
                    var previousData = db.Bankbook.Where(x => x.Id == i - 2).FirstOrDefault();
                    resetData.Id = originalData.Id - 1;
                    resetData.Reason = originalData.Reason;
                    resetData.Date = originalData.Date;
                    resetData.Price = originalData.Price;
                    resetData.Balance = (previousData.Balance + originalData.Price);
                    db.Bankbook.Add(resetData);
                    db.Bankbook.Remove(originalData);
                }
            }
            db.SaveChanges();
            //return Json(SymbolString);
            return Json(new { success = true });
        }

        public ActionResult Initial()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Initial(FormCollection data)
        {
            //清空舊資料
            db.Bankbook.RemoveRange(db.Bankbook);
            db.HeldStockList.RemoveRange(db.HeldStockList);
            //抓回傳值
            var Balance = Convert.ToInt32(data["Balance"]);
            var Cost = Convert.ToInt32(data["Cost"]);
            var StockSymbolList = data.GetValues("StockSymbol");
            var StockNameList = data.GetValues("StockName");
            var StockQuantityList = data.GetValues("StockQuantity");
            var StockTotalCostList = data.GetValues("StockTotalCost");
            //設定初始值
            var InitialSaving = new Bankbook();//給定初始成本
            InitialSaving.Id = 1;
            InitialSaving.Date = DateTime.Today;
            InitialSaving.Reason = "[存提款] (存入)";
            InitialSaving.Price = Cost;
            InitialSaving.Balance = 0 + InitialSaving.Price;
            db.Bankbook.Add(InitialSaving);
            var InitialCost = new Bankbook();//給定初始花費
            InitialCost.Id = 2;
            InitialCost.Date = DateTime.Today;
            InitialCost.Reason = "[其他] 初始花費";
            InitialCost.Price = Balance - Cost;
            InitialCost.Balance = Balance;
            db.Bankbook.Add(InitialCost);
            for (var i = 0; i < StockSymbolList.Length; i++)//給定初始持股
            {
                if (StockSymbolList[i] != "" && StockNameList[i] != "")
                {
                    if (StockQuantityList[i] != "" && StockTotalCostList[i] != "")
                    {
                        var StockInfo = new HeldStockList();
                        StockInfo.Symbol = StockSymbolList[i];
                        StockInfo.Name = StockNameList[i];
                        StockInfo.Quantity = Convert.ToInt32(StockQuantityList[i]);
                        StockInfo.AverageCost = Convert.ToInt32(StockTotalCostList[i]) / StockInfo.Quantity;
                        db.HeldStockList.Add(StockInfo);
                    }
                }
            }
            db.SaveChanges();
            return View();
        }
    }
}