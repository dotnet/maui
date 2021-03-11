using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

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

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void SettersAppliedBeforeTriggers(bool useCompiledXaml)
			{
				var layout = new Bz28556(useCompiledXaml);

				Assert.AreEqual(Color.Yellow, layout.entry.TextColor);
				Assert.AreEqual(Color.Green, layout.entry.BackgroundColor);

				Assert.AreEqual(Color.Red, layout.disableEntry.TextColor);
				Assert.AreEqual(Color.Purple, layout.disableEntry.BackgroundColor);

				layout.entry.IsEnabled = false;
				layout.disableEntry.IsEnabled = true;

				Assert.AreEqual(Color.Yellow, layout.disableEntry.TextColor);
				Assert.AreEqual(Color.Green, layout.disableEntry.BackgroundColor);

				Assert.AreEqual(Color.Red, layout.entry.TextColor);
				Assert.AreEqual(Color.Purple, layout.entry.BackgroundColor);

				layout.entry.IsEnabled = true;
				layout.disableEntry.IsEnabled = false;

				Assert.AreEqual(Color.Yellow, layout.entry.TextColor);
				Assert.AreEqual(Color.Green, layout.entry.BackgroundColor);

				Assert.AreEqual(Color.Red, layout.disableEntry.TextColor);
				Assert.AreEqual(Color.Purple, layout.disableEntry.BackgroundColor);
			}
		}
	}
}

