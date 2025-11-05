using System;

namespace WebApp.Dtos
{
	public sealed class SaveSnapshotItemDto
	{
		public DateTime TimestampUtc { get; set; }   // required
		public decimal PriceCzk { get; set; }        // required
		public decimal BestBidCzk { get; set; }      // required
		public decimal BestAskCzk { get; set; }      // required
		public string Market { get; set; }           // e.g., "coinbase"
		public string Instrument { get; set; }       // e.g., "BTC-EUR"
	}

	public sealed class SaveSnapshotsRequest
	{
		public SaveSnapshotItemDto[] Items { get; set; }
	}
}