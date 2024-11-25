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
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21495
{
    public Maui21495()
    {
        InitializeComponent();
    }

    public Maui21495(bool useCompiledXaml)
    {
        //this stub will be replaced at compile time
    }

    [TestFixture]
    class Test
    {
        [SetUp]
        public void Setup()
        {
            Application.SetCurrentApplication(new MockApplication());
            DispatcherProvider.SetCurrent(new DispatcherProviderStub());
        }

        [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

        [Test]
        public async Task AppThemeLeak([Values(false, true)] bool useCompiledXaml)
        {
            Application.Current.Resources.Add("labelColor", Colors.HotPink);
			var page = new Maui21495(useCompiledXaml);
			Application.Current.MainPage = page;
            var pagewr = new WeakReference(page);

            Application.Current.MainPage = page = null;            
            await Task.Delay(10);
            GC.Collect();
            Assert.IsNull(pagewr.Target, "Page leaked");
        }
    }
}
