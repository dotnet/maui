using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz51567 : ContentPage
	{
		public Bz51567()
		{
			InitializeComponent();
		}

		public Bz51567(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void SetterWithElementValue(bool useCompiledXaml)
			{
				var page = new Bz51567(useCompiledXaml);
				var style = page.Resources["ListText"] as Style;
				var setter = style.Setters[1];
				Assert.NotNull(setter);
			}
		}
	}
}
