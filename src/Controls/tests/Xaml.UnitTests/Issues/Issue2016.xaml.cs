using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue2016 : ContentPage
	{
		public Issue2016()
		{
			InitializeComponent();
		}

		public Issue2016(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(false)]
			[InlineData(true)]
			public void TestSwitches(bool useCompiledXaml)
			{
				var page = new Issue2016(useCompiledXaml);
				Assert.Equal(false, page.a0.IsToggled);
				Assert.Equal(false, page.b0.IsToggled);
				Assert.Equal(false, page.s0.IsToggled);
				Assert.Equal(false, page.t0.IsToggled);

				page.a0.IsToggled = true;
				page.b0.IsToggled = true;

				Assert.Equal(true, page.s0.IsToggled);
				Assert.Equal(true, page.t0.IsToggled);
			}
		}
	}
}