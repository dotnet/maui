using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class MeasureInvalidationTests : BaseTestFixture
	{
		private class MeasureInvalidatedExpectations
		{
			public IReadOnlyList<(View Target, InvalidationTrigger Trigger)> Events { get; init; }
		}

		[Fact]
		public void VisualElementWhenHandlerIsSetInvalidatesWithRendererReady()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.Handler = null;
					view.Handler = new HandlerStub();
				},
				out var element,
				out var parent);

			Assert.Equal(1, element.InvalidateMeasureCount);
			Assert.Equal(0, parent.ComputeConstraintCount);
			Assert.Equal(0, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events, 
				(element, InvalidationTrigger.RendererReady),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenSizeRequestIsSetInvalidatesWithSizeRequestChanged()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.HeightRequest = 100;
				},
				out var element,
				out var parent);

			Assert.Equal(1, element.InvalidateMeasureCount);
			Assert.Equal(0, parent.ComputeConstraintCount);
			Assert.Equal(0, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events, 
				(element, InvalidationTrigger.SizeRequestChanged),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenMeasureChangedInvalidatesWithMeasureChanged()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.PlatformSizeChanged();
				},
				out var element,
				out var parent);

			Assert.Equal(1, element.InvalidateMeasureCount);
			Assert.Equal(0, parent.ComputeConstraintCount);
			Assert.Equal(0, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events, 
				(element, InvalidationTrigger.MeasureChanged),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenIsVisibleChangesInvalidatesWithUndefined()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.IsVisible = false;
				},
				out var element,
				out var parent);

			Assert.Equal(0, parent.ComputeConstraintCount);
			AssertEvents(expectations.Events, 
				(element, InvalidationTrigger.Undefined),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenHorizontalOptionsChangesInvalidatesWithHorizontalOptionsChanged()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.HorizontalOptions = LayoutOptions.Center;
				},
				out var element,
				out var parent);

			Assert.Equal(0, element.InvalidateMeasureCount);
			Assert.Equal(1, parent.ComputeConstraintCount);
			Assert.Equal(1, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events, 
				(element, InvalidationTrigger.HorizontalOptionsChanged),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenVerticalOptionsChangesInvalidatesWithVerticalOptionsChanged()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.VerticalOptions = LayoutOptions.Center;
				},
				out var element,
				out var parent);

			Assert.Equal(0, element.InvalidateMeasureCount);
			Assert.Equal(1, parent.ComputeConstraintCount);
			Assert.Equal(1, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events, 
				(element, InvalidationTrigger.VerticalOptionsChanged),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenNotApplyingBindingsTriggerMultipleInvalidations()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.HeightRequest = 100;
					view.HorizontalOptions = LayoutOptions.Center;
				},
				out var element,
				out var parent);

			Assert.Equal(1, element.InvalidateMeasureCount);
			Assert.Equal(1, parent.ComputeConstraintCount);
			Assert.Equal(1, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events,
				(element, InvalidationTrigger.SizeRequestChanged),
				(parent, InvalidationTrigger.MeasureChanged),
				(element, InvalidationTrigger.HorizontalOptionsChanged),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenApplyingBindingsTriggerSingleInvalidation()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.SetBinding(VisualElement.HeightRequestProperty, new Binding("HeightRequest"));
					view.SetBinding(View.HorizontalOptionsProperty, new Binding("HorizontalOptions"));
					view.BindingContext = new { HeightRequest = 100, HorizontalOptions = LayoutOptions.Center };
				},
				out var element,
				out var parent);

			Assert.Equal(1, element.InvalidateMeasureCount);
			Assert.Equal(1, parent.ComputeConstraintCount);
			Assert.Equal(0, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events,
				(element, InvalidationTrigger.SizeRequestChanged),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenPropagatingBindingsTriggerSingleInvalidation()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.SetBinding(VisualElement.HeightRequestProperty, new Binding("HeightRequest"));
					view.SetBinding(View.HorizontalOptionsProperty, new Binding("HorizontalOptions"));
					parentView.BindingContext = new { HeightRequest = 100, HorizontalOptions = LayoutOptions.Center };
				},
				out var element,
				out var parent);

			Assert.Equal(1, element.InvalidateMeasureCount);
			Assert.Equal(1, parent.ComputeConstraintCount);
			Assert.Equal(0, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events,
				(element, InvalidationTrigger.SizeRequestChanged),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenPropagatingBindingsTriggerSingleInvalidationOnParent()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					view.SetBinding(View.MarginProperty, new Binding("Margin"));
					view.SetBinding(View.HorizontalOptionsProperty, new Binding("HorizontalOptions"));
					parentView.BindingContext = new { Margin = 10, HorizontalOptions = LayoutOptions.Center };
				},
				out var element,
				out var parent);

			Assert.Equal(0, element.InvalidateMeasureCount);
			Assert.Equal(1, parent.ComputeConstraintCount);
			Assert.Equal(1, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events,
				(element, InvalidationTrigger.MarginChanged),
				(parent, InvalidationTrigger.MeasureChanged));
		}

		[Fact]
		public void VisualElementWhenApplyingAndPropagatingBindingsTriggerSingleInvalidationOnParent()
		{
			var expectations = TestMeasureInvalidation(
				(view, parentView) =>
				{
					parentView.SetBinding(VisualElement.WidthRequestProperty, new Binding("WidthRequest"));
					view.SetBinding(VisualElement.HeightRequestProperty, new Binding("HeightRequest"));
					view.SetBinding(View.HorizontalOptionsProperty, new Binding("HorizontalOptions"));
					parentView.BindingContext = new { WidthRequest = 10, HeightRequest = 100, HorizontalOptions = LayoutOptions.Center };
				},
				out var element,
				out var parent);

			Assert.Equal(1, element.InvalidateMeasureCount);
			Assert.Equal(2, parent.ComputeConstraintCount); // setting WidthRequest on parent triggers children constraint computation
			Assert.Equal(1, parent.InvalidateMeasureCount);
			AssertEvents(expectations.Events,
				(element, InvalidationTrigger.SizeRequestChanged),
				(parent, InvalidationTrigger.SizeRequestChanged));
		}

		static MeasureInvalidatedExpectations TestMeasureInvalidation(
			Action<ButtonStub, LayoutStub> act,
			out ButtonStub element,
			out LayoutStub parent)
		{
			// Arrange
			element = new ButtonStub();
			parent = new LayoutStub
			{
				element
			};
			parent.Handler = new HandlerStub();
			element.Handler = new HandlerStub();

			// reset counters
			parent.ComputeConstraintCount = 0;
			parent.InvalidateMeasureCount = 0;
			element.InvalidateMeasureCount = 0;

			var events = new List<(View Target, InvalidationTrigger Trigger)>();
			element.MeasureInvalidated += (s, e) => events.Add(((View)s, ((InvalidationEventArgs)e).Trigger));
			parent.MeasureInvalidated += (s, e) => events.Add(((View)s, ((InvalidationEventArgs)e).Trigger));

			// Act
			act(element, parent);

			return new MeasureInvalidatedExpectations { Events = events };
		}

		private static void AssertEvents(IEnumerable<(View Target, InvalidationTrigger Trigger)> actualEvents, params (View Target, InvalidationTrigger Trigger)[] expectedEvents)
		{
			Assert.Equal(expectedEvents, actualEvents);
		}

		class LayoutStub : VerticalStackLayout
		{
			public int ComputeConstraintCount { get; set; }
			public int InvalidateMeasureCount { get; set; }

			internal override void ComputeConstraintForView(View view)
			{
				base.ComputeConstraintForView(view);
				++ComputeConstraintCount;
			}

			protected override void InvalidateMeasureOverride()
			{
				base.InvalidateMeasureOverride();
				++InvalidateMeasureCount;
			}
		}

		class ButtonStub : Button
		{
			public int InvalidateMeasureCount { get; set; }
			
			protected override void InvalidateMeasureOverride()
			{
				base.InvalidateMeasureOverride();
				++InvalidateMeasureCount;
			}
		}
	}
}