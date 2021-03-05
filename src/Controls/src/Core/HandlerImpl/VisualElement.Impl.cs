using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
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

		public bool IsArrangeValid { get; protected set; }

		public void Arrange(Rectangle bounds)
		{
			Layout(bounds);
		}

		void IFrameworkElement.Arrange(Rectangle bounds)
		{
			ArrangeOverride(bounds);
		}

		// ArrangeOverride provides a way to allow subclasses (e.g., Layout) to override Arrange even though
		// the interface has to be explicitly implemented to avoid conflict with the old Arrange method
		protected virtual void ArrangeOverride(Rectangle bounds)
		{
			if (IsArrangeValid)
				return;
			IsArrangeValid = true;

			var newRect = this.ComputeFrame(bounds);

			Bounds = newRect;
			Handler?.SetFrame(Bounds);
		}

		// TODO: MAUI
		// This is here to support layout calls from legacy code
		// This should go away once we get everything piping through
		// Maui based layout code
		public void Layout(Rectangle bounds)
		{
			if (Bounds != bounds)
			{
				Bounds = bounds;
				Handler?.SetFrame(Bounds);
			}
		}

		// TODO MAUI. Current MAUI layous don't
		// invalidate if the children change
		void InvalidateParentHack()
		{
			this.FindParentOfType<Page>().InvalidateMeasure();
		}

		void IFrameworkElement.InvalidateMeasure()
		{
			InvalidateMeasureOverride();
		}

		// ArrangeOverride provides a way to allow subclasses (e.g., Layout) to override InvalidateMeasure even though
		// the interface has to be explicitly implemented to avoid conflict with the VisualElement.InvalidateMeasure method
		protected virtual void InvalidateMeasureOverride()
		{
			if (!IsMeasureValid && !IsArrangeValid)
				return;

			IsMeasureValid = false;
			IsArrangeValid = false;
			InvalidateMeasure();
			InvalidateParentHack();
		}

		void IFrameworkElement.InvalidateArrange()
		{
			IsArrangeValid = false;
		}

		Size IFrameworkElement.Measure(double widthConstraint, double heightConstraint)
		{
			return MeasureOverride(widthConstraint, heightConstraint);
		}

		// ArrangeOverride provides a way to allow subclasses (e.g., Layout) to override Measure even though
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
	}
}
