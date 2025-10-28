using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class U001Page : ContentPage
	{
		public U001Page()
		{
			;
		}

	}

	public partial class Unreported001 : TabbedPage
	{
		public Unreported001()
		{
			InitializeComponent();
		}

		public Unreported001(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(false)]
			[InlineData(true)]
			public void DoesNotThrow(bool useCompiledXaml)
			{
				var p = new Unreported001(useCompiledXaml);
				Assert.IsType<U001Page>(p.navpage.CurrentPage);
			}
		}
	}
}