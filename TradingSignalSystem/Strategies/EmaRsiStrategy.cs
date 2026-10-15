using System.Linq;
using TradingSignalSystem.Models;

namespace TradingSignalSystem.Strategies
{
    public class EmaRsiStrategy : ITradingStrategy
    {
        public TradingSignal GenerateSignal(List<MarketData> data)
        {
            if (data == null || data.Count < 20)
            {
                return new TradingSignal
                {
                    Signal = "HOLD",
                    Strategy = "EMA + RSI",
                    Entry = 0,
                    StopLoss = 0,
                    TakeProfit = 0,
                    Confidence = 0,
                    Explanation = "Not enough data"
                };
            }

            var last = data.Last();
            double currentPrice = last.Close;

            // 🔥 REAL EMA (period = 14)
            double ema = CalculateEMA(data.Select(x => x.Close).ToList(), 14);

            // 🔥 REAL RSI (period = 14)
            double rsi = CalculateRSI(data.Select(x => x.Close).ToList(), 14);

            string signal = "HOLD";
            int confidence = 50;
            string explanation = "";

            // 📊 SIGNAL LOGIC — relaxed thresholds for intraday
            if (currentPrice > ema && rsi < 55)
            {
                signal = "BUY";
                confidence = 85;
                explanation = "Uptrend + RSI pullback";
            }
            else if (currentPrice < ema && rsi > 45)
            {
                signal = "SELL";
                confidence = 85;
                explanation = "Downtrend + RSI rejection";
            }
            else if (currentPrice > ema)
            {
                signal = "BUY";
                confidence = 65;
                explanation = "Price above EMA — weak buy";
            }
            else if (currentPrice < ema)
            {
                signal = "SELL";
                confidence = 65;
                explanation = "Price below EMA — weak sell";
            }
            else
            {
                signal = "HOLD";
                confidence = 40;
                explanation = $"No clear setup (RSI: {rsi:F2})";
            }

            // 🔥 Use real High/Low for SL & TP
            double recentHigh = data.TakeLast(10).Max(x => x.High);
            double recentLow = data.TakeLast(10).Min(x => x.Low);

            double stopLoss;
            double takeProfit;

            if (signal == "BUY")
            {
                stopLoss = recentLow - (currentPrice * 0.001);
                takeProfit = currentPrice + (currentPrice - stopLoss) * 2;
            }
            else if (signal == "SELL")
            {
                stopLoss = recentHigh + (currentPrice * 0.001);
                takeProfit = currentPrice - (stopLoss - currentPrice) * 2;
            }
            else
            {
                stopLoss = currentPrice;
                takeProfit = currentPrice;
            }

            return new TradingSignal
            {
                Signal = signal,
                Strategy = "EMA + RSI",
                Entry = currentPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                Confidence = confidence,
                Explanation = $"{explanation} | DataCount: {data.Count} | EMA: {ema:F4} | RSI: {rsi:F2}"
            };
        }

        // 🔥 EMA CALCULATION
        private double CalculateEMA(List<double> prices, int period)
        {
            double multiplier = 2.0 / (period + 1);
            double ema = prices.Take(period).Average();

            for (int i = period; i < prices.Count; i++)
            {
                ema = ((prices[i] - ema) * multiplier) + ema;
            }

            return ema;
        }

        // 🔥 RSI CALCULATION
        private double CalculateRSI(List<double> prices, int period)
        {
            double gain = 0;
            double loss = 0;

            for (int i = 1; i <= period; i++)
            {
                double change = prices[i] - prices[i - 1];
                if (change > 0) gain += change;
                else loss -= change;
            }

            gain /= period;
            loss /= period;

            if (loss == 0) return 100;

            double rs = gain / loss;
            double rsi = 100 - (100 / (1 + rs));

            return rsi;
        }
    }
}