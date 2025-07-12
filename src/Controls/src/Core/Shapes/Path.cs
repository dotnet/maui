#nullable disable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Path']/Docs/*" />
	public sealed partial class Path : Shape, IShape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Path() : base()
		{
		}

		public Path(Geometry data) : this()
		{
			Data = data;
		}

		/// <summary>Bindable property for <see cref="Data"/>.</summary>
		public static readonly BindableProperty DataProperty =
			 BindableProperty.Create(nameof(Data), typeof(Geometry), typeof(Path), null,
				 propertyChanged: OnGeometryPropertyChanged);

		/// <summary>Bindable property for <see cref="RenderTransform"/>.</summary>
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

		// TODO this should move to a remapped mapper
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == DataProperty.PropertyName)
			{
				Handler?.UpdateValue(nameof(IShapeView.Shape));
			}
		}

		public override PathF GetPath()
		{
			var path = new PathF();

			Data?.AppendPath(path);

			return path;
		}
	}
}
