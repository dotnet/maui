using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class TypeLoader : ContentPage
	{
		public TypeLoader()
		{
			InitializeComponent();
		}

		public TypeLoader(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp]
			[Xunit.Fact]
			public void SetUp()
			{
				Application.Current = new MockApplication();
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void LoadTypeFromXmlns(bool useCompiledXaml)
			{
				TypeLoader layout = null;
				Assert.DoesNotThrow(() => layout = new TypeLoader(useCompiledXaml));
				Assert.NotNull(layout.customview0);
				Assert.IsType<CustomView>(layout.customview0);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void LoadTypeFromXmlnsWithoutAssembly(bool useCompiledXaml)
			{
				TypeLoader layout = null;
				Assert.DoesNotThrow(() => layout = new TypeLoader(useCompiledXaml));
				Assert.NotNull(layout.customview1);
				Assert.IsType<CustomView>(layout.customview1);
			}
		}
	}
}