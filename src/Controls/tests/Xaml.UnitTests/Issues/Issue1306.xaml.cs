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
		}
		public class Tests
		{
			// Constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// IDisposable public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			[InlineData(true)]
			public void AssignBindingMarkupToBindingBase(bool useCompiledXaml)
			{
				var listView = new Issue1306(useCompiledXaml);

				Assert.NotNull(listView.GroupDisplayBinding);
				Assert.NotNull(listView.GroupShortNameBinding);
				Assert.That(listView.GroupDisplayBinding, Is.TypeOf<Binding>());
				Assert.That(listView.GroupShortNameBinding, Is.TypeOf<Binding>());
				Assert.Equal("Key", (listView.GroupDisplayBinding as Binding).Path);
				Assert.Equal("Key", (listView.GroupShortNameBinding as Binding).Path);
			}
		}
	}
}

