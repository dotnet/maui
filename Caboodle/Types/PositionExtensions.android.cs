using System;

namespace Microsoft.Caboodle
{
	public static partial class PositionExtensions
	{
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static DateTimeOffset GetTimestamp(this Android.Locations.Location location)
        {
            try
            {
                return new DateTimeOffset(epoch.AddMilliseconds(location.Time));
            }
            catch (Exception e)
            {
                return new DateTimeOffset(epoch);
            }
        }

        internal static Position ToPosition(this Android.Locations.Location location)
        {
			var p = new Position
			{
				Longitude = location.Longitude,
				Latitude = location.Latitude,
				TimestampUtc = location.GetTimestamp().ToUniversalTime()
			};
			return p;
        }
    }
}
