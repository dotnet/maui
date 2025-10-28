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
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21774
{
	public Maui21774()
	{
		InitializeComponent();
	}

	public Maui21774(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
		{
			Application.Current.Resources.Add("labelColor", Colors.LimeGreen);
			Application.Current.UserAppTheme = AppTheme.Light;
			var page = new Maui21774(useCompiledXaml);
			Application.Current.MainPage = page;

			Assert.Equal(Colors.LimeGreen, page.label0.TextColor);
			Assert.Equal(Colors.LimeGreen, page.label1.TextColor);

			//unparent the page, change the resource and the theme
			Application.Current.MainPage = null;
			Application.Current.Resources["labelColor"] = Colors.HotPink;
			Application.Current.UserAppTheme = AppTheme.Dark;
			//labels should not change
			Assert.Equal(Colors.LimeGreen, page.label0.TextColor);
			Assert.Equal(Colors.LimeGreen, page.label1.TextColor);

			//reparent the page
			Application.Current.MainPage = page;
			//labels should change
			Assert.Equal(Colors.HotPink, page.label0.TextColor);
			Assert.Equal(Colors.HotPink, page.label1.TextColor);
		}
	}
}
