using System.Linq;
using TradingSignalSystem.Models;

namespace TradingSignalSystem.Strategies
{
    public class BollingerBandsStrategy : ITradingStrategy
    {
        public TradingSignal GenerateSignal(List<MarketData> data)
        {
            if (data == null || data.Count < 20)
            {
                return new TradingSignal
                {
                    Signal = "HOLD",
                    Strategy = "Bollinger Bands",
                    Entry = 0,
                    StopLoss = 0,
                    TakeProfit = 0,
                    Confidence = 0,
                    Explanation = "Not enough data for Bollinger Bands"
                };
            }

            var prices = data.TakeLast(20).Select(x => x.Close).ToList();
            double currentPrice = data.Last().Close;

            // 🔥 Middle Band = SMA 20
            double sma = prices.Average();

            // 🔥 Standard Deviation
            double stdDev = Math.Sqrt(prices.Average(p => Math.Pow(p - sma, 2)));

            // 🔥 Upper and Lower Bands
            double upperBand = sma + (2 * stdDev);
            double lowerBand = sma - (2 * stdDev);

            string signal = "HOLD";
            int confidence = 40;
            string explanation = "";
            double stopLoss;
            double takeProfit;

            if (currentPrice <= lowerBand)
            {
                signal = "BUY";
                confidence = 82;
                explanation = $"Price touched lower band — reversal expected | Lower: {lowerBand:F4} | Upper: {upperBand:F4}";
                stopLoss = lowerBand - (currentPrice * 0.001);
                takeProfit = sma; // target middle band
            }
            else if (currentPrice >= upperBand)
            {
                signal = "SELL";
                confidence = 82;
                explanation = $"Price touched upper band — reversal expected | Lower: {lowerBand:F4} | Upper: {upperBand:F4}";
                stopLoss = upperBand + (currentPrice * 0.001);
                takeProfit = sma; // target middle band
            }
            else if (currentPrice > sma)
            {
                signal = "BUY";
                confidence = 60;
                explanation = $"Price above middle band | SMA: {sma:F4}";
                stopLoss = lowerBand;
                takeProfit = upperBand;
            }
            else
            {
                signal = "SELL";
                confidence = 60;
                explanation = $"Price below middle band | SMA: {sma:F4}";
                stopLoss = upperBand;
                takeProfit = lowerBand;
            }

            return new TradingSignal
            {
                Signal = signal,
                Strategy = "Bollinger Bands",
                Entry = currentPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                Confidence = confidence,
                Explanation = explanation
            };
        }
    }
}