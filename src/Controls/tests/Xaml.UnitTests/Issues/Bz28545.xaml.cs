using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;
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

		public Bz28545(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void TypeConverterAreAppliedForSettersToAttachedBP(bool useCompiledXaml)
			{
				var layout = new Bz28545(useCompiledXaml);
				Assert.Equal(Colors.Pink, layout.label.TextColor);
				Assert.Equal(AbsoluteLayoutFlags.PositionProportional, AbsoluteLayout.GetLayoutFlags(layout.label));
				Assert.Equal(new Rect(1, 1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayout.GetLayoutBounds(layout.label));
			}
		}
	}
}