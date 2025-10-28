using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public abstract class Gh3260MyGLayout<T> : Controls.Compatibility.Layout<T> where T : View
	{
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			throw new NotImplementedException();
		}
	}

	public class Gh3260MyLayout : Gh3260MyGLayout<View>
	{
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Gh3260 : ContentPage
	{
		public Gh3260()
		{
			InitializeComponent();
		}

		public Gh3260(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(false), TestCase(true)]
			public void AssignContentWithNoContentAttributeDoesNotThrow(bool useCompiledXaml)
			{
				var layout = new Gh3260(useCompiledXaml);
				Assert.Equal(1, layout.mylayout.Children.Count);
				Assert.Equal(layout.label, layout.mylayout.Children[0]);
			}
		}
	}
}
