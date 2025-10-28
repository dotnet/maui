using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue1306 : ListView
	{
		public Issue1306()
		{
			InitializeComponent();
		}

		public Issue1306(bool useCompiledXaml)
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
			public void AssignBindingMarkupToBindingBase(bool useCompiledXaml)
			{
				var listView = new Issue1306(useCompiledXaml);

				Assert.NotNull(listView.GroupDisplayBinding);
				Assert.NotNull(listView.GroupShortNameBinding);
				Assert.IsType<Binding>(listView.GroupDisplayBinding);
				Assert.IsType<Binding>(listView.GroupShortNameBinding);
				Assert.Equal("Key", (listView.GroupDisplayBinding as Binding).Path);
				Assert.Equal("Key", (listView.GroupShortNameBinding as Binding).Path);
			}
		}
	}
}

