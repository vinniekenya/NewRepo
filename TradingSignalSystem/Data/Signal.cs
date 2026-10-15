namespace TradingSignalsApp.Models
{
    public class Signal
    {
        public int Id { get; set; }


        public string? SignalType { get; set; }
        public string? Explanation { get; set; }

        public double Entry { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }

        public int Confidence { get; set; }
    }
}