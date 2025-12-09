using System.Text.RegularExpressions;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25671, "Layout passes should not increase", PlatformAffected.All)]

public partial class Issue25671 : ContentPage
{
	public static long MeasurePasses = 0;
	public static long ArrangePasses = 0;

	private int _regenIndex = 2;
	private static readonly string _loremIpsumLongText = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed non risus. Suspendisse lectus tortor, dignissim sit amet, adipiscing nec, ultricies sed, dolor. Cras elementum ultrices diam. Maecenas ligula massa, varius a, semper congue, euismod non, mi. Proin porttitor, orci nec nonummy molestie, enim est eleifend mi, non fermentum diam nisl sit amet erat. Duis semper. Duis arcu massa, scelerisque vitae, consequat in, pretium a, enim. Pellentesque congue.";
	private static readonly string[] _words = new Regex(@"\w+").Matches(_loremIpsumLongText).Select(m => m.Value).ToArray();

	public Issue25671()
	{
		InitializeComponent();
		GenerateItems();
		CV.HandlerChanged += (s, e) =>
		{
			if (CV.Handler is { } handler)
			{
				HeadingLabel.Text = handler.GetType().Name;
			}
		};
	}

	void RegenerateItems(object sender, EventArgs args)
	{
		_regenIndex = (_regenIndex - 1) % 4 + 2;
		GenerateItems();
	}

	void OnClick(object sender, EventArgs args)
	{
		((Button)sender).Text = $"M: {MeasurePasses}, A: {ArrangePasses}";
	}

	void GenerateItems()
	{
		CV.ItemsSource = Enumerable.Range(4, 200).Select(i =>
			new
			{
				Text = string.Join(' ', _words.Take(i % _regenIndex == 0 ? i % _words.Length : (i * 2) % _words.Length)),
				Glyph = char.ConvertFromUtf32(0xf127 + i % 10)
			});
	}
}

public class Issue25671AbsoluteLayout : AbsoluteLayout
{
	public static long MeasurePasses = 0;
	public static long ArrangePasses = 0;

	protected override Size ArrangeOverride(Rect bounds)
	{
		Interlocked.Increment(ref Issue25671.ArrangePasses);
		Interlocked.Increment(ref ArrangePasses);
		return base.ArrangeOverride(bounds);
	}

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		Interlocked.Increment(ref Issue25671.MeasurePasses);
		Interlocked.Increment(ref MeasurePasses);
		return base.MeasureOverride(widthConstraint, heightConstraint);
	}
}

public class Issue25671VerticalStackLayout : VerticalStackLayout
{
	public static long MeasurePasses = 0;
	public static long ArrangePasses = 0;

	protected override Size ArrangeOverride(Rect bounds)
	{
		Interlocked.Increment(ref Issue25671.ArrangePasses);
		Interlocked.Increment(ref ArrangePasses);
		return base.ArrangeOverride(bounds);
	}

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		Interlocked.Increment(ref Issue25671.MeasurePasses);
		Interlocked.Increment(ref MeasurePasses);
		return base.MeasureOverride(widthConstraint, heightConstraint);
	}
}

public class Issue25671Grid : Grid
{
	public static long MeasurePasses = 0;
	public static long ArrangePasses = 0;

	protected override Size ArrangeOverride(Rect bounds)
	{
		Interlocked.Increment(ref Issue25671.ArrangePasses);
		Interlocked.Increment(ref ArrangePasses);
		return base.ArrangeOverride(bounds);
	}

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		Interlocked.Increment(ref Issue25671.MeasurePasses);
		Interlocked.Increment(ref MeasurePasses);
		return base.MeasureOverride(widthConstraint, heightConstraint);
	}
}

public class Issue25671ContentView : ContentView
{
	public static long MeasurePasses = 0;
	public static long ArrangePasses = 0;

	protected override Size ArrangeOverride(Rect bounds)
	{
		Interlocked.Increment(ref Issue25671.ArrangePasses);
		Interlocked.Increment(ref ArrangePasses);
		return base.ArrangeOverride(bounds);
	}

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		Interlocked.Increment(ref Issue25671.MeasurePasses);
		Interlocked.Increment(ref MeasurePasses);
		return base.MeasureOverride(widthConstraint, heightConstraint);
	}
}

public class Issue25671Label : Label
{
	public static long MeasurePasses = 0;
	public static long ArrangePasses = 0;

	protected override Size ArrangeOverride(Rect bounds)
	{
		Interlocked.Increment(ref Issue25671.ArrangePasses);
		Interlocked.Increment(ref ArrangePasses);
		return base.ArrangeOverride(bounds);
	}

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		Interlocked.Increment(ref Issue25671.MeasurePasses);
		Interlocked.Increment(ref MeasurePasses);
		return base.MeasureOverride(widthConstraint, heightConstraint);
	}
}

public class Issue25671Button : Button
{
	public static long MeasurePasses = 0;
	public static long ArrangePasses = 0;

	protected override Size ArrangeOverride(Rect bounds)
	{
		Interlocked.Increment(ref Issue25671.ArrangePasses);
		Interlocked.Increment(ref ArrangePasses);
		return base.ArrangeOverride(bounds);
	}

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		Interlocked.Increment(ref Issue25671.MeasurePasses);
		Interlocked.Increment(ref MeasurePasses);
		return base.MeasureOverride(widthConstraint, heightConstraint);
	}
}

public class Issue25671Image : Image
{
	public static long MeasurePasses = 0;
	public static long ArrangePasses = 0;

	protected override Size ArrangeOverride(Rect bounds)
	{
		Interlocked.Increment(ref Issue25671.ArrangePasses);
		Interlocked.Increment(ref ArrangePasses);
		return base.ArrangeOverride(bounds);
	}

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		Interlocked.Increment(ref Issue25671.MeasurePasses);
		Interlocked.Increment(ref MeasurePasses);
		return base.MeasureOverride(widthConstraint, heightConstraint);
	}
}