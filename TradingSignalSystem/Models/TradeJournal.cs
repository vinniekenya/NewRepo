namespace TradingSignalSystem.Models
{
    public class TradeJournal
    {
        public string Pair { get; set; }

        public string Signal { get; set; }

        public double EntryPrice { get; set; }

        public string Result { get; set; }
    }
}