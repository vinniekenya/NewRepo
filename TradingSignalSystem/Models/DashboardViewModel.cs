using System.Collections.Generic;

namespace TradingSignalSystem.Models
{
    public class DashboardViewModel
    {
        public Dictionary<string, TradingSignal>? AllSignals { get; set; }
        public SignalModel? BestSignal { get; set; }
        public string? BestPair { get; set; }
        public Dictionary<string, int>? PairScores { get; set; }
        public List<SignalModel>? Signals { get; set; }
        public dynamic? News { get; set; }
        public bool BlockTrade { get; set; }
        public bool IsVip { get; set; } // ✅ no ? needed for bool
    }

    public class SignalModel
    {
        public string? Pair { get; set; }        // ✅ nullable
        public string? Signal { get; set; }      // ✅ nullable
        public int Confidence { get; set; }
        public decimal Entry { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public string? Explanation { get; set; } // ✅ nullable
    }
}