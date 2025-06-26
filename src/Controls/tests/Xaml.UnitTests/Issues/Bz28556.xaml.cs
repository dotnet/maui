using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz28556 : ContentPage
	{
		public Bz28556()
		{
			InitializeComponent();
		}

		public Bz28556(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			[Theory]
			[InlineData(false)]
			public void SettersAppliedBeforeTriggers(bool useCompiledXaml)
			{
				var layout = new Bz28556(useCompiledXaml);

				Assert.Equal(Colors.Yellow, layout.entry.TextColor);
				Assert.Equal(Colors.Green, layout.entry.BackgroundColor);

				Assert.Equal(Colors.Red, layout.disableEntry.TextColor);
				Assert.Equal(Colors.Purple, layout.disableEntry.BackgroundColor);

				layout.entry.IsEnabled = false;
				layout.disableEntry.IsEnabled = true;

				Assert.Equal(Colors.Yellow, layout.disableEntry.TextColor);
				Assert.Equal(Colors.Green, layout.disableEntry.BackgroundColor);

				Assert.Equal(Colors.Red, layout.entry.TextColor);
				Assert.Equal(Colors.Purple, layout.entry.BackgroundColor);

				layout.entry.IsEnabled = true;
				layout.disableEntry.IsEnabled = false;

				Assert.Equal(Colors.Yellow, layout.entry.TextColor);
				Assert.Equal(Colors.Green, layout.entry.BackgroundColor);

				Assert.Equal(Colors.Red, layout.disableEntry.TextColor);
				Assert.Equal(Colors.Purple, layout.disableEntry.BackgroundColor);
			}
		}
	}
}

