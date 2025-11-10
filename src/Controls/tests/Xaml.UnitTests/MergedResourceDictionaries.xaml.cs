using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class MergedResourceDictionaries : ContentPage
{
	public MergedResourceDictionaries() => InitializeComponent();

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
		public void MergedResourcesAreFound(XamlInflator inflator)
		{
			MockCompiler.Compile(typeof(MergedResourceDictionaries));
			var layout = new MergedResourceDictionaries(inflator);
			Assert.Equal("Foo", layout.label0.Text);
			Assert.Equal(Colors.Pink, layout.label0.TextColor);
			Assert.Equal(Color.FromArgb("#111"), layout.label0.BackgroundColor);
		}
	}
}