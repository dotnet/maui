

using PoolMathApp.Models;

[assembly: ExportFont("ionicons.ttf", Alias = "Ionicons")]
[assembly: ExportFont("pe-icon-set-weather.ttf", Alias = "Weather")]
[assembly: ExportFont("feather.ttf", Alias = "Feather")]
[assembly: ExportFont("icomoon.ttf", Alias = "Icomoon")]

//[assembly: ExportFont("Gilroy-UltraLight.ttf", Alias = "DefaultLightest")]
//[assembly: ExportFont("Gilroy-Light.ttf", Alias = "DefaultLighter")]
//[assembly: ExportFont("Gilroy-Regular.ttf", Alias = "Default")]
//[assembly: ExportFont("Gilroy-Medium.ttf", Alias = "DefaultStrong")]
//[assembly: ExportFont("Gilroy-SemiBold.ttf", Alias = "DefaultStronger")]
//[assembly: ExportFont("Gilroy-Bold.ttf", Alias = "DefaultHeavy")]
//[assembly: ExportFont("Gilroy-ExtraBold.ttf", Alias = "DefaultHeavier")]
//[assembly: ExportFont("Gilroy-Heavy.ttf", Alias = "DefaultHeaviest")]


namespace PoolMathApp.Helpers;

public static class FontWeightExtensions
{
	public static string ToFontAlias(this Models.FontWeight fontWeight)
		=> fontWeight switch
		{
			PoolMathApp.Models.FontWeight.Normal => "Default",
			PoolMathApp.Models.FontWeight.Lightest => "DefaultLightest",
			PoolMathApp.Models.FontWeight.Lighter => "DefaultLighter",
			PoolMathApp.Models.FontWeight.Strong => "DefaultStrong",
			PoolMathApp.Models.FontWeight.Stronger => "DefaultStronger",
			PoolMathApp.Models.FontWeight.Heavy => "DefaultHeavy",
			PoolMathApp.Models.FontWeight.Heavier => "DefaultHeavier",
			PoolMathApp.Models.FontWeight.Heaviest => "DefaultHeaviest",
			_ => "Default"
		};

	public static string ToFontSizeResourceKey(this NamedFontSize fontSize)
		=> fontSize switch
		{
			NamedFontSize.FontSize08 => fontSizeKey(8),
			NamedFontSize.FontSize09 => fontSizeKey(9),
			NamedFontSize.FontSize10 => fontSizeKey(10),
			NamedFontSize.FontSize11 => fontSizeKey(11),
			NamedFontSize.FontSize12 => fontSizeKey(12),
			NamedFontSize.FontSize13 => fontSizeKey(13),
			NamedFontSize.FontSize14 => fontSizeKey(14),
			NamedFontSize.FontSize15 => fontSizeKey(15),
			NamedFontSize.FontSize16 => fontSizeKey(16),
			NamedFontSize.FontSize17 => fontSizeKey(17),
			NamedFontSize.FontSize18 => fontSizeKey(18),
			NamedFontSize.FontSize19 => fontSizeKey(19),
			NamedFontSize.FontSize20 => fontSizeKey(20),
			NamedFontSize.FontSize21 => fontSizeKey(21),
			NamedFontSize.FontSize22 => fontSizeKey(22),
			NamedFontSize.FontSize24 => fontSizeKey(24),
			NamedFontSize.FontSize26 => fontSizeKey(26),
			NamedFontSize.FontSize28 => fontSizeKey(28),
			NamedFontSize.FontSize30 => fontSizeKey(30),
			NamedFontSize.FontSize32 => fontSizeKey(32),
			NamedFontSize.FontSize34 => fontSizeKey(34),
			_ => fontSizeKey(12)

		};

	static string fontSizeKey(int size)
		=> size < 10 ? $"FontSize_0{size}" : $"FontSize_{size}";
}