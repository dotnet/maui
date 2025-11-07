using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25172 : CoreContentPage<VM25172>
{
	public Maui25172() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void CompilationWithGenericBaseClassSucceeds(XamlInflator inflator)
		{
			var page = new Maui25172(inflator);
			Assert.True(typeof(CoreContentPage<VM25172>).IsAssignableFrom(page.GetType()));
		}
	}
}

public class VM25172 { }

public class CoreContentPage<T> : ContentPage
{
}
