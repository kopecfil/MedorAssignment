using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Services
{
	public sealed class ExchangeRateService
	{
		const string CoinbaseBtcEurEchangeRateUrl = "https://data-api.coindesk.com/spot/v1/latest/tick?market=coinbase&instruments=BTC-EUR";
		
		private static readonly HttpClient http = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(5)
		};

		private async Task<string> FetchRawJsonAsync(CancellationToken ct)
		{

			using (var response = await http.GetAsync(CoinbaseBtcEurEchangeRateUrl, ct))
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
	}
}