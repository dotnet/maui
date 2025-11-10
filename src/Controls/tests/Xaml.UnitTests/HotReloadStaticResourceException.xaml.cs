using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class HotReloadStaticResourceException : ContentPage
{
	public HotReloadStaticResourceException()
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
			Controls.Internals.ResourceLoader.ResourceProvider2 = null;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

#if DEBUG
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		public void MissingResourceExceptionAreHandled(XamlInflator inflator)
		{
			Controls.Internals.ResourceLoader.ExceptionHandler2 = (ex) =>
			{
				var (exception, filepath) = ex;

			};
			var page = new HotReloadStaticResourceException(inflator);
		}
#endif
	}
}