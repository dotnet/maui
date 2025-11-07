using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz37524 : ContentPage
{
	public Bz37524()
	{
		InitializeComponent();
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void MultiTriggerConditionNotApplied(XamlInflator inflator)
		{
			var layout = new Bz37524(inflator);
			Assert.False(layout.TheButton.IsEnabled);
		}
	}
}
