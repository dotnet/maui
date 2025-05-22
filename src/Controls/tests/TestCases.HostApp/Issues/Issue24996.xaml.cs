namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24996, "Changing Translation of an element causes Maui in iOS to constantly run Measure & ArrangeChildren", PlatformAffected.All)]
public partial class Issue24996 : ContentPage
{
	Point[] _translations = [
		new(40, 80),
		new(1000, 20),
		new(20, 1000),
		new(1000, 1000),
	];

	int _index = -1;

	public Issue24996()
	{
		InitializeComponent();
		UpdateText();

		SizeChanged += delegate
		{
			if (Width > 0)
			{
				// For some reason, constraining this layout to a fixed size causes a `SetNeedsLayout` to be called
				// when translating the Lvl2 view outside the bottom boundary.
				// This causes a layout pass to be called on the Root, Lvl1, Lvl2, and Lvl3.
				Lvl1.WidthRequest = Width;
				Lvl1.HeightRequest = Height;
			}
		};
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Task.Delay(250);
		Lvl1.MeasurePasses = Lvl1.ArrangePasses = 0;
		Lvl2.MeasurePasses = Lvl2.ArrangePasses = 0;
		Lvl3.MeasurePasses = Lvl3.ArrangePasses = 0;
		UpdateText();
	}

	public async void OnTapped(object sender, EventArgs e)
	{
		var testPoint = _translations[++_index % _translations.Length];
		Coords.Text = $"X: {testPoint.X}, Y: {testPoint.Y}";
		Lvl2.TranslationX = testPoint.X;
		Lvl2.TranslationY = testPoint.Y;
		await Task.Delay(100);
		UpdateText();
	}

	void UpdateText()
	{
		Stats.Text = $"Lvl1[{Lvl1.MeasurePasses}/{Lvl1.ArrangePasses}] - Lvl2[{Lvl2.MeasurePasses}/{Lvl2.ArrangePasses}] - Lvl3[{Lvl3.MeasurePasses}/{Lvl3.ArrangePasses}]";
	}
}

public class ObservedLayout24996 : AbsoluteLayout
{
	public int MeasurePasses { get; set; }
	public int ArrangePasses { get; set; }

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		MeasurePasses++;
		return base.MeasureOverride(widthConstraint, heightConstraint);
	}

	protected override Size ArrangeOverride(Rect bounds)
	{
		ArrangePasses++;
		return base.ArrangeOverride(bounds);
	}
}