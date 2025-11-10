using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui11467 : ParentButton
{
	public Maui11467() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			AppInfo.SetCurrent(new MockAppInfo());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void EventHandlerFromBaseType(XamlInflator inflator)
		{
			// Used to throw:
			// XamlParseException : Position 5:9. No method ParentButton_OnClicked with correct signature found on type Microsoft.Maui.Controls.Xaml.UnitTests.Maui11467
			var button = new Maui11467(inflator);
			Assert.Equal(1, button.MyEventSubscriberCount);
		}
	}
}