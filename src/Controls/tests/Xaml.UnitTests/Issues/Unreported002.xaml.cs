using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

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

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void TypeConvertersOnAttachedBP(bool useCompiledXaml)
			{
				var p = new Unreported002(useCompiledXaml);
				Assert.AreEqual(new Rect(0.5, 0.5, 1, -1), AbsoluteLayoutCompat.GetLayoutBounds(p.label));
				Assert.AreEqual(new Rect(0.7, 0.7, 0.9, -1), Microsoft.Maui.Controls.AbsoluteLayout.GetLayoutBounds(p.label2));
			}
		}
	}
}