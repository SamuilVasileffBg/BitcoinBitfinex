using Crypto_MVC.Models;
using Microsoft.AspNetCore.Mvc;
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
            var model = await bitfinexService.GetBitcoinDataAsyncForTheDay(-733);
            return View(model);
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
