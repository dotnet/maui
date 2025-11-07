using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18697 : ContentPage
{
	public Maui18697() => InitializeComponent();

	public class Test
	{
		MockDeviceInfo mockDeviceInfo;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void OnBindingRelease(XamlInflator inflator)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Desktop;
			var page = new Maui18697(inflator);
			Assert.Equal("_ProfileToolBarText", page.ToolbarItems[0].Text);
		}
	}
}

[AcceptEmptyServiceProvider]
[ContentProperty(nameof(Name))]
public class Maui18697TranslateExtension : IMarkupExtension<BindingBase>
{
	public string Name { get; set; }

	public BindingBase ProvideValue(IServiceProvider serviceProvider)
	{
		return new Binding
		{
			Mode = BindingMode.OneWay,
			Path = $"[{Name}]",
			Source = new Dictionary<string, string>
			{
				{ "Hello", "_Hello" },
				{ "ProfileToolBarText", "_ProfileToolBarText" }
			}
		};
	}

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
