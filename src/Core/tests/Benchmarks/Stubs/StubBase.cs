using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	public class StubBase : IFrameworkElement
	{
		public bool IsEnabled { get; set; } = true;

		public Visibility Visibility { get; set; } = Visibility.Visible;

		public double Opacity { get; set; } = 1.0d;

		public Paint Background { get; set; }

		public Rectangle Frame { get; set; } = new Rectangle(0, 0, 20, 20);

		public IViewHandler Handler { get; set; }

		public IFrameworkElement Parent { get; set; }

		public Size DesiredSize { get; set; } = new Size(20, 20);

		public bool IsMeasureValid { get; set; }

		public bool IsArrangeValid { get; set; }

		public double Width { get; set; }

		public double Height { get; set; }

		public Thickness Margin { get; set; }

		public string AutomationId { get; set; }

		public FlowDirection FlowDirection { get; set; }

		public LayoutAlignment HorizontalLayoutAlignment { get; set; }

		public LayoutAlignment VerticalLayoutAlignment { get; set; }

		public Semantics Semantics { get; set; } = new Semantics();

		public Size Arrange(Rectangle bounds)
		{
			Frame = bounds;
			DesiredSize = bounds.Size;
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

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			return new Size(widthConstraint, heightConstraint);
		}
	}
}
