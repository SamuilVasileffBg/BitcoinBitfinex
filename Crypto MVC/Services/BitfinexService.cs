using Crypto_MVC.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class BitfinexService
{
    private readonly HttpClient _client;

    public BitfinexService(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<BitcoinPrice>> GetBitcoinDataAsyncForTheDay(int dayChange, bool euro = false)
    {
        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(dayChange)).ToUnixTimeMilliseconds();

        var endTime = new DateTimeOffset(dayChange == 0 ? DateTime.UtcNow.AddDays(dayChange) : DateTime.UtcNow.Date.AddDays(dayChange + 1)).ToUnixTimeMilliseconds();

        var currency = euro ? "EUR" : "USD";
        string url = $"https://api.bitfinex.com/v2/candles/trade:1h:tBTC{currency}/hist?start={startTime}&end={endTime}&sort=1";

        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var candles = JsonConvert.DeserializeObject<List<List<decimal>>>(content);

                var prices = candles.Select(c => new BitcoinPrice
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds((long)c[0]).UtcDateTime,
                    Close = c[2]
                }).ToList();

                return prices;
            }
        }

        return new List<BitcoinPrice>();
    }

    public async Task<List<BitcoinPrice>> GetWeeklyBitcoinDataAsync(bool euro = false)
    {
        var weeklyPrices = new List<BitcoinPrice>();

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;
        var startTime = new DateTimeOffset(sevenDaysAgo).ToUnixTimeMilliseconds();

        var endTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

        var currency = euro ? "EUR" : "USD";
        string url = $"https://api.bitfinex.com/v2/candles/trade:1D:tBTC{currency}/hist?start={startTime}&end={endTime}&sort=1";

        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var candles = JsonConvert.DeserializeObject<List<List<decimal>>>(content);

                weeklyPrices = candles.Select(c => new BitcoinPrice
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds((long)c[0]).UtcDateTime,
                    Close = c[2]
                }).ToList();
            }
        }

        weeklyPrices = weeklyPrices.OrderBy(p => p.Date).ToList();

        return weeklyPrices;
    }

    private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }
}
