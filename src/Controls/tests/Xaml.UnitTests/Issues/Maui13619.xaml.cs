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
		public public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void Method(bool useCompiledXaml)
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
