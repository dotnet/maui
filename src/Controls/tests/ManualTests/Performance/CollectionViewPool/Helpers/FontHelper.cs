using System;
using System.Collections.Generic;
using System.Text;

namespace PoolMathApp
{
	public static class FontGlyphs
	{
		public static readonly string Save = "\uf2bc";
		public static readonly string Cancel = "\uf2c0";
		public static readonly string RightArrow = "\uf119";
		public static readonly string DropDownArrow = "\uf3d0";
		public static readonly string Alert = "\uf104";
		public static readonly string TriangleAlert = "\uf268";
		public static readonly string Warn = "\uf16f";
		public static readonly string Reminder = "\uf274";
		public static readonly string Add = "\uf102";
		public static readonly string Flame = "\uf42f";
		public static readonly string RightChevron = "\uf3d1";
		public static readonly string Cost = "\uf2b1";
		public static readonly string Copy = "\uf2DA";
		public static readonly string Restore = "\uf366";
		public static readonly string Location = "\uf1e5";

		public static readonly string AlertThin = "\ue81C";

		public static readonly string Plus = "\uf273";

		public static readonly string Bookmark = "\uF29F";
		public static readonly string Premium = "\uf371";
		public static readonly string Rising = "\uf25b";
		public static readonly string Falling = "\uf25a";

		public static readonly string Beaker = "\uf431";

		public const string Overview = "\uf27a";
		public const string Timeline = "\uf369";
		//public const string Overview = "\uf450";

		public const string Navburger = "\ue900";

		public const string Advice = "\uf388";

		public const string Settings = "\uf4a7";
		public const string Share = "\uf378";

	}

	public static class WeatherGlyphs
	{
		public static readonly Dictionary<int, string> Glyphs = new Dictionary<int, string>
		{
			{ 1, "\ue99b" },
			{ 2, "\ue999" },
			{ 3, "\ue999" },
			{ 4, "\ue92b" },
			{ 5, "\ue92b" },
			{ 6, "\ue972" },
			{ 7, "\ue970" },
			{ 8, "\ue970" },
			{ 9, "" },
			{ 10, "" },
			{ 11, "\ue956" },
			{ 12, "\ue935" },
			{ 13, "\ue939" },
			{ 14, "\ue939" },
			{ 15, "\ue958" },
			{ 16, "\ue9b5" },
			{ 17, "\ue9b5" },
			{ 18, "\ue94f" },
			{ 19, "\ue915" },
			{ 20, "\ue984" },
			{ 21, "\ue984" },
			{ 22, "\ue901" },
			{ 23, "\ue986" },
			{ 24, "\ue9ad" },
			{ 25, "\ue97a" },
			{ 26, "\ue97a" },
			{ 27, "" },
			{ 28, "" },
			{ 29, "\ue97a" },
			{ 30, "\ue9a5" },
			{ 31, "\ue9ab" },
			{ 32, "\ue9cf" },
			{ 33, "\ue95e" },
			{ 34, "\ue95e" },
			{ 35, "\ue976" },
			{ 36, "\ue976" },
			{ 37, "\ue968" },
			{ 38, "\ue976" },
			{ 39, "\ue980" },
			{ 40, "\ue980" },
			{ 41, "\ue9be" },
			{ 42, "\ue9be" },
			{ 43, "\ue991" },
			{ 44, "\ue993" },
		};

		public static string Get(long id)
			=> Glyphs[(int)id];
	}
}
