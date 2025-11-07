using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlnsWithDataTemplate : ContentPage
{
	public GlobalXmlnsWithDataTemplate() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void GlobalXmlnsWithDataTemplateTest(XamlInflator inflator)
		{
			var page = new GlobalXmlnsWithDataTemplate(inflator);
		}
	}
}