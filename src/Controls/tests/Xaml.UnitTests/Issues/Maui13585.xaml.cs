using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Xaml.UnitTests
{


	public partial class Maui13585 : ContentPage
	{
		public Maui13585() => InitializeComponent();
		public Maui13585(bool useCompiledXaml)
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
				var page = new Maui13585(useCompiledXaml);
				Assert.Equal(Colors.Green, page.styleTriggerWithStaticResources.BackgroundColor);
				Assert.Equal(Colors.Green, page.styleTriggerWithDynamicResources.BackgroundColor);

				page.styleTriggerWithStaticResources.IsEnabled = false;
				page.styleTriggerWithDynamicResources.IsEnabled = false;

				Assert.Equal(Colors.Purple, page.styleTriggerWithStaticResources.BackgroundColor);
				Assert.Equal(Colors.Purple, page.styleTriggerWithDynamicResources.BackgroundColor);

				page.styleTriggerWithStaticResources.IsEnabled = true;
				page.styleTriggerWithDynamicResources.IsEnabled = true;

				Assert.Equal(Colors.Green, page.styleTriggerWithStaticResources.BackgroundColor);
				Assert.Equal(Colors.Green, page.styleTriggerWithDynamicResources.BackgroundColor);

			}
		}
	}
}