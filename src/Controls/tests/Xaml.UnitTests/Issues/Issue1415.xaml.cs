using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue1415 : ContentPage
	{
		public Issue1415()
		{
			InitializeComponent();
		}

		public Issue1415(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void NestedMarkupExtension(bool useCompiledXaml)
			{
				var page = new Issue1415(useCompiledXaml);
				var label = page.FindByName<Label>("label");
				Assert.NotNull(label);
				label.BindingContext = "foo";
				Assert.Equal("oof", label.Text);
			}
		}
	}
}