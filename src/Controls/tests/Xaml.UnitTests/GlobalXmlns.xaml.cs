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
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}	
		public void Dispose()
        {
           	Application.SetCurrentApplication(null);	
			AppInfo.SetCurrent(null);
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