using System;
using Xunit;

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

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void AssignContentWithNoContentAttributeDoesNotThrow(XamlInflator inflator)
		{
			var layout = new Gh3260(inflator);
			Assert.Single(layout.mylayout.Children);
			Assert.Equal(layout.label, layout.mylayout.Children[0]);
		}
	}
}
