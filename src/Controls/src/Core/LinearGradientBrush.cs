#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/LinearGradientBrush.xml" path="Type[@FullName='Microsoft.Maui.Controls.LinearGradientBrush']/Docs/*" />
	public class LinearGradientBrush : GradientBrush
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/LinearGradientBrush.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public LinearGradientBrush()
		{

		}

		/// <include file="../../docs/Microsoft.Maui.Controls/LinearGradientBrush.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public LinearGradientBrush(GradientStopCollection gradientStops)
		{
			GradientStops = gradientStops;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/LinearGradientBrush.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/LinearGradientBrush.xml" path="//Member[@MemberName='StartPoint']/Docs/*" />
		public Point StartPoint
		{
			get => (Point)GetValue(StartPointProperty);
			set => SetValue(StartPointProperty, value);
		}

		/// <summary>Bindable property for <see cref="EndPoint"/>.</summary>
		public static readonly BindableProperty EndPointProperty = BindableProperty.Create(
			nameof(EndPoint), typeof(Point), typeof(LinearGradientBrush), new Point(1, 1));

		/// <include file="../../docs/Microsoft.Maui.Controls/LinearGradientBrush.xml" path="//Member[@MemberName='EndPoint']/Docs/*" />
		public Point EndPoint
		{
			get => (Point)GetValue(EndPointProperty);
			set => SetValue(EndPointProperty, value);
		}
	}
}