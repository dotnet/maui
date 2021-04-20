using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement : IFrameworkElement
	{
		private IViewHandler _handler;

		public Rectangle Frame => Bounds;

		public IViewHandler Handler
		{
			get => _handler;
			set
			{
				_handler = value;
				IsPlatformEnabled = _handler != null;
			}
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			(Handler)?.UpdateValue(propertyName);
		}

		IFrameworkElement IFrameworkElement.Parent => Parent as IView;

		public Size DesiredSize { get; protected set; }

		public virtual bool IsMeasureValid { get; protected set; }

		public virtual bool IsArrangeValid { get; protected set; }

		public void Arrange(Rectangle bounds)
		{
			Layout(bounds);
		}

		Size IFrameworkElement.Arrange(Rectangle bounds)
		{
			return ArrangeOverride(bounds);
		}

		// ArrangeOverride provides a way to allow subclasses (e.g., Layout) to override Arrange even though
		// the interface has to be explicitly implemented to avoid conflict with the old Arrange method
		protected virtual Size ArrangeOverride(Rectangle bounds)
		{
			if (IsArrangeValid)
			{
				return bounds.Size;
			}

			IsArrangeValid = true;

			// Setting Bounds here is equivalent to setting the Frame
			Bounds = this.ComputeFrame(bounds);

			return Frame.Size;
		}

		public void Layout(Rectangle bounds)
		{
			Bounds = bounds;
		}

		void IFrameworkElement.InvalidateMeasure()
		{
			InvalidateMeasureOverride();
		}

		// InvalidateMeasureOverride provides a way to allow subclasses (e.g., Layout) to override InvalidateMeasure even though
		// the interface has to be explicitly implemented to avoid conflict with the VisualElement.InvalidateMeasure method
		protected virtual void InvalidateMeasureOverride()
		{
			if (!IsMeasureValid && !IsArrangeValid)
				return;

			IsMeasureValid = false;
			IsArrangeValid = false;
			InvalidateMeasure();
		}

		void IFrameworkElement.InvalidateArrange()
		{
			IsArrangeValid = false;
		}

		Size IFrameworkElement.Measure(double widthConstraint, double heightConstraint)
		{
			return MeasureOverride(widthConstraint, heightConstraint);
		}

		// MeasureOverride provides a way to allow subclasses (e.g., Layout) to override Measure even though
		// the interface has to be explicitly implemented to avoid conflict with the old Measure method
		protected virtual Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			if (!IsMeasureValid)
			{
				DesiredSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);
			}

			IsMeasureValid = true;
			return DesiredSize;
		}

		Maui.FlowDirection IFrameworkElement.FlowDirection => FlowDirection.ToPlatformFlowDirection();
		Primitives.LayoutAlignment IFrameworkElement.HorizontalLayoutAlignment => default;
		Primitives.LayoutAlignment IFrameworkElement.VerticalLayoutAlignment => default;

		Maui.Semantics _semantics;
		Maui.Semantics IFrameworkElement.Semantics
		{
			get => _semantics;
		}

		// We don't want to initialize Semantics until someone explicitly 
		// wants to modify some aspect of the semantics class
		internal Semantics SetupSemantics() =>
			_semantics ??= new Maui.Semantics();

		double IFrameworkElement.Width { get => WidthRequest; }
		double IFrameworkElement.Height { get => HeightRequest; }
	}
}
