using System;

namespace WebApp.Data
{
	/// <summary>
	/// Persisted snapshot of a BTC/EUR tick converted to CZK.
	/// </summary>
	public sealed class ExchangeRateEntry
	{
		public int ExchangeRateEntryId { get; set; }

		public DateTime TimestampUtc { get; set; }   // provider timestamp (UTC)

		// Values for CZK-only UI, original values discarded.
		public decimal PriceCzk { get; set; }
		public decimal BestBidCzk { get; set; }
		public decimal BestAskCzk { get; set; }

		public string Market { get; set; }           // e.g., "coinbase"
		public string Instrument { get; set; }       // e.g., "BTC-EUR"

		public string UserNote { get; set; }         // nullable, edited on Saved page

		public DateTime CreatedAtUtc { get; set; }
		public DateTime UpdatedAtUtc { get; set; }
	}
}