using System;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Services;

namespace WebApp.Controllers
{
	[RoutePrefix("api/exchangeRates")]
	public sealed class ExchangeRateController : ApiController
	{
		// single service instance; small, easy to step through
		private static readonly ExchangeRateService _service = new ExchangeRateService();
		
		// GET /api/exchangeRates/coindesk/btceur
		[HttpGet, Route("coindesk/btceur")]
		public async Task<HttpResponseMessage> GetBtcEurExchangeRateAsync()
		{
			try
			{
				string json = await _service.GetLatestRawJsonAsync(CancellationToken.None);

				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
				};
			}
			catch (Exception ex)
			{
				// keep it simple for now; we can switch to ProblemDetails later
				return Request.CreateErrorResponse(HttpStatusCode.BadGateway, ex);
			}
		}
	}
}
