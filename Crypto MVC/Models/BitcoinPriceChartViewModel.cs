using Crypto_MVC.Models;
using System.Collections.Generic;

namespace Crypto_MVC.Models
{
    public class BitcoinPriceChartViewModel
    {
        public List<BitcoinPricePoint> PricePoints { get; set; } = new List<BitcoinPricePoint>();
    }
}