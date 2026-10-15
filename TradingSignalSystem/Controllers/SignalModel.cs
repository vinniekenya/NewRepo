namespace TradingSignalSystem.Controllers
{
    internal class SignalModel
    {
        public string Pair { get; set; }
        public string Signal { get; set; }
        public int Confidence { get; set; }
        public decimal Entry { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public string Explanation { get; set; }
    }
}