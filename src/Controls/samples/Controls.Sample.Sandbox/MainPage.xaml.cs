namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	readonly string[] _colors = ["#512BD4", "#FBF0EB", "#FF6347", "#2E8B57", "#4169E1"];
	int _colorIndex;

	public MainPage()
	{
		InitializeComponent();
	}

	void OnChangeColorClicked(object? sender, EventArgs e)
	{
		_colorIndex = (_colorIndex + 1) % _colors.Length;
		var newColor = Color.FromArgb(_colors[_colorIndex]);

		// Dynamically change the Primary resource at runtime to reproduce flyout template behavior.
		Application.Current!.Resources["Primary"] = newColor;

		CurrentColorLabel.Text = $"Current Primary: {_colors[_colorIndex]}";
	}
}