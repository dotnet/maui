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

public partial class Maui28849 : ContentPage
{
    public Maui28849()
    {
        InitializeComponent();
    }

    public Maui28849(bool useCompiledXaml)
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


        [TearDown] public void TearDown()
        {
            AppInfo.SetCurrent(null);
            DeviceInfo.SetCurrent(null);
        }

        [Test]
        public void VSGReturnsToNormal([Values(false, true)] bool useCompiledXaml)
        {
            var page = new Maui28849(useCompiledXaml);

            Assert.That(page.button.IsEnabled, Is.False);
            Assert.That((page.button.Background as SolidColorBrush).Color , Is.EqualTo(Color.FromHex("#EBE9EB")));
            Assert.That(page.button.TextColor, Is.EqualTo(Color.FromHex("#3c3c3b")));

            page.button.IsEnabled = true;
            Assert.That(page.button.IsEnabled, Is.True);
            Assert.That((page.button.Background as SolidColorBrush).Color, Is.EqualTo(Color.FromHex("#D65397")));
            Assert.That(page.button.TextColor, Is.EqualTo(Colors.White));

            page.button.IsEnabled = false;
            Assert.That(page.button.IsEnabled, Is.False);
            Assert.That((page.button.Background as SolidColorBrush).Color , Is.EqualTo(Color.FromHex("#EBE9EB")));
            Assert.That(page.button.TextColor, Is.EqualTo(Color.FromHex("#3c3c3b")));

        }
    }
}