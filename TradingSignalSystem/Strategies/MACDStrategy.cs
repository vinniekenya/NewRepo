using System.Linq;
using TradingSignalSystem.Models;

namespace TradingSignalSystem.Strategies
{
    public class MACDStrategy : ITradingStrategy
    {
        public TradingSignal GenerateSignal(List<MarketData> data)
        {
            if (data == null || data.Count < 26)
            {
                return new TradingSignal
                {
                    Signal = "HOLD",
                    Strategy = "MACD",
                    Entry = 0,
                    StopLoss = 0,
                    TakeProfit = 0,
                    Confidence = 0,
                    Explanation = "Not enough data for MACD"
                };
            }

            var prices = data.Select(x => x.Close).ToList();
            double currentPrice = prices.Last();

            // 🔥 MACD = EMA12 - EMA26
            double ema12 = CalculateEMA(prices, 12);
            double ema26 = CalculateEMA(prices, 26);
            double macd = ema12 - ema26;

            // 🔥 Signal Line = EMA9 of MACD
            // Approximate using last 9 MACD values
            var macdValues = new List<double>();
            for (int i = 0; i < 9; i++)
            {
                var subset = prices.Take(prices.Count - i).ToList();
                double e12 = CalculateEMA(subset, 12);
                double e26 = CalculateEMA(subset, 26);
                macdValues.Insert(0, e12 - e26);
            }
            double signalLine = macdValues.Average();
            double histogram = macd - signalLine;

            string signal = "HOLD";
            int confidence = 40;
            string explanation = "";

            double recentHigh = data.TakeLast(10).Max(x => x.High);
            double recentLow = data.TakeLast(10).Min(x => x.Low);
            double stopLoss;
            double takeProfit;

            if (macd > signalLine && histogram > 0)
            {
                signal = "BUY";
                confidence = 80;
                explanation = $"MACD crossed above signal line | MACD: {macd:F5} | Signal: {signalLine:F5}";
                stopLoss = recentLow - (currentPrice * 0.001);
                takeProfit = currentPrice + (currentPrice - stopLoss) * 2;
            }
            else if (macd < signalLine && histogram < 0)
            {
                signal = "SELL";
                confidence = 80;
                explanation = $"MACD crossed below signal line | MACD: {macd:F5} | Signal: {signalLine:F5}";
                stopLoss = recentHigh + (currentPrice * 0.001);
                takeProfit = currentPrice - (stopLoss - currentPrice) * 2;
            }
            else
            {
                explanation = $"No MACD crossover | MACD: {macd:F5} | Signal: {signalLine:F5}";
                stopLoss = currentPrice;
                takeProfit = currentPrice;
            }

            return new TradingSignal
            {
                Signal = signal,
                Strategy = "MACD",
                Entry = currentPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                Confidence = confidence,
                Explanation = explanation
            };
        }

        private double CalculateEMA(List<double> prices, int period)
        {
            if (prices.Count < period) return prices.Average();
            double multiplier = 2.0 / (period + 1);
            double ema = prices.Take(period).Average();
            for (int i = period; i < prices.Count; i++)
                ema = ((prices[i] - ema) * multiplier) + ema;
            return ema;
        }
    }
}