using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue1213 : TabbedPage
	{
		public Issue1213()
		{
			InitializeComponent();
		}

		public Issue1213(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			// Constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// IDisposable public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			[InlineData(true)]
			public void MultiPageAsContentPropertyAttribute(bool useCompiledXaml)
			{
				var page = new Issue1213(useCompiledXaml);
				Assert.Equal(2, page.Children.Count);
			}
		}
	}
}