using System.Linq;
using TradingSignalSystem.Models;

namespace TradingSignalSystem.Strategies
{
    public class ATRStrategy : ITradingStrategy
    {
        public TradingSignal GenerateSignal(List<MarketData> data)
        {
            if (data == null || data.Count < 15)
            {
                return new TradingSignal
                {
                    Signal = "HOLD",
                    Strategy = "ATR",
                    Entry = 0,
                    StopLoss = 0,
                    TakeProfit = 0,
                    Confidence = 0,
                    Explanation = "Not enough data for ATR"
                };
            }

            double currentPrice = data.Last().Close;

            // 🔥 Calculate ATR (14 periods)
            double atr = CalculateATR(data, 14);

            // 🔥 EMA for trend direction
            var prices = data.Select(x => x.Close).ToList();
            double ema = CalculateEMA(prices, 14);

            string signal = "HOLD";
            int confidence = 40;
            string explanation = "";

            // 🔥 ATR-based SL & TP (professional volatility-based)
            double stopLoss;
            double takeProfit;

            if (currentPrice > ema)
            {
                signal = "BUY";
                confidence = 78;
                stopLoss = currentPrice - (atr * 1.5); // 1.5x ATR below entry
                takeProfit = currentPrice + (atr * 3.0); // 3x ATR above entry (1:2 RR)
                explanation = $"Uptrend | ATR-based SL/TP | ATR: {atr:F5}";
            }
            else
            {
                signal = "SELL";
                confidence = 78;
                stopLoss = currentPrice + (atr * 1.5); // 1.5x ATR above entry
                takeProfit = currentPrice - (atr * 3.0); // 3x ATR below entry (1:2 RR)
                explanation = $"Downtrend | ATR-based SL/TP | ATR: {atr:F5}";
            }

            return new TradingSignal
            {
                Signal = signal,
                Strategy = "ATR",
                Entry = currentPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                Confidence = confidence,
                Explanation = explanation
            };
        }

        // 🔥 ATR CALCULATION
        private double CalculateATR(List<MarketData> data, int period)
        {
            var trueRanges = new List<double>();

            for (int i = 1; i < data.Count; i++)
            {
                double highLow = data[i].High - data[i].Low;
                double highClose = Math.Abs(data[i].High - data[i - 1].Close);
                double lowClose = Math.Abs(data[i].Low - data[i - 1].Close);
                trueRanges.Add(Math.Max(highLow, Math.Max(highClose, lowClose)));
            }

            return trueRanges.TakeLast(period).Average();
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