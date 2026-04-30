namespace MauiApp._1.Resources.Styles;

// Note: For Android please see also Platforms/Android/Resources/values/colors.xml
public class AppColors : ResourceDictionary
{
	public AppColors()
	{
		Add("Primary", Color.FromArgb("#512BD4"));
		Add("PrimaryDark", Color.FromArgb("#ac99ea"));
		Add("PrimaryDarkText", Color.FromArgb("#242424"));
		Add("Secondary", Color.FromArgb("#DFD8F7"));
		Add("SecondaryDarkText", Color.FromArgb("#9880e5"));
		Add("Tertiary", Color.FromArgb("#2B0B98"));

		Add("White", Colors.White);
		Add("Black", Colors.Black);
		Add("Magenta", Color.FromArgb("#D600AA"));
		Add("MidnightBlue", Color.FromArgb("#190649"));
		Add("OffBlack", Color.FromArgb("#1f1f1f"));

		Add("Gray100", Color.FromArgb("#E1E1E1"));
		Add("Gray200", Color.FromArgb("#C8C8C8"));
		Add("Gray300", Color.FromArgb("#ACACAC"));
		Add("Gray400", Color.FromArgb("#919191"));
		Add("Gray500", Color.FromArgb("#6E6E6E"));
		Add("Gray600", Color.FromArgb("#404040"));
		Add("Gray900", Color.FromArgb("#212121"));
		Add("Gray950", Color.FromArgb("#141414"));

		Add("PrimaryBrush", new SolidColorBrush((Color)this["Primary"]));
		Add("SecondaryBrush", new SolidColorBrush((Color)this["Secondary"]));
		Add("TertiaryBrush", new SolidColorBrush((Color)this["Tertiary"]));
		Add("WhiteBrush", new SolidColorBrush((Color)this["White"]));
		Add("BlackBrush", new SolidColorBrush((Color)this["Black"]));

		Add("Gray100Brush", new SolidColorBrush((Color)this["Gray100"]));
		Add("Gray200Brush", new SolidColorBrush((Color)this["Gray200"]));
		Add("Gray300Brush", new SolidColorBrush((Color)this["Gray300"]));
		Add("Gray400Brush", new SolidColorBrush((Color)this["Gray400"]));
		Add("Gray500Brush", new SolidColorBrush((Color)this["Gray500"]));
		Add("Gray600Brush", new SolidColorBrush((Color)this["Gray600"]));
		Add("Gray900Brush", new SolidColorBrush((Color)this["Gray900"]));
		Add("Gray950Brush", new SolidColorBrush((Color)this["Gray950"]));
	}
}
