using Crypto_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crypto_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly BitfinexService bitfinexService;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory, BitfinexService bitfinexService)
        {
            this._logger = logger;
            this._clientFactory = clientFactory;
            this.bitfinexService = bitfinexService;
        }

        public async Task<ActionResult> Index()
        {
            var model = await bitfinexService.GetBitcoinDataAsyncForTheDay(0);
            return View(model);
        }

        public async Task<IActionResult> GetDayInfo(int dayChange = 0)
        {
            var data = await bitfinexService.GetBitcoinDataAsyncForTheDay(dayChange);

            var viewModel = new LabelPricePairInfoViewModel
            {
                Labels = data.Select(m => m.Date.ToString("HH:mm")),
                Prices = data.Select(m => m.Close ?? 0)
            };

            return Ok(viewModel);
        }

        public async Task<IActionResult> GetWeeklyInfo()
        {
            var data = await bitfinexService.GetWeeklyBitcoinDataAsync();

            var viewModel = new LabelPricePairInfoViewModel
            {
                Labels = data.Select(m => m.Date.ToString("yyyy-MM-dd")),
                Prices = data.Select(m => m.Close ?? 0)
            };

            return Ok(viewModel);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
