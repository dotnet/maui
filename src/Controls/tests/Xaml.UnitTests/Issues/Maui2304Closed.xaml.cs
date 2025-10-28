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
		}		class Tests
		{
			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				var layout = new Maui2304Closed(useCompiledXaml);
				Assert.Equal(typeof(OnPlatform<string>), typeof(Maui2304Closed).BaseType);
			}
		}
	}
}
