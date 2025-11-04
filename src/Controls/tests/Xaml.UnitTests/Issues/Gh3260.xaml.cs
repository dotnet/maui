using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public abstract class Gh3260MyGLayout<T> : Controls.Compatibility.Layout<T> where T : View
{
	protected override void LayoutChildren(double x, double y, double width, double height) => throw new NotImplementedException();
}

public class Gh3260MyLayout : Gh3260MyGLayout<View>
{
	protected override void LayoutChildren(double x, double y, double width, double height) => throw new NotImplementedException();
}

public partial class Gh3260 : ContentPage
{
	public Gh3260() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void AssignContentWithNoContentAttributeDoesNotThrow([Values] XamlInflator inflator)
		{
			var layout = new Gh3260(inflator);
			Assert.That(layout.mylayout.Children.Count, Is.EqualTo(1));
			Assert.That(layout.mylayout.Children[0], Is.EqualTo(layout.label));
		}
	}
}
