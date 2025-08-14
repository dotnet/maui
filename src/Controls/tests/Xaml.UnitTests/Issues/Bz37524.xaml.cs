using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz37524 : ContentPage
{
	public Bz37524()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void MultiTriggerConditionNotApplied([Values] XamlInflator inflator)
		{
			var layout = new Bz37524(inflator);
			Assert.AreEqual(false, layout.TheButton.IsEnabled);
		}
	}
}
