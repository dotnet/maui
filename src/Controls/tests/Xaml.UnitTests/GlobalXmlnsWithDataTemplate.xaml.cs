using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlnsWithDataTemplate : ContentPage
{
	public GlobalXmlnsWithDataTemplate() => InitializeComponent();

	[Test]
	internal void GlobalXmlnsWithDataTemplateTest([Values] XamlInflator inflator)
	{
		var page = new GlobalXmlnsWithDataTemplate(inflator);
	}
}