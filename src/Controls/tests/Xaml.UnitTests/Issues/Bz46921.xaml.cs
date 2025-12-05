using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz46921 : ContentPage
{
	public Bz46921()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void MultipleWaysToCreateAThicknessResource(XamlInflator inflator)
		{
			var page = new Bz46921(inflator);
			foreach (var resname in new string[] { "thickness0", "thickness1", "thickness2", "thickness3", })
			{
				var resource = page.Resources[resname];
				Assert.IsType<Thickness>(resource);
				var thickness = (Thickness)resource;
				Assert.Equal(new Thickness(4, 20, 4, 20), thickness);

			}
		}
	}
}