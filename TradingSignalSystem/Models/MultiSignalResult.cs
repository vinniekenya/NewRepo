using System.Collections.Generic;

namespace TradingSignalSystem.Models
{
    public class MultiSignalResult
    {
        public string? BestPair { get; set; }
        public TradingSignal? BestSignal { get; set; }
        public Dictionary<string, TradingSignal>? AllSignals { get; set; }
    }
}