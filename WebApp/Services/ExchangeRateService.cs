using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WebApp.Dtos;

namespace WebApp.Services
{
    public sealed class ExchangeRateService
    {
        // todo cleanup rename
        private static readonly HttpClient s_http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private const string BtcEurUrl =
            "https://data-api.coindesk.com/spot/v1/latest/tick?market=coinbase&instruments=BTC-EUR";

        private static DateTime FromUnixSecondsToUtc(long seconds)
            => DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;

        private async Task<string> FetchRawJsonAsync(CancellationToken ct)
        {
            using (var response = await s_http.GetAsync(BtcEurUrl, ct))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> GetLatestRawJsonAsync(CancellationToken ct)
        {
            var raw = await FetchRawJsonAsync(ct);
            return raw;
        }
        
        public async Task<BtcEurTickDto> GetLatestParsedAsync(CancellationToken ct)
        {
            string rawJson = await FetchRawJsonAsync(ct);

            var root = JObject.Parse(rawJson);
            var tick = (JObject)root["Data"]?["BTC-EUR"];

            if (tick == null)
                throw new InvalidOperationException("Unexpected payload: Data['BTC-EUR'] not found.");

            var dto = new BtcEurTickDto
            {
                Market = tick.Value<string>("MARKET"),
                Instrument = tick.Value<string>("INSTRUMENT"),
                Price = tick.Value<decimal>("PRICE"),
                BestBid = tick.Value<decimal?>("BEST_BID") ?? 0m,
                BestAsk = tick.Value<decimal?>("BEST_ASK") ?? 0m,
                PriceLastUpdateUtc = FromUnixSecondsToUtc(tick.Value<long>("PRICE_LAST_UPDATE_TS")),
                CurrentHourOpen = tick.Value<decimal?>("CURRENT_HOUR_OPEN") ?? 0m,
                CurrentHourHigh = tick.Value<decimal?>("CURRENT_HOUR_HIGH") ?? 0m,
                CurrentHourLow = tick.Value<decimal?>("CURRENT_HOUR_LOW") ?? 0m
            };

            return dto;
        }
    }
}