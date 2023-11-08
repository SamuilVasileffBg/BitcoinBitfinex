using Crypto_MVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class SendEmailsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    public SendEmailsService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    await DoWorkAsync(stoppingToken, dbContext);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken, ApplicationDbContext _context)
    {
        try
        {
            var subscriptions = await _context.Subscriptions.Where(x => x.IsDeleted == false).ToListAsync(cancellationToken);

            foreach (var subscription in subscriptions)
            {
                var currentTime = DateTime.UtcNow;
                var currentPrice = await GetCurrentBitcoinPriceAsync(cancellationToken);
                var historicalPrice = await GetBitcoinPriceNHoursAgoAsync((int)subscription.Hours, cancellationToken);

                var percentageChange = ((currentPrice - historicalPrice) / historicalPrice) * 100;

                Console.WriteLine($"Current:{currentPrice} Historical:{historicalPrice}");
                Console.WriteLine($"{subscription.Email}: {percentageChange}%");

                if (percentageChange >= (decimal)subscription.Percentage)
                {
                    subscription.IsDeleted = true;
                    await _context.SaveChangesAsync();

                    Execute($"Bitcoin price has gone up with {percentageChange:f2}%" +
                        $" for the last {subscription.Hours} hour(s).", subscription.Email).Wait();
                }

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    public async Task<decimal> GetCurrentBitcoinPriceAsync(CancellationToken cancellationToken)
    {
        string url = "https://api.bitfinex.com/v2/ticker/tBTCUSD";

        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ticker = JsonConvert.DeserializeObject<List<decimal>>(content);

                return ticker[6];
            }
            else
            {
                throw new HttpRequestException("Could not retrieve current Bitcoin price.");
            }
        }
    }

    public async Task<decimal> GetBitcoinPriceNHoursAgoAsync(int hoursAgo, CancellationToken cancellationToken)
    {
        var startTime = new DateTimeOffset(DateTime.UtcNow.AddHours(-hoursAgo)).ToUnixTimeMilliseconds();

        var endTime = startTime + (3600 * 1000);

        string url = $"https://api.bitfinex.com/v2/candles/trade:1h:tBTCUSD/hist?start={startTime}&end={endTime}&limit=1";

        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var candles = JsonConvert.DeserializeObject<List<List<decimal>>>(content);
                if (candles != null && candles.Count > 0)
                {
                    var candle = candles[0];
                    return  candle[2];
                }
                else
                {
                    throw new Exception("No data returned for the specified time frame.");
                }
            }
            else
            {
                throw new HttpRequestException($"Could not retrieve Bitcoin price for {hoursAgo} hours ago.");
            }
        }
    }

    static async Task Execute(string msgText, string toEmail)
    {
        var apiKey = "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";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("samuil.vasilev73@gmail.com", "Samuil Vasilev");
        var subject = "Bitcoin Price info";
        var to = new EmailAddress(toEmail, "Subscriber");
        var plainTextContent = msgText;
        var htmlContent = $"<strong>{msgText}</strong>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
    }
}