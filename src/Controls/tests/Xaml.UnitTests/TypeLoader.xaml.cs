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
		public class Tests
		{
			// Constructor
			public void SetUp()
			{
				Application.Current = new MockApplication();
			}

			[Theory]
			[InlineData(true)]
			public void LoadTypeFromXmlns(bool useCompiledXaml)
			{
				TypeLoader layout = null;
				Assert.DoesNotThrow(() => layout = new TypeLoader(useCompiledXaml));
				Assert.NotNull(layout.customview0);
				Assert.That(layout.customview0, Is.TypeOf<CustomView>());
			}

			[Theory]
			[InlineData(true)]
			public void LoadTypeFromXmlnsWithoutAssembly(bool useCompiledXaml)
			{
				TypeLoader layout = null;
				Assert.DoesNotThrow(() => layout = new TypeLoader(useCompiledXaml));
				Assert.NotNull(layout.customview1);
				Assert.That(layout.customview1, Is.TypeOf<CustomView>());
			}
		}
	}
}