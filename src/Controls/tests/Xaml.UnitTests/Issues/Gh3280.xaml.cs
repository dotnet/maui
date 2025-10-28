using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh3280 : ContentPage
	{
		public Gh3280()
		{
			InitializeComponent();
		}

		public Size Foo { get; set; }

		public Gh3280(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(false), TestCase(true)]
			public void SizeHasConverter(bool useCompiledXaml)
			{
				Gh3280 layout = null;
				Assert.DoesNotThrow(() => layout = new Gh3280(useCompiledXaml));
				Assert.Equal(new Size(15, 25, layout.Foo));
			}
		}
	}
}
