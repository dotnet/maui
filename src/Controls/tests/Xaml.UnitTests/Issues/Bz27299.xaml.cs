using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz27299ViewModel
	{
		public string Text
		{
			get { return "Foo"; }
		}
	}
	public class Bz27299ViewModelLocator
	{
		public static int Count { get; set; }
		public object Bz27299
		{
			get
			{
				Count++;
				return new Bz27299ViewModel();
			}
		}
	}

	public partial class Bz27299 : ContentPage
	{
		public Bz27299()
		{
			InitializeComponent();
		}

		public Bz27299(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp]
			[Xunit.Fact]
			public void SetUp()
			{
				Bz27299ViewModelLocator.Count = 0;
			}

			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void ViewModelLocatorOnlyCalledOnce(bool useCompiledXaml)
			{
				Assert.Empty(Bz27299ViewModelLocator);
				var layout = new Bz27299(useCompiledXaml);
				Assert.Single(Bz27299ViewModelLocator);
				Assert.Equal("Foo", layout.label.Text);
			}
		}
	}
}