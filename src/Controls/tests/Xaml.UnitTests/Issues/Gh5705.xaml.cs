using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5705 : Shell
{
	public Gh5705() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{

		[Theory]
		[XamlInflatorData]
		internal void SearchHandlerIneritBC(XamlInflator inflator)
		{
			var vm = new object();
			var shell = new Gh5705(inflator) { BindingContext = vm };
			Assert.Equal(vm, shell.searchHandler.BindingContext);
		}
	}
}
