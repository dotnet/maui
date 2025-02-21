using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23347
{
    public Maui23347()
    {
        InitializeComponent();
    }

    public Maui23347(bool useCompiledXaml)
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
        public void FontImageSourceIssue([Values(false, true)] bool useCompiledXaml)
        {
			Application.Current.UserAppTheme = AppTheme.Light;
            var page = new Maui23347(useCompiledXaml);
            Application.Current.MainPage = page;
        }
    }
}