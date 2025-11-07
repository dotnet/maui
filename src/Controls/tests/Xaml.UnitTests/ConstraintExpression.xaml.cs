using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ConstraintExpression : ContentPage
{
	public ConstraintExpression() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void ConstantConstraint(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(ConstraintExpression));
			}
			var layout = new ConstraintExpression(inflator);
			var label = layout.constantConstraint;
			var constraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetWidthConstraint(label);
			Assert.NotNull(constraint);
			Assert.Equal(42, constraint.Compute(null));
		}

		[Theory]
		[Values]
		public void ConstraintRelativeToParent(XamlInflator inflator)
		{
			var layout = new ConstraintExpression(inflator);
			layout.relativeLayout.Layout(new Rect(0, 0, 200, 200));
			var label = layout.constraintRelativeToParent;
			var constraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetWidthConstraint(label);
			Assert.NotNull(constraint);
			Assert.Equal(102, constraint.Compute(layout.relativeLayout));
		}

		[Theory]
		[Values]
		public void ContraintRelativeToView(XamlInflator inflator)
		{
			var layout = new ConstraintExpression(inflator)
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