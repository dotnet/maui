using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;
using WindowsOS = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class PlatformSpecific : FlyoutPage
{
	public PlatformSpecific() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void PlatformSpecificPropertyIsSet(XamlInflator inflator)
		{
			var layout = new PlatformSpecific(inflator);
			Assert.Equal(CollapseStyle.Partial, layout.On<WindowsOS>().GetCollapseStyle());
			Assert.Equal(96d, layout.On<WindowsOS>().CollapsedPaneWidth());
		}
	}
}