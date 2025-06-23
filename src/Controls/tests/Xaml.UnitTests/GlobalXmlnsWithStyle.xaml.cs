using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlnsWithStyle : ContentPage
{
	public GlobalXmlnsWithStyle()
	{
		InitializeComponent();
	}
	public GlobalXmlnsWithStyle(bool useCompiledXaml)
	{
		// this stub will be replaced at compile time
	}

	[Test]
	public void GlobalXmlnsWithStyleTest([Values] bool useCompiledXaml)
	{

		var page = new GlobalXmlnsWithStyle(useCompiledXaml);
		Assert.That(page.label0.TextColor, Is.EqualTo(Colors.Red));
		Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.Blue));

	}
}