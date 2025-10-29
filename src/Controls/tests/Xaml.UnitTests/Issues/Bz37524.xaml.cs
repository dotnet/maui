using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz37524 : ContentPage
	{
		public Bz37524()
		{
			InitializeComponent();
		}

		public Bz37524(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void MultiTriggerConditionNotApplied(bool useCompiledXaml)
			{
				var layout = new Bz37524(useCompiledXaml);
				Assert.False(layout.TheButton.IsEnabled);
			}
		}
	}
}