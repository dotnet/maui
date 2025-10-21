using System;
using System.Collections.Generic;
using System.Text;
//using PoolMathApp.Services;

namespace PoolMathApp.Xaml
{
	public static class TimelineBindingHelper
	{
		//static IDataLayer Data => Host.Data;

		public static bool TrackCSI => true; //Data?.CurrentPool?.TrackCSI ?? false;

		public static bool TrackSalt => true; // Data?.CurrentPool?.TrackSalt ?? false;
		public static bool TrackBorates => true;
		public static bool TrackWaterTemp => true;
	}
}
