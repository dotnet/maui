#nullable disable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A shape that can draw complex geometries defined by a <see cref="PathGeometry"/>.
	/// </summary>
	public sealed partial class Path : Shape, IShape
	{
		// Subscribe to the assigned Data geometry / RenderTransform via weak proxies so a shared,
		// long-lived Geometry/Transform does not root the transient Path (and its visual tree)
		// through a plain multicast-delegate subscription. See issue #36375.
		readonly WeakNotifyPropertyChangedProxy _dataProxy = new();
		readonly WeakNotifyPropertyChangedProxy _transformProxy = new();
		PropertyChangedEventHandler _dataChangedHandler;
		PropertyChangedEventHandler _transformChangedHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="Path"/> class.
		/// </summary>
		public Path() : base()
		{
		}

		~Path()
		{
			_dataProxy?.Unsubscribe();
			_transformProxy?.Unsubscribe();
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

		/// <summary>
		/// Gets or sets the <see cref="Geometry"/> that specifies the shape to be drawn. This is a bindable property.
		/// </summary>
		[System.ComponentModel.TypeConverter(typeof(PathGeometryConverter))]
		public Geometry Data
		{
			set { SetValue(DataProperty, value); }
			get { return (Geometry)GetValue(DataProperty); }
		}

		/// <summary>
		/// Gets or sets the <see cref="Transform"/> applied to the path geometry. This is a bindable property.
		/// </summary>
		public Transform RenderTransform
		{
			set { SetValue(RenderTransformProperty, value); }
			get { return (Transform)GetValue(RenderTransformProperty); }
		}

		static void OnGeometryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var path = bindable as Path;

			if (oldValue is PathGeometry oldPathGeometry)
				oldPathGeometry.InvalidatePathGeometryRequested -= path.OnInvalidatePathGeometryRequested;

			path._dataProxy.Unsubscribe();

			if (newValue is Geometry newGeometry)
			{
				path._dataChangedHandler ??= path.OnGeometryPropertyChanged;
				path._dataProxy.Subscribe(newGeometry, path._dataChangedHandler);

				if (newValue is PathGeometry newPathGeometry)
					newPathGeometry.InvalidatePathGeometryRequested += path.OnInvalidatePathGeometryRequested;
			}
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var path = bindable as Path;

			path._transformProxy.Unsubscribe();

			if (newValue is Transform newTransform)
			{
				path._transformChangedHandler ??= path.OnTransformPropertyChanged;
				path._transformProxy.Subscribe(newTransform, path._transformChangedHandler);
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
