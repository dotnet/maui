using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class SetStyleIdFromXName : ContentPage
{
	public SetStyleIdFromXName() => InitializeComponent();


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
		public void SetStyleId(XamlInflator inflator)
		{
			var layout = new SetStyleIdFromXName(inflator);
			Assert.Equal("label0", layout.label0.StyleId);
			Assert.Equal("foo", layout.label1.StyleId);
			Assert.Equal("bar", layout.label2.StyleId);
		}
	}
}
