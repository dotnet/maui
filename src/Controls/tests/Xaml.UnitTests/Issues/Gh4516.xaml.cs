using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4516VM
{
	public Uri[] Images { get; } = Array.Empty<Uri>();
}

public partial class Gh4516 : ContentPage
{
	public Gh4516() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void BindingToEmptyCollection(XamlInflator inflator)
		{
			Gh4516 layout = null;
			var ex = Record.Exception(() => layout = new Gh4516(inflator) { BindingContext = new Gh4516VM() });
			Assert.Null(ex);
			Assert.Equal("foo.jpg", (layout.image.Source as FileImageSource).File);
		}
	}
}