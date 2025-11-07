using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5705 : Shell
{
	public Gh5705() => InitializeComponent();


	public class Tests
	{

		[Theory]
		[Values]
		public void SearchHandlerIneritBC(XamlInflator inflator)
		{
			var vm = new object();
			var shell = new Gh5705(inflator) { BindingContext = vm };
			Assert.Equal(vm, shell.searchHandler.BindingContext);
		}
	}
}
