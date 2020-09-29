using System.ComponentModel;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms.Shapes
{
	[RenderWith(typeof(_PathRenderer))]
	public class Path : Shape
	{
		public static readonly BindableProperty DataProperty =
			 BindableProperty.Create(nameof(Data), typeof(Geometry), typeof(Path), null,
				 propertyChanged: OnGeometryPropertyChanged);

		public static readonly BindableProperty RenderTransformProperty =
			BindableProperty.Create(nameof(RenderTransform), typeof(Transform), typeof(Path), null,
				propertyChanged: OnTransformPropertyChanged);

		[TypeConverter(typeof(PathGeometryConverter))]
		public Geometry Data
		{
			set { SetValue(DataProperty, value); }
			get { return (Geometry)GetValue(DataProperty); }
		}

		public Transform RenderTransform
		{
			set { SetValue(RenderTransformProperty, value); }
			get { return (Transform)GetValue(RenderTransformProperty); }
		}

		static void OnGeometryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue != null)
			{
				(oldValue as Geometry).PropertyChanged -= (bindable as Path).OnGeometryPropertyChanged;
			}

			if (newValue != null)
			{
				(newValue as Geometry).PropertyChanged += (bindable as Path).OnGeometryPropertyChanged;
			}
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue != null)
			{
				(oldValue as Transform).PropertyChanged -= (bindable as Path).OnTransformPropertyChanged;
			}

			if (newValue != null)
			{
				(newValue as Transform).PropertyChanged += (bindable as Path).OnTransformPropertyChanged;
			}
		}

		void OnGeometryPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			OnPropertyChanged(nameof(Geometry));
		}

		void OnTransformPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == Transform.ValueProperty.PropertyName)
			{
				OnPropertyChanged(nameof(RenderTransform));
			}
		}
	}
}