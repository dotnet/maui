using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5705 : Shell
{
	public Gh5705() => InitializeComponent();

	[TestFixture]
	class Tests
	{

		[Test]
		public void SearchHandlerIneritBC([Values] XamlInflator inflator)
		{
			var vm = new object();
			var shell = new Gh5705(inflator) { BindingContext = vm };
			Assert.That(shell.searchHandler.BindingContext, Is.EqualTo(vm));
		}
	}
}
