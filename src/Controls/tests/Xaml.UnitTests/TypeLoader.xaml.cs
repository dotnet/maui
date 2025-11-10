using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TypeLoader : ContentPage
{
	public TypeLoader() => InitializeComponent();

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
		public void LoadTypeFromXmlns(XamlInflator inflator)
		{
			TypeLoader layout = null;
			Assert.Null(Record.Exception(() => layout = new TypeLoader(inflator)));
			Assert.NotNull(layout.customview0);
			Assert.IsType<CustomView>(layout.customview0);
		}

		[Theory]
		[Values]
		public void LoadTypeFromXmlnsWithoutAssembly(XamlInflator inflator)
		{
			TypeLoader layout = null;
			Assert.Null(Record.Exception(() => layout = new TypeLoader(inflator)));
			Assert.NotNull(layout.customview1);
			Assert.IsType<CustomView>(layout.customview1);
		}
	}
}