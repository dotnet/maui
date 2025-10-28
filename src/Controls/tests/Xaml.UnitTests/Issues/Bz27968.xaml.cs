using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz27968Page : ContentPage
	{
	}

	public partial class Bz27968 : Bz27968Page
	{
		public Bz27968()
		{
			InitializeComponent();
		}

		public Bz27968(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(true)]
			[InlineData(false)]
			public void BaseClassIdentifiersAreValidForResources(bool useCompiledXaml)
			{
				var layout = new Bz27968(useCompiledXaml);
				Assert.IsType<ListView>(layout.Resources["listView"]);
			}
		}
	}
}
