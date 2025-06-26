using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{

	public partial class Maui13619 : ContentPage
	{
		public Maui13619() => InitializeComponent();
		public Maui13619(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Fact]
			public void AppThemeBindingAndDynamicResource([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui13619(useCompiledXaml);
				Assert.Equal(Colors.HotPink, page.label0.TextColor);
				Assert.Equal(Colors.DarkGray, page.label0.BackgroundColor);

				page.Resources["Primary"] = Colors.SlateGray;
				Assert.Equal(Colors.SlateGray, page.label0.BackgroundColor);

			}
		}
	}
}
