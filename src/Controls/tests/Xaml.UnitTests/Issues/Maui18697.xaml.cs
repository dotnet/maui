using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18697 : ContentPage
{
	public Maui18697()
	{
		InitializeComponent();
	}

	public Maui18697(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
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
		public void OnBindingRelease([Values(false, true)] bool useCompiledXaml)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Desktop;
			var page = new Maui18697(useCompiledXaml);
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

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
	{
		return ProvideValue(serviceProvider);
	}
}