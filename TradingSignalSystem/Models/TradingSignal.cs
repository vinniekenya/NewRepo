namespace TradingSignalSystem.Models
{
    public class TradingSignal
    {
        public string Signal { get; set; }

        public string Strategy { get; set; }

        public string Explanation { get; set; }

        public double Entry { get; set; }

        public double StopLoss { get; set; }

        public double TakeProfit { get; set; }

        public int Confidence { get; set; }
    }
}