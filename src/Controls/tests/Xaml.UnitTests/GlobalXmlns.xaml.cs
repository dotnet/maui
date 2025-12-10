using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlns
{
	public GlobalXmlns() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void WorksWithoutXDeclaration(XamlInflator inflator)
		{
			var page = new GlobalXmlns(inflator);
			Assert.NotNull(page.label);
			Assert.Equal("No xmlns:x declaration, but x: usage anyway", page.label.Text);
		}
	}
}