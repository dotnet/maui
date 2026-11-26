using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui39132Probe
{
	public static int Instances;

	public Maui39132Probe() => Instances++;
}

public partial class Maui39132 : ContentPage
{
	public Maui39132() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void MarkupInXDataTypeIsNotInstantiated(XamlInflator inflator)
		{
			Maui39132Probe.Instances = 0;

			var page = new Maui39132(inflator);
			var template = (Controls.DataTemplate)page.Resources["ItemTemplate"];
			for (var i = 0; i < 3; i++)
				Assert.IsType<Label>(template.CreateContent());

			Assert.Equal(0, Maui39132Probe.Instances);
		}
	}
}
