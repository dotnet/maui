using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TypeLoader : ContentPage
{
	public TypeLoader() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IClassFixture<ApplicationFixture>
	{
		public Tests(ApplicationFixture fixture) { }

		[Theory]
		[XamlInflatorData]
		internal void LoadTypeFromXmlns(XamlInflator inflator)
		{
			TypeLoader layout = null;
			var exception = Record.Exception(() => layout = new TypeLoader(inflator));
			Assert.Null(exception);
			Assert.NotNull(layout.customview0);
			Assert.IsType<CustomView>(layout.customview0);
		}

		[Theory]
		[XamlInflatorData]
		internal void LoadTypeFromXmlnsWithoutAssembly(XamlInflator inflator)
		{
			TypeLoader layout = null;
			var exception = Record.Exception(() => layout = new TypeLoader(inflator));
			Assert.Null(exception);
			Assert.NotNull(layout.customview1);
			Assert.IsType<CustomView>(layout.customview1);
		}
	}
}