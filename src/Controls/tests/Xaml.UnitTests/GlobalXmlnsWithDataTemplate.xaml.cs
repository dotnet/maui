using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class GlobalXmlnsWithDataTemplate : ContentPage
{
	public GlobalXmlnsWithDataTemplate() => InitializeComponent();

	[Test]
	public void GlobalXmlnsWithDataTemplateTest([Values] XamlInflator inflator)
	{
		var page = new GlobalXmlnsWithDataTemplate(inflator);
	}
}