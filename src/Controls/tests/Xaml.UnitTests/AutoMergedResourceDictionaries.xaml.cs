using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class AutoMergedResourceDictionaries : ContentPage
{
	public AutoMergedResourceDictionaries()
	{
		InitializeComponent();
	}


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
		public void AutoMergedRd(XamlInflator inflator)
		{
			var layout = new AutoMergedResourceDictionaries(inflator);
			Assert.Equal(Colors.Purple, layout.label.TextColor);
			Assert.Equal(Color.FromArgb("#FF96F3"), layout.label.BackgroundColor);
		}
	}
}
