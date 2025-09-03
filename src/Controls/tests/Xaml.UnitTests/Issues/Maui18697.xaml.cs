using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18697 : ContentPage
{
	public Maui18697() => InitializeComponent();

	class Test
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Test]
		public void OnBindingRelease([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Desktop;
			var page = new Maui18697(inflator);
			Assert.That(page.ToolbarItems[0].Text, Is.EqualTo("_ProfileToolBarText"));
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