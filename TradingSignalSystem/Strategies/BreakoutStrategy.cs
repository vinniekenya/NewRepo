using TradingSignalSystem.Models;

namespace TradingSignalSystem.Strategies
{
    public class BreakoutStrategy : ITradingStrategy
    {
        public TradingSignal GenerateSignal(List<MarketData> data)
        {
            if (data == null || data.Count < 20)
            {
                return new TradingSignal
                {
                    Signal = "HOLD",
                    Strategy = "Breakout Strategy",
                    Entry = 0,
                    StopLoss = 0,
                    TakeProfit = 0,
                    Confidence = 0,
                    Explanation = "Not enough data"
                };
            }

            double currentPrice = data.Last().Close;

            // 🔥 Real resistance and support from last 20 candles
            double resistance = data.TakeLast(20).Max(x => x.High);
            double support = data.TakeLast(20).Min(x => x.Low);

            string signal = "HOLD";
            int confidence = 40;
            string explanation = "";
            double stopLoss = currentPrice;
            double takeProfit = currentPrice;

            if (currentPrice >= resistance * 0.999)
            {
                signal = "BUY";
                confidence = 75;
                stopLoss = support;
                takeProfit = currentPrice + (currentPrice - stopLoss) * 2;
                explanation = $"Price broke resistance at {resistance:F4}";
            }
            else if (currentPrice <= support * 1.001)
            {
                signal = "SELL";
                confidence = 75;
                stopLoss = resistance;
                takeProfit = currentPrice - (stopLoss - currentPrice) * 2;
                explanation = $"Price broke support at {support:F4}";
            }
            else
            {
                explanation = $"No breakout | Support: {support:F4} | Resistance: {resistance:F4}";
            }

            return new TradingSignal
            {
                Signal = signal,
                Strategy = "Breakout Strategy",
                Entry = currentPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                Confidence = confidence,
                Explanation = explanation
            };
        }
    }
}