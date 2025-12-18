using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlnsWithDataTemplate : ContentPage
{
	public GlobalXmlnsWithDataTemplate() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void GlobalXmlnsWithDataTemplateTest(XamlInflator inflator)
		{
			var page = new GlobalXmlnsWithDataTemplate(inflator);
		}
	}
}