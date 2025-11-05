using System;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Dtos;
using WebApp.Services;

namespace WebApp.Controllers
{
	[RoutePrefix("api/exchangeRates")]
	public sealed class ExchangeRateController : ApiController
	{
		private static readonly ExchangeRateService _service = new ExchangeRateService();

		// GET /api/exchangeRates/coindesk/btceur
		[HttpGet, Route("coindesk/btceur")]
		public async Task<IHttpActionResult> GetBtcEurExchangeRateAsync()
		{
			try
			{
				BtcEurTickDto dto = await _service.GetLatestParsedAsync(CancellationToken.None);
				return Ok(dto);
			}
			catch(Exception ex)
			{
				return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
			}
		}
	}
}