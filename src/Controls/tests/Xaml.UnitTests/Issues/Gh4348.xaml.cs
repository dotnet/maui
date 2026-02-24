using System.Collections.ObjectModel;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4348VM : ObservableCollection<string>
{
	public Gh4348VM()
	{
		Add("foo");
		Add("bar");
	}
}

public partial class Gh4348 : ContentPage
{
	public Gh4348() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void GenericBaseClassResolution(XamlInflator inflator)
		{
			var layout = new Gh4348(inflator) { BindingContext = new Gh4348VM() };
			Assert.Equal("2", layout.labelCount.Text);
		}
	}
}
