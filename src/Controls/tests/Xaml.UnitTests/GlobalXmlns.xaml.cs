using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class GlobalXmlns
{
	public GlobalXmlns() => InitializeComponent();

	[SetUp]
	public void Setup()
	{
		Application.SetCurrentApplication(new MockApplication());
		DispatcherProvider.SetCurrent(new DispatcherProviderStub());
	}

	[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

	[Test]
	public void WorksWithoutXDeclaration([Values] XamlInflator inflator)
	{
		var page = new GlobalXmlns(inflator);
		Assert.That(page.label, Is.Not.Null);
		Assert.That(page.label.Text, Is.EqualTo("No xmlns:x declaration, but x: usage anyway"));
	}
}