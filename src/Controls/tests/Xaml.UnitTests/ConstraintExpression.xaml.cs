using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

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

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ConstantConstraint(bool useCompiledXaml)
			{
				var layout = new ConstraintExpression(useCompiledXaml);
				var label = layout.constantConstraint;
				var constraint = RelativeLayout.GetWidthConstraint(label);
				Assert.NotNull(constraint);
				Assert.AreEqual(42, constraint.Compute(null));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ConstraintRelativeToParent(bool useCompiledXaml)
			{
				var layout = new ConstraintExpression(useCompiledXaml);
				layout.relativeLayout.Layout(new Rectangle(0, 0, 200, 200));
				var label = layout.constraintRelativeToParent;
				var constraint = RelativeLayout.GetWidthConstraint(label);
				Assert.NotNull(constraint);
				Assert.AreEqual(102, constraint.Compute(layout.relativeLayout));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ContraintRelativeToView(bool useCompiledXaml)
			{
				var layout = new ConstraintExpression(useCompiledXaml)
				{
					IsPlatformEnabled = true
				};
				layout.relativeLayout.Layout(new Rectangle(0, 0, 200, 100));
				layout.foo.Layout(new Rectangle(5, 5, 190, 25));

				var label = layout.constraintRelativeToView;
				var constraint = RelativeLayout.GetWidthConstraint(label);
				Assert.NotNull(constraint);
				Assert.AreEqual(97, constraint.Compute(layout.relativeLayout));
			}
		}
	}
}