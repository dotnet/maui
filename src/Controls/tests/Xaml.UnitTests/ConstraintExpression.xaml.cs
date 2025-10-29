using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class ConstraintExpression : ContentPage
	{
		public ConstraintExpression()
		{
			InitializeComponent();
		}

		public ConstraintExpression(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void ConstantConstraint(bool useCompiledXaml)
			{
				var layout = new ConstraintExpression(useCompiledXaml);
				var label = layout.constantConstraint;
				var constraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetWidthConstraint(label);
				Assert.NotNull(constraint);
				Assert.Equal(42, constraint.Compute(null));
			}

			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void ConstraintRelativeToParent(bool useCompiledXaml)
			{
				var layout = new ConstraintExpression(useCompiledXaml);
				layout.relativeLayout.Layout(new Rect(0, 0, 200, 200));
				var label = layout.constraintRelativeToParent;
				var constraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetWidthConstraint(label);
				Assert.NotNull(constraint);
				Assert.Equal(102, constraint.Compute(layout.relativeLayout));
			}

			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void ContraintRelativeToView(bool useCompiledXaml)
			{
				var layout = new ConstraintExpression(useCompiledXaml)
				{
					IsPlatformEnabled = true
				};
				layout.relativeLayout.Layout(new Rect(0, 0, 200, 100));
				layout.foo.Layout(new Rect(5, 5, 190, 25));

				var label = layout.constraintRelativeToView;
				var constraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetWidthConstraint(label);
				Assert.NotNull(constraint);
				Assert.Equal(97, constraint.Compute(layout.relativeLayout));
			}
		}
	}
}