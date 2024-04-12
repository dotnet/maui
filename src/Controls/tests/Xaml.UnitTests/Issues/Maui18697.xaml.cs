using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NuGet.Frameworks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[ContentProperty(nameof(Name))]
[AcceptEmptyServiceProvider]
public class Maui18697ExtExtension : IMarkupExtension<BindingBase>
{
	public string Name { get; set; }	
	public BindingBase ProvideValue(IServiceProvider serviceProvider)
	{
		return new Binding(Name);
	}

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}

//  [XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Maui18697
{
    public string Foo { get; set; } = "Success";
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
		MockDeviceInfo _mockDeviceInfo;

        [SetUp]
        public void Setup()
        {
            Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(_mockDeviceInfo = new MockDeviceInfo());
            DispatcherProvider.SetCurrent(new DispatcherProviderStub());
        }

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Test]
        public void OnIdiomBinding([Values(false, true)] bool useCompiledXaml)
        {
            if (useCompiledXaml)
                MockCompiler.Compile(typeof(Maui18697));

			_mockDeviceInfo.Idiom = DeviceIdiom.Phone;
			var page = new Maui18697(useCompiledXaml) { BindingContext = new { Foo = "Success" } };
			Assert.That(page.label0.Text, Is.EqualTo("Success"));
			Assert.That(page.label1.Text, Is.EqualTo("Success"));
        }
    }
}