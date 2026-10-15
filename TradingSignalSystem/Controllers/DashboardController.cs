using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TradingSignalSystem.Models;
using TradingSignalSystem.Services;
using TradingSignalSystem.Strategies;

namespace TradingSignalSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly NewsService _newsService;

        public DashboardController(NewsService newsService)
        {
            _newsService = newsService;
        }

        public async Task<IActionResult> Index()
        {
            //  Block access if not logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
            {
                return RedirectToAction("Login", "Account");
            }

            //  Check if user is VIP or Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            bool isVip = userRole == "VIP" || userRole == "Admin";

            var engine = new StrategyEngine();
            var strategy = new EmaRsiStrategy();

            //  Pass isVip to engine
            var result = await engine.RunMultiStrategyAsync(strategy, isVip);
            var news = await _newsService.GetForexNews();

            var pairScores = new Dictionary<string, int>();
            var finalSignals = new List<TradingSignalSystem.Models.SignalModel>();
            bool blockTrade = false;

            if (result?.AllSignals != null)
            {
                foreach (var pair in result.AllSignals.Keys)
                {
                    var signalData = result.AllSignals[pair];
                    int newsScore = news != null
                        ? _newsService.GetPairSpecificScore(news, pair)
                        : 0;

                    pairScores[pair] = newsScore;
                    string finalSignal = signalData.Signal ?? "HOLD";
                    int confidence = signalData.Confidence;

                    if (newsScore >= 8)
                    {
                        finalSignal = "BLOCKED";
                        confidence = 0;
                        blockTrade = true;
                    }
                    else if (newsScore >= 5) confidence -= 20;
                    else confidence += 10;

                    if (confidence < 0) confidence = 0;
                    if (confidence > 100) confidence = 100;

                    finalSignals.Add(new TradingSignalSystem.Models.SignalModel
                    {
                        Pair = pair,
                        Signal = finalSignal,
                        Confidence = confidence,
                        Entry = (decimal)signalData.Entry,
                        StopLoss = (decimal)signalData.StopLoss,
                        TakeProfit = (decimal)signalData.TakeProfit,
                        Explanation = signalData.Explanation ?? ""
                    });
                }
            }

            var bestTrade = finalSignals
                .Where(x => x.Signal != "BLOCKED" && x.Signal != "HOLD")
                .OrderByDescending(x => x.Confidence)
                .FirstOrDefault();

            var model = new DashboardViewModel
            {
                Signals = finalSignals,
                BestSignal = bestTrade,
                PairScores = pairScores,
                News = news,
                BlockTrade = blockTrade,
                IsVip = isVip  // ✅ pass to view
            };

            return View(model);
        }
    }
}