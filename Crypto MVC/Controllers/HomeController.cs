using Crypto_MVC.Data;
using Crypto_MVC.Data.Models;
using Crypto_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
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
        private readonly ApplicationDbContext context;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory, BitfinexService bitfinexService, ApplicationDbContext context)
        {
            this._logger = logger;
            this._clientFactory = clientFactory;
            this.bitfinexService = bitfinexService;
            this.context = context;
        }

        public async Task<ActionResult> Index()
        {
            var model = await bitfinexService.GetBitcoinDataAsyncForTheDay(0);
            return View(model);
        }

        public async Task<IActionResult> GetDayInfo(int dayChange = 0, bool euro = false)
        {
            var data = await bitfinexService.GetBitcoinDataAsyncForTheDay(dayChange, euro);

            var viewModel = new LabelPricePairInfoViewModel
            {
                Labels = data.Select(m => m.Date.ToString("HH:mm")),
                Prices = data.Select(m => m.Close ?? 0)
            };

            return Ok(viewModel);
        }

        public async Task<IActionResult> GetWeeklyInfo(bool euro = false)
        {
            var data = await bitfinexService.GetWeeklyBitcoinDataAsync(euro);

            var viewModel = new LabelPricePairInfoViewModel
            {
                Labels = data.Select(m => m.Date.ToString("yyyy-MM-dd")),
                Prices = data.Select(m => m.Close ?? 0)
            };

            return Ok(viewModel);
        }

        public async Task<IActionResult> Subscribe(string email, double percentage, int hours)
        {
            var subscription = new Subscription()
            {
                Email = email,
                Percentage = percentage,
                Hours = hours,
            };

            await context.Subscriptions.AddAsync(subscription);
            await context.SaveChangesAsync();

            return Ok();
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
