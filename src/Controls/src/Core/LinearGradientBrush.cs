#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="GradientBrush"/> that paints an area with a linear gradient.</summary>
	public class LinearGradientBrush : GradientBrush
	{
		/// <summary>Initializes a new instance of the <see cref="LinearGradientBrush"/> class.</summary>
		public LinearGradientBrush()
		{

		}

		/// <summary>Initializes a new instance of the <see cref="LinearGradientBrush"/> class with the specified gradient stops.</summary>
		/// <param name="gradientStops">The collection of gradient stops.</param>
		public LinearGradientBrush(GradientStopCollection gradientStops)
		{
			GradientStops = gradientStops;
		}

		/// <summary>Initializes a new instance of the <see cref="LinearGradientBrush"/> class with the specified gradient stops and points.</summary>
		/// <param name="gradientStops">The collection of gradient stops.</param>
		/// <param name="startPoint">The starting point of the gradient.</param>
		/// <param name="endPoint">The ending point of the gradient.</param>
		public LinearGradientBrush(GradientStopCollection gradientStops, Point startPoint, Point endPoint)
		{
			GradientStops = gradientStops;
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public override bool IsEmpty => base.IsEmpty;

		/// <summary>Bindable property for <see cref="StartPoint"/>.</summary>
		public static readonly BindableProperty StartPointProperty = BindableProperty.Create(
			nameof(StartPoint), typeof(Point), typeof(LinearGradientBrush), new Point(0, 0));

		/// <summary>Gets or sets the starting point of the gradient. This is a bindable property.</summary>
		public Point StartPoint
		{
			get => (Point)GetValue(StartPointProperty);
			set => SetValue(StartPointProperty, value);
		}

		/// <summary>Bindable property for <see cref="EndPoint"/>.</summary>
		public static readonly BindableProperty EndPointProperty = BindableProperty.Create(
			nameof(EndPoint), typeof(Point), typeof(LinearGradientBrush), new Point(1, 1));

		/// <summary>Gets or sets the ending point of the gradient. This is a bindable property.</summary>
		public Point EndPoint
		{
			get => (Point)GetValue(EndPointProperty);
			set => SetValue(EndPointProperty, value);
		}
	}
}