using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue1438 : ContentPage
	{
		public Issue1438()
		{
			InitializeComponent();
		}

		public Issue1438(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void XNameForwardDeclaration(bool useCompiledXaml)
			{
				var page = new Issue1438(useCompiledXaml);

				var slider = page.FindByName<Slider>("slider");
				var label = page.FindByName<Label>("label");
				Assert.Same(slider, label.BindingContext);
				Assert.IsType<StackLayout>(slider.Parent);
			}
		}
	}
}