using System;

namespace WebApp.Dtos
{
	public sealed class BtcEurTickDto
	{
		public string Market { get; set; }                 // coinbase
		public string Instrument { get; set; }             // BTC-EUR
		public decimal Price { get; set; }                 // PRICE
		public decimal PriceCzk { get; set; }			   // computed via CNB EUR->CZK
		public decimal BestBid { get; set; }               // BEST_BID
		public decimal BestAsk { get; set; }               // BEST_ASK
		public DateTime PriceLastUpdateUtc { get; set; }   // from PRICE_LAST_UPDATE_TS
		public decimal CurrentHourOpen { get; set; }
		public decimal CurrentHourHigh { get; set; }
		public decimal CurrentHourLow { get; set; }
	}
}