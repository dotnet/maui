using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Issue1250AspectRatioContainer : ContentView
{
	protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
	{
		return new SizeRequest(new Size(widthConstraint, widthConstraint * AspectRatio));
	}

	public double AspectRatio { get; set; }
}

public partial class Issue1250 : ContentPage
{
	public Issue1250() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void AddCustomElementInCollection(XamlInflator inflator)
		{
			var page = new Issue1250(inflator);
			var stack = page.stack;

			Assert.Equal(3, stack.Children.Count);
			Assert.IsType<Label>(stack.Children[0]);
			Assert.IsType<Issue1250AspectRatioContainer>(stack.Children[1]);
			Assert.IsType<Label>(stack.Children[2]);
		}
	}
}