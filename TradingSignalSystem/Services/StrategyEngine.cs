using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingSignalSystem.Models;
using TradingSignalSystem.Strategies;

namespace TradingSignalSystem.Services
{
    public class StrategyEngine
    {
        private readonly MarketDataService _marketService;

        // ✅ FREE pairs
        public static readonly List<(string from, string to, string name)> FreePairs = new()
        {
            ("USD", "JPY", "USDJPY")
        };

        // ✅ VIP pairs (includes free pairs too)
        public static readonly List<(string from, string to, string name)> VipPairs = new()
        {
            ("USD", "JPY", "USDJPY"),
            ("EUR", "USD", "EURUSD"),
            ("GBP", "USD", "GBPUSD"),
            ("AUD", "JPY", "AUDJPY"),
            ("AUD", "USD", "AUDUSD"),
            ("USD", "CAD", "USDCAD")
        };

        public StrategyEngine()
        {
            _marketService = new MarketDataService();
        }

        public async Task<MultiSignalResult> RunMultiStrategyAsync(
            ITradingStrategy strategy, bool isVip = false)
        {
            // ✅ Pick pairs based on user role
            var pairs = isVip ? VipPairs : FreePairs;

            var results = new Dictionary<string, TradingSignal>();

            var strategies = new List<ITradingStrategy>
            {
                new EmaRsiStrategy(),
                new BreakoutStrategy(),
                new MACDStrategy(),
                new BollingerBandsStrategy(),
                new ATRStrategy()
            };

            foreach (var pair in pairs)
            {
                var data = await _marketService.GetForexData(pair.from, pair.to);

                System.Diagnostics.Debug.WriteLine($"{pair.name} — Data count: {data?.Count ?? 0}");

                if (data == null || data.Count == 0)
                {
                    results[pair.name] = new TradingSignal
                    {
                        Signal = "HOLD",
                        Confidence = 0,
                        Explanation = "No data from API"
                    };
                    await Task.Delay(15000);
                    continue;
                }

                var signals = strategies.Select(s => s.GenerateSignal(data)).ToList();

                int buyVotes = signals.Count(x => x.Signal == "BUY");
                int sellVotes = signals.Count(x => x.Signal == "SELL");

                System.Diagnostics.Debug.WriteLine($"{pair.name} — BUY: {buyVotes} | SELL: {sellVotes}");

                string finalSignal;
                int confidence;
                string explanation;

                if (buyVotes >= 2)
                {
                    finalSignal = "BUY";
                    confidence = Math.Min(50 + (buyVotes * 10), 100);
                    explanation = $"BUY confirmed by {buyVotes}/5 strategies";
                }
                else if (sellVotes >= 2)
                {
                    finalSignal = "SELL";
                    confidence = Math.Min(50 + (sellVotes * 10), 100);
                    explanation = $"SELL confirmed by {sellVotes}/5 strategies";
                }
                else if (buyVotes > sellVotes)
                {
                    finalSignal = "BUY";
                    confidence = 45;
                    explanation = $"Weak BUY | BUY: {buyVotes} SELL: {sellVotes}";
                }
                else if (sellVotes > buyVotes)
                {
                    finalSignal = "SELL";
                    confidence = 45;
                    explanation = $"Weak SELL | BUY: {buyVotes} SELL: {sellVotes}";
                }
                else
                {
                    finalSignal = "HOLD";
                    confidence = 30;
                    explanation = $"No consensus | BUY: {buyVotes} SELL: {sellVotes}";
                }

                var atrSignal = signals.FirstOrDefault(x => x.Strategy == "ATR");
                double entry = signals.First().Entry;

                results[pair.name] = new TradingSignal
                {
                    Signal = finalSignal,
                    Confidence = confidence,
                    Entry = entry,
                    StopLoss = atrSignal?.StopLoss ?? signals.First().StopLoss,
                    TakeProfit = atrSignal?.TakeProfit ?? signals.First().TakeProfit,
                    Explanation = explanation
                };

                await Task.Delay(15000);
            }

            var bestEntry = results
                .Where(x => x.Value?.Signal != "HOLD")
                .OrderByDescending(x => x.Value?.Confidence)
                .FirstOrDefault();

            return new MultiSignalResult
            {
                BestPair = bestEntry.Key ?? null,
                BestSignal = bestEntry.Value ?? null,
                AllSignals = results
            };
        }
    }
}