using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	using AbsoluteLayoutCompat = Microsoft.Maui.Controls.Compatibility.AbsoluteLayout;

	public partial class Unreported002 : ContentPage
	{
		public Unreported002()
		{
			InitializeComponent();
		}

		public Unreported002(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void TypeConvertersOnAttachedBP(bool useCompiledXaml)
			{
				var p = new Unreported002(useCompiledXaml);
				Assert.Equal(new Rect(0.5, 0.5, 1, -1), AbsoluteLayoutCompat.GetLayoutBounds(p.label));
				Assert.Equal(new Rect(0.7, 0.7, 0.9, -1), Microsoft.Maui.Controls.AbsoluteLayout.GetLayoutBounds(p.label2));
			}
		}
	}
}