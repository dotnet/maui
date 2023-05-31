namespace MauiApp._1.Resources.Styles;

public class AppColors : ResourceDictionary
{
    public static Color Primary { get; } = Color.FromArgb("#512BD4");
    public static Color Secondary { get; } = Color.FromArgb("#DFD8F7");
    public static Color Tertiary { get; } = Color.FromArgb("#2B0B98");
    public static Color White { get; } = Colors.White;
    public static Color Black { get; } = Colors.Black;
    public static Color Gray100 { get; } = Color.FromArgb("#E1E1E1");
    public static Color Gray200 { get; } = Color.FromArgb("#C8C8C8");
    public static Color Gray300 { get; } = Color.FromArgb("#ACACAC");
    public static Color Gray400 { get; } = Color.FromArgb("#919191");
    public static Color Gray500 { get; } = Color.FromArgb("#6E6E6E");
    public static Color Gray600 { get; } = Color.FromArgb("#404040");
    public static Color Gray900 { get; } = Color.FromArgb("#212121");
    public static Color Gray950 { get; } = Color.FromArgb("#141414");

    public static SolidColorBrush PrimaryBrush { get; } = new(Primary);
    public static SolidColorBrush SecondaryBrush { get; } = new(Secondary);
    public static SolidColorBrush TertiaryBrush { get; } = new(Tertiary);
    public static SolidColorBrush WhiteBrush { get; } = new(White);
    public static SolidColorBrush BlackBrush { get; } = new(Black);
    public static SolidColorBrush Gray100Brush { get; } = new(Gray100);
    public static SolidColorBrush Gray200Brush { get; } = new(Gray200);
    public static SolidColorBrush Gray300Brush { get; } = new(Gray300);
    public static SolidColorBrush Gray400Brush { get; } = new(Gray400);
    public static SolidColorBrush Gray500Brush { get; } = new(Gray500);
    public static SolidColorBrush Gray600Brush { get; } = new(Gray600);
    public static SolidColorBrush Gray900Brush { get; } = new(Gray900);
    public static SolidColorBrush Gray950Brush { get; } = new(Gray950);

    public static Color Yellow100Accent { get; } = Color.FromArgb("#F7B548");
    public static Color Yellow200Accent { get; } = Color.FromArgb("#FFD590");
    public static Color Yellow300Accent { get; } = Color.FromArgb("#FFE5B9");
    public static Color Cyan100Accent { get; } = Color.FromArgb("#28C2D1");
    public static Color Cyan200Accent { get; } = Color.FromArgb("#7BDDEF");
    public static Color Cyan300Accent { get; } = Color.FromArgb("#C3F2F4");
    public static Color Blue100Accent { get; } = Color.FromArgb("#3E8EED");
    public static Color Blue200Accent { get; } = Color.FromArgb("#72ACF1");
    public static Color Blue300Accent { get; } = Color.FromArgb("#A7CBF6");
}
