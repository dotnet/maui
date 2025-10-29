using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Issue1250AspectRatioContainer : ContentView
	{
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(widthConstraint, widthConstraint * AspectRatio));
		}

		public double AspectRatio { get; set; }
	}

	public partial class Issue1250 : ContentPage
	{
		public Issue1250()
		{
			InitializeComponent();
		}

		public Issue1250(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void AddCustomElementInCollection(bool useCompiledXaml)
			{
				var page = new Issue1250(useCompiledXaml);
				var stack = page.stack;

				Assert.Equal(3, stack.Children.Count);
				Assert.IsType<Label>(stack.Children[0]);
				Assert.IsType<Issue1250AspectRatioContainer>(stack.Children[1]);
				Assert.IsType<Label>(stack.Children[2]);
			}
		}
	}
}