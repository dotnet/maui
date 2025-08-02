using System;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void AddCustomElementInCollection([Values] XamlInflator inflator)
		{
			var page = new Issue1250(inflator);
			var stack = page.stack;

			Assert.AreEqual(3, stack.Children.Count);
			Assert.That(stack.Children[0], Is.TypeOf<Label>());
			Assert.That(stack.Children[1], Is.TypeOf<Issue1250AspectRatioContainer>());
			Assert.That(stack.Children[2], Is.TypeOf<Label>());
		}
	}
}