namespace Crypto_MVC.Models
{
    public class LabelPricePairInfoViewModel
    {
        public IEnumerable<string> Labels { get; set; }
        public IEnumerable<decimal> Prices { get; set; }
    }
}
