using System;
using System.Linq;
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
		
		// POST /api/exchangeRates/snapshots/bulk
		[HttpPost, Route("snapshots/bulk")]
		public async Task<IHttpActionResult> SaveSnapshotsBulk([FromBody] SaveSnapshotsRequest request)
		{
			if (request?.Items == null || request.Items.Length == 0)
				return BadRequest("Items is required and must contain at least one item.");

			try
			{
				var ids = await _service.SaveSnapshotsAsync(request.Items);
				return Ok(new { inserted = ids.Length, ids });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}
		
		// GET /api/exchangeRates/snapshots/all
		[HttpGet, Route("snapshots/all")]
		public IHttpActionResult GetAllSnapshots()
		{
			using (var db = new WebApp.Data.ExchangeRatesDbContext())
			{
				var rows = db.ExchangeRateEntries
					.OrderBy(x => x.TimestampUtc)   // oldest -> newest
					.ToList();

				return Ok(rows);
			}
		}
	}
}
