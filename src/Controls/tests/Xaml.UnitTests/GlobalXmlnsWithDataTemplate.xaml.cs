using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlnsWithDataTemplate : ContentPage
{
	public GlobalXmlnsWithDataTemplate()
	{
		InitializeComponent();
	}
	public GlobalXmlnsWithDataTemplate(bool useCompiledXaml)
	{
	}

	[Test]
	public void GlobalXmlnsWithDataTemplateTest([Values] bool useCompiledXaml)
	{
		var page = new GlobalXmlnsWithDataTemplate(useCompiledXaml);
	}
}