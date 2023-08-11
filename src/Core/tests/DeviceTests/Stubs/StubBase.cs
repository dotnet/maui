using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class StubBase : ElementStub, IStubBase
	{
		IElementHandler IElement.Handler
		{
			get => Handler;
			set => Handler = (IViewHandler)value;
		}

		public bool IsEnabled { get; set; } = true;

		public bool IsFocused { get; set; }

		public IList<IView> Children { get; set; } = new List<IView>();

		public Visibility Visibility { get; set; } = Visibility.Visible;

		public double Opacity { get; set; } = 1.0d;

		public Paint Background { get; set; }

		public Rect Frame { get; set; }

		public new IViewHandler Handler
		{
			get => (IViewHandler)base.Handler;
			set => base.Handler = value;
		}

		public IShape Clip { get; set; }

		public IShadow Shadow { get; set; }

		public Size DesiredSize { get; set; } = new Size(50, 50);

		public double Width { get; set; } = 50;

		public double Height { get; set; } = 50;

		public double MaximumWidth { get; set; } = Primitives.Dimension.Maximum;

		public double MaximumHeight { get; set; } = Primitives.Dimension.Maximum;

		public double MinimumWidth { get; set; } = Primitives.Dimension.Minimum;

		public double MinimumHeight { get; set; } = Primitives.Dimension.Minimum;

		public double TranslationX { get; set; }

		public double TranslationY { get; set; }

		public double Scale { get; set; } = 1d;

		public double ScaleX { get; set; } = 1d;

		public double ScaleY { get; set; } = 1d;

		public double Rotation { get; set; }

		public double RotationX { get; set; }

		public double RotationY { get; set; }

		public double AnchorX { get; set; } = .5d;

		public double AnchorY { get; set; } = .5d;

		public Thickness Margin { get; set; }

		public string AutomationId { get; set; }

		public FlowDirection FlowDirection { get; set; } = FlowDirection.LeftToRight;

		public LayoutAlignment HorizontalLayoutAlignment { get; set; }

		public LayoutAlignment VerticalLayoutAlignment { get; set; }

		public Semantics Semantics { get; set; } = new Semantics();

		public int ZIndex { get; set; }

		public bool InputTransparent { get; set; }

		public ToolTip ToolTip { get; set; }

		public Size Arrange(Rect bounds)
		{
			Frame = bounds;
			DesiredSize = bounds.Size;

			// If this view is attached to the visual tree then let's arrange it
			if (IsLoaded)
				Handler?.PlatformArrange(Frame);

			return DesiredSize;
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName] string propertyName = "",
			Action<T, T> onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			var oldValue = backingStore;
			backingStore = value;
			Handler?.UpdateValue(propertyName);
			onChanged?.Invoke(oldValue, value);
			return true;
		}

		public void InvalidateArrange()
		{
		}

		public void InvalidateMeasure()
		{
		}

		public bool Focus()
		{
			FocusRequest focusRequest = new FocusRequest();
			return Handler?.InvokeWithResult(nameof(IView.Focus), focusRequest) ?? false;
		}

		public void Unfocus()
		{
			IsFocused = false;
		}

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			if (Handler != null)
			{
				DesiredSize = Handler.GetDesiredSize(widthConstraint, heightConstraint);
				return DesiredSize;
			}

			return new Size(widthConstraint, heightConstraint);
		}

		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => this.Children.Cast<IVisualTreeElement>().ToList().AsReadOnly();

		IVisualTreeElement IVisualTreeElement.GetVisualParent() => this.Parent as IVisualTreeElement;

		PropertyMapper IPropertyMapperView.GetPropertyMapperOverrides() =>
			PropertyMapperOverrides;

		public PropertyMapper PropertyMapperOverrides
		{
			get;
			set;
		}

		public bool IsLoaded
		{
			get
			{
				return (Handler as IPlatformViewHandler)?.PlatformView?.IsLoaded() == true;
			}
		}
	}
}
