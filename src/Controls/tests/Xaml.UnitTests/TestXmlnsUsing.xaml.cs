using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class TestXmlnsUsing : ContentPage
	{
		public TestXmlnsUsing()
		{
			InitializeComponent();
		}

		public TestXmlnsUsing(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown]
			[Xunit.Fact]
			public void TearDown()
			{
				Application.Current = null;
			}

			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void SupportUsingXmlns(bool useCompiledXaml)
			{
				var page = new TestXmlnsUsing(useCompiledXaml);
				Assert.NotNull(page.Content);
				Assert.IsType<CustomXamlView>(page.CustomView);
				Assert.Equal(1, page.Radio1.Value);
				Assert.Equal(2, page.Radio2.Value);
			}
		}
	}
}
