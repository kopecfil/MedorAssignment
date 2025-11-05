using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WebApp.Data;
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

        private const string BtcEurUrl = "https://data-api.coindesk.com/spot/v1/latest/tick?market=coinbase&instruments=BTC-EUR";
        private const string CnbDailyUrl = "https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/daily.txt";

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

            var eurToCzk = await GetEurToCzkAsync(ct); // <— CNB rate (CZK per EUR)

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
                CurrentHourLow = tick.Value<decimal?>("CURRENT_HOUR_LOW") ?? 0m,
                PriceCzk = eurToCzk * tick.Value<decimal>("PRICE")
            };
            // todo this seems unnecessarily ugly tho it doesn't actually matter, this code is as simple as it can get
            dto.BestBidCzk = eurToCzk * dto.BestBid;
            dto.BestAskCzk = eurToCzk * dto.BestAsk;
            dto.CurrentHourOpenCzk = eurToCzk * dto.CurrentHourOpen;
            dto.CurrentHourHighCzk = eurToCzk * dto.CurrentHourHigh;
            dto.CurrentHourLowCzk = eurToCzk * dto.CurrentHourLow;


            return dto;
        }
        
        private static decimal ParseCnbRate(string text, string code)
        {
            // CNB format:
            // Line 1: "DD MMM YYYY #NNN"
            // Line 2: header
            // Next lines: Country|Currency|Amount|Code|Rate   (Rate uses comma decimal)
            // Example EUR line: "...|1|EUR|24,70"
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 2; i < lines.Length; i++)
            {
                var parts = lines[i].Split('|');
                if (parts.Length < 5) continue;
                var lineCode = parts[3].Trim();
                if (!string.Equals(lineCode, code, StringComparison.OrdinalIgnoreCase)) continue;

                var amountStr = parts[2].Trim();
                var rateStr = parts[4].Trim().Replace(',', '.'); // make decimal invariant

                if (!int.TryParse(amountStr, out var amount)) throw new InvalidOperationException("CNB: invalid Amount");
                if (!decimal.TryParse(rateStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rateCzk))
                    throw new InvalidOperationException("CNB: invalid Rate");

                if (amount <= 0) throw new InvalidOperationException("CNB: Amount must be > 0");
                return rateCzk / amount; // CZK per 1 EUR
            }

            throw new InvalidOperationException("CNB: EUR line not found");
        }

        private async Task<decimal> GetEurToCzkAsync(CancellationToken ct)
        {
            using (var resp = await s_http.GetAsync(CnbDailyUrl, ct))
            {
                resp.EnsureSuccessStatusCode();
                var text = await resp.Content.ReadAsStringAsync();
                return ParseCnbRate(text, "EUR");
            }
        }
        
        public async Task<int[]> SaveSnapshotsAsync(IEnumerable<WebApp.Dtos.SaveSnapshotItemDto> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var nowUtc = DateTime.UtcNow;
            using (var db = new ExchangeRatesDbContext())
            {
                var entities = new List<ExchangeRateEntry>();

                foreach (var i in items)
                {
                    // basic validation
                    if (i.TimestampUtc == default) throw new ArgumentException("TimestampUtc is required.");
                    // Price/Bid/Ask can be 0m but not NaN; numbers from JSON come as decimals fine.

                    entities.Add(new ExchangeRateEntry
                    {
                        TimestampUtc = i.TimestampUtc,
                        PriceCzk = i.PriceCzk,
                        BestBidCzk = i.BestBidCzk,
                        BestAskCzk = i.BestAskCzk,
                        Market = string.IsNullOrWhiteSpace(i.Market) ? "coinbase" : i.Market,
                        Instrument = string.IsNullOrWhiteSpace(i.Instrument) ? "BTC-EUR" : i.Instrument,
                        UserNote = null,
                        CreatedAtUtc = nowUtc,
                        UpdatedAtUtc = nowUtc
                    });
                }

                db.ExchangeRateEntries.AddRange(entities);
                await db.SaveChangesAsync();

                return entities.Select(e => e.ExchangeRateEntryId).ToArray();
            }
        }

    }
}