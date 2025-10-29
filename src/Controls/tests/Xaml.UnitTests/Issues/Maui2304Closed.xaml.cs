using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui2304Closed
	{
		public Maui2304Closed() => InitializeComponent();
		public Maui2304Closed(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				var layout = new Maui2304Closed(useCompiledXaml);
				Assert.Equal(typeof(OnPlatform<string>), typeof(Maui2304Closed).BaseType);
			}
		}
	}
}
