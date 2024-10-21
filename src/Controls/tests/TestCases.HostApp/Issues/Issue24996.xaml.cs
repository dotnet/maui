namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24996, "Translation causes multiple layout passes", PlatformAffected.All)]
public partial class Issue24996 : ContentPage
{
	public Issue24996()
	{
		InitializeComponent();
		UpdateText();

		SizeChanged += delegate {
			if (Width > 0) {
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
		Root.MeasurePasses = Root.ArrangePasses = 0;
		Lvl1.MeasurePasses = Lvl1.ArrangePasses = 0;
		Lvl2.MeasurePasses = Lvl2.ArrangePasses = 0;
		Lvl3.MeasurePasses = Lvl3.ArrangePasses = 0;
		UpdateText();
	}

	public async void OnTapped(object sender, EventArgs e)
	{
		Lvl2.TranslationY = Random.Shared.Next(0, (int)Height);
		Lvl2.TranslationX = Random.Shared.Next(0, (int)Width);
		await Task.Delay(500);
		UpdateText();
	}

	void UpdateText()
	{
		Stats.Text = $"Root[{Root.MeasurePasses}/{Root.ArrangePasses}] - Lvl1[{Lvl1.MeasurePasses}/{Lvl1.ArrangePasses}] - Lvl2[{Lvl2.MeasurePasses}/{Lvl2.ArrangePasses}] - Lvl3[{Lvl3.MeasurePasses}/{Lvl3.ArrangePasses}]";
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