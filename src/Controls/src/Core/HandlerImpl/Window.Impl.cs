using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;


namespace Microsoft.Maui.Controls
{
    public partial class Window : IWindow
    {
        // TODO ezhart That there's a layout alignment here tells us this hierarchy needs work :) 
        public Primitives.LayoutAlignment HorizontalLayoutAlignment => Primitives.LayoutAlignment.Fill;

        // TODO ezhart super sus
        public Thickness Margin => Thickness.Zero;

		public IMauiContext MauiContext { get; set; }

		IPage IWindow.Page => Page;

        internal override void InvalidateMeasureInternal(InvalidationTrigger trigger)
        {
            IsArrangeValid = false;
            base.InvalidateMeasureInternal(trigger);
        }

        public override bool IsMeasureValid
        {
            get
            {
                return base.IsMeasureValid && Page.IsMeasureValid;
            }

            protected set => base.IsMeasureValid = value;
        }

        public override bool IsArrangeValid
        {
            get
            {
                return base.IsArrangeValid && Page.IsArrangeValid;
            }

            protected set => base.IsArrangeValid = value;
        }

        protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            if (Page is IFrameworkElement frameworkElement)
            {
                frameworkElement.Measure(widthConstraint, heightConstraint);
            }

            IsMeasureValid = true;
            return new Size(widthConstraint, heightConstraint);
        }

        protected override Size ArrangeOverride(Rectangle bounds)
        {
            if (IsArrangeValid)
            {
                return bounds.Size;
            }

            IsArrangeValid = true;

            // Update the Bounds (Frame) for this page
            Layout(bounds);

            if (Page is IFrameworkElement element)
            {
                element.Arrange(bounds);
                element.Handler?.SetFrame(element.Frame);
            }

            return Frame.Size;
        }

        protected override void InvalidateMeasureOverride()
        {
            base.InvalidateMeasureOverride();
            if (Page is IFrameworkElement frameworkElement)
            {
                frameworkElement.InvalidateMeasure();
            }
        }
    }
}
