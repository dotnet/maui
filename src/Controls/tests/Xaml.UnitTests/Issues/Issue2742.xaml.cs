using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Issue2742BasePage : ContentPage
	{

	}

	public partial class Issue2742 : Issue2742BasePage
	{
		public Issue2742()
		{
			InitializeComponent();
		}

		public Issue2742(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(true)]
			public void ToolBarItemsOnContentPageInheritors(bool useCompiledXaml)
			{
				var layout = new Issue2742(useCompiledXaml);
				Assert.That(layout.Content, Is.TypeOf<Label>());
				Assert.Equal("test", ((Label)layout.Content).Text);

				Assert.NotNull(layout.ToolbarItems);
				Assert.Equal(2, layout.ToolbarItems.Count);
				Assert.Equal("One", layout.ToolbarItems[0].Text);
			}
		}
	}
}