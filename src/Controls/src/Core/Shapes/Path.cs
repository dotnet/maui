using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Path']/Docs/*" />
	public sealed partial class Path : Shape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Path() : base()
		{
		}

		public Path(Geometry data) : this()
		{
			Data = data;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="//Member[@MemberName='DataProperty']/Docs/*" />
		public static readonly BindableProperty DataProperty =
			 BindableProperty.Create(nameof(Data), typeof(Geometry), typeof(Path), null,
				 propertyChanged: OnGeometryPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="//Member[@MemberName='RenderTransformProperty']/Docs/*" />
		public static readonly BindableProperty RenderTransformProperty =
			BindableProperty.Create(nameof(RenderTransform), typeof(Transform), typeof(Path), null,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="//Member[@MemberName='Data']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(PathGeometryConverter))]
		public Geometry Data
		{
			set { SetValue(DataProperty, value); }
			get { return (Geometry)GetValue(DataProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="//Member[@MemberName='RenderTransform']/Docs/*" />
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

				if (oldValue is PathGeometry pathGeometry)
					pathGeometry.InvalidatePathGeometryRequested -= (bindable as Path).OnInvalidatePathGeometryRequested;
			}

			if (newValue != null)
			{
				(newValue as Geometry).PropertyChanged += (bindable as Path).OnGeometryPropertyChanged;

				if (newValue is PathGeometry pathGeometry)
					pathGeometry.InvalidatePathGeometryRequested += (bindable as Path).OnInvalidatePathGeometryRequested;
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
			OnPropertyChanged(nameof(Data));
		}

		void OnInvalidatePathGeometryRequested(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(Data));
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
