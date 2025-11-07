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


	public class Tests
	{
		[Theory]
		[Values]
		public void BindingToEmptyCollection()
		{
			Gh4516 layout = null;
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => layout = new Gh4516(inflator) { BindingContext = new Gh4516VM() });
			Assert.Equal("foo.jpg", (layout.image.Source as FileImageSource).File);
		}
	}
}