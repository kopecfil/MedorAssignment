using System;
using System.Globalization;

namespace WebApp.Basic
{
	public class Utils
	{
		public static DateTime? ParseUtcOrNull(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return null;

			var cs = new CultureInfo("cs-CZ");
			DateTime dt;

			// full date-time
			if (DateTime.TryParseExact(input.Trim(),
				    new[] { "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm" },
				    cs, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
				return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

			// date only → start of day UTC
			if (DateTime.TryParseExact(input.Trim(), "dd.MM.yyyy",
				    cs, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
				return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

			return null;
		}

		public static decimal? ParseDecOrNull(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return null;
			// tolerate both comma and dot
			var s = input.Trim().Replace(',', '.');
			return decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : (decimal?)null;
		}

	}
}