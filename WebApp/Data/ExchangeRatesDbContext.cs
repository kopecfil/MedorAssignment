using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WebApp.Data
{
	/// <summary>
	/// EF6 context for exchange rate snapshots.
	/// </summary>
	public sealed class ExchangeRatesDbContext : DbContext
	{
		// Uses the connection string named "ExchangeRatesDb" from Web.config
		public ExchangeRatesDbContext() : base("name=ExchangeRatesDb") { }

		public DbSet<ExchangeRateEntry> ExchangeRateEntries { get; set; }

		protected override void OnModelCreating(DbModelBuilder mb)
		{
			// Table names singular for readability.
			mb.Conventions.Remove<PluralizingTableNameConvention>();

			// Currency precision.
			mb.Entity<ExchangeRateEntry>().Property(p => p.PriceCzk).HasPrecision(18, 8);
			mb.Entity<ExchangeRateEntry>().Property(p => p.BestBidCzk).HasPrecision(18, 8);
			mb.Entity<ExchangeRateEntry>().Property(p => p.BestAskCzk).HasPrecision(18, 8);

			base.OnModelCreating(mb);
		}
	}
}