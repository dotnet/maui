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


	public class Tests : IDisposable
	{

		public void Dispose() { }
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void WorksWithoutXDeclaration(XamlInflator inflator)
		{
			var page = new GlobalXmlns(inflator);
			Assert.NotNull(page.label);
			Assert.Equal("No xmlns:x declaration, but x: usage anyway", page.label.Text);
		}
	}
}