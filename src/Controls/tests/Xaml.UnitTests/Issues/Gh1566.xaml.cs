using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh1566
	{
		public Gh1566()
		{
			InitializeComponent();
		}

		public Gh1566(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(true), InlineData(false)]
			public void ObsoletePropsDoNotThrow(bool useCompiledXaml)
			{
				var layout = new Gh1566(useCompiledXaml);
				Assert.Equal(Colors.Red, layout.frame.BorderColor);
			}
		}
	}
}
