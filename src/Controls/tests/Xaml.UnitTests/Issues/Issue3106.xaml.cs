using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue3106 : ContentPage
	{
		public Issue3106()
		{
			InitializeComponent();
		}
		public Issue3106(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void NewDoesNotThrow(bool useCompiledXaml)
			{
				var p = new Issue3106(useCompiledXaml);
			}
		}
	}
}

