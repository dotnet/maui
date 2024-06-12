using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui19560 : ContentPage
	{
		public Maui19560()
		{
			InitializeComponent();
		}

		public Maui19560(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void StylesAreAppliedToShadow(bool useCompiledXaml)
			{
				var layout = new Maui19560(useCompiledXaml);
				Assert.AreEqual(Colors.Red, ((SolidColorBrush)layout.label.Shadow.Brush).Color);
				Assert.AreEqual(0.5, layout.label.Shadow.Opacity);
				Assert.AreEqual(15, layout.label.Shadow.Radius);
				Assert.AreEqual(10, layout.label.Shadow.Offset.X);
				Assert.AreEqual(10, layout.label.Shadow.Offset.Y);
			}
		}
	}
}

