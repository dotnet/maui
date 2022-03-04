using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.UnitTests
{
	class ViewStub : IViewStub
	{
		IElementHandler IElement.Handler
		{
			get => Handler;
			set => Handler = (IViewHandler)value;
		}

		IElement IElement.Parent => Parent;

		public bool IsEnabled { get; set; }

		public bool IsFocused { get; set; }

		public Visibility Visibility { get; set; }

		public IShape Clip { get; set; }

		public IShadow Shadow { get; set; }

		public double Opacity { get; set; }

		public Rect Frame { get; set; }

		public IViewHandler Handler { get; set; }

		public IView Parent { get; set; }

		public Size DesiredSize { get; set; }

		public bool IsMeasureValid { get; set; }

		public bool IsArrangeValid { get; set; }

		public double Width { get; set; }

		public double Height { get; set; }

		public double MinimumHeight { get; set; }

		public double MinimumWidth { get; set; }

		public double MaximumHeight { get; set; }

		public double MaximumWidth { get; set; }

		public Thickness Margin { get; set; }

		public string AutomationId { get; set; }

		public FlowDirection FlowDirection { get; set; }

		public LayoutAlignment HorizontalLayoutAlignment { get; set; }

		public LayoutAlignment VerticalLayoutAlignment { get; set; }

		public Semantics Semantics { get; set; }

		public Paint Background { get; set; }

		public double TranslationX { get; set; }

		public double TranslationY { get; set; }

		public double Scale { get; set; }

		public double ScaleX { get; set; }

		public double ScaleY { get; set; }

		public double Rotation { get; set; }

		public double RotationX { get; set; }

		public double RotationY { get; set; }

		public double AnchorX { get; set; }

		public double AnchorY { get; set; }

		public int ZIndex { get; set; }

		public bool InputTransparent { get; set; }

		public Size Arrange(Rect bounds) => Size.Zero;

		public void InvalidateArrange() { }

		public void InvalidateMeasure() { }

		public bool Focus() => false;

		public void Unfocus() { }

		public Size Measure(double widthConstraint, double heightConstraint) =>
			Size.Zero;
	}
}