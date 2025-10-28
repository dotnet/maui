using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;


namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui11204 : ContentPage
	{
		public Maui11204() => InitializeComponent();
		public Maui11204(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				var page = new Maui11204(useCompiledXaml);
				Assert.Equal(Colors.FloralWhite, page.border.BackgroundColor);
				VisualStateManager.GoToState(page.border, "State1");
				Assert.Equal(2, page.border.StrokeThickness);
				Assert.Equal(Colors.Blue, page.border.BackgroundColor);
			}

			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				var page = new Maui11204(useCompiledXaml);
				Assert.Equal(Colors.HotPink, page.borderWithStyleClass.BackgroundColor);
				VisualStateManager.GoToState(page.borderWithStyleClass, "State1");
				Assert.Equal(2, page.borderWithStyleClass.StrokeThickness);
				Assert.Equal(Colors.Blue, page.borderWithStyleClass.BackgroundColor);
			}
		}
	}
}