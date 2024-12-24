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

public partial class Maui21774
{
	public Maui21774()
	{
		InitializeComponent();
	}

	public Maui21774(bool useCompiledXaml)
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
		public void AppThemeChangeOnUnparentedPage([Values(false, true)] bool useCompiledXaml)
		{
			Application.Current.Resources.Add("labelColor", Colors.LimeGreen);
			Application.Current.UserAppTheme = AppTheme.Light;
			var page = new Maui21774(useCompiledXaml);
			Application.Current.MainPage = page;

			Assert.That(page.label0.TextColor, Is.EqualTo(Colors.LimeGreen));
			Assert.That(page.label1.TextColor, Is.EqualTo(Colors.LimeGreen));

			//unparent the page, change the resource and the theme
			Application.Current.MainPage = null;
			Application.Current.Resources["labelColor"] = Colors.HotPink;
			Application.Current.UserAppTheme = AppTheme.Dark;
			//labels should not change
			Assert.That(page.label0.TextColor, Is.EqualTo(Colors.LimeGreen));
			Assert.That(page.label1.TextColor, Is.EqualTo(Colors.LimeGreen));

			//reparent the page
			Application.Current.MainPage = page;
			//labels should change
			Assert.That(page.label0.TextColor, Is.EqualTo(Colors.HotPink));
			Assert.That(page.label1.TextColor, Is.EqualTo(Colors.HotPink));
		}
	}
}
