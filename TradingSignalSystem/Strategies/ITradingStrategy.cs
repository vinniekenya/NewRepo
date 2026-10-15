using TradingSignalSystem.Models;

namespace TradingSignalSystem.Strategies
{
    public interface ITradingStrategy
    {
        TradingSignal GenerateSignal(List<MarketData> data);
    }
}