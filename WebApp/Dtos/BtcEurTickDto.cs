using System;

namespace WebApp.Dtos
{
	public sealed class BtcEurTickDto
	{
		public string Market { get; set; }                 
		public string Instrument { get; set; }             
		public decimal Price { get; set; }                 
		public decimal PriceCzk { get; set; }			   
		public decimal BestBid { get; set; }               
		public decimal BestAsk { get; set; }               
		public DateTime PriceLastUpdateUtc { get; set; }   
		public decimal CurrentHourOpen { get; set; }
		public decimal CurrentHourHigh { get; set; }
		public decimal CurrentHourLow { get; set; }
		public decimal BestBidCzk { get; set; }
		public decimal BestAskCzk { get; set; }
		public decimal CurrentHourOpenCzk { get; set; }
		public decimal CurrentHourHighCzk { get; set; }
		public decimal CurrentHourLowCzk { get; set; }
	}
}