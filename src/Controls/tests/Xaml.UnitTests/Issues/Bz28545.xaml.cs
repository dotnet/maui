using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	using AbsoluteLayout = Microsoft.Maui.Controls.Compatibility.AbsoluteLayout;

	public partial class Bz28545 : ContentPage
	{
		public Bz28545()
		{
			InitializeComponent();
		}

		[TestFixture]
		class Tests
		{
			[Test]
			public void TypeConverterAreAppliedForSettersToAttachedBP([Values] XamlInflator inflator)
			{
				var layout = new Bz28545(inflator);
				Assert.AreEqual(Colors.Pink, layout.label.TextColor);
				Assert.AreEqual(AbsoluteLayoutFlags.PositionProportional, AbsoluteLayout.GetLayoutFlags(layout.label));
				Assert.AreEqual(new Rect(1, 1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayout.GetLayoutBounds(layout.label));
			}
		}
	}
}