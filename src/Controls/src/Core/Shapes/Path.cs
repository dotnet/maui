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
		WeakGeometryChangedProxy _dataProxy;
		EventHandler _dataChanged;
		WeakNotifyPropertyChangedProxy _transformProxy;
		PropertyChangedEventHandler _transformChanged;

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
				 propertyChanging: (bindable, oldValue, newValue) =>
				 {
					 if (oldValue != null)
						 (bindable as Path)?.StopNotifyingDataChanges();
				 },
				 propertyChanged: (bindable, oldValue, newValue) =>
				 {
					 if (newValue != null)
						 (bindable as Path)?.NotifyDataChanges();
				 });

		/// <summary>Bindable property for <see cref="RenderTransform"/>.</summary>
		public static readonly BindableProperty RenderTransformProperty =
			BindableProperty.Create(nameof(RenderTransform), typeof(Transform), typeof(Path), null,
				propertyChanging: (bindable, oldValue, newValue) =>
				{
					if (oldValue != null)
						(bindable as Path)?.StopNotifyingTransformChanges();
				},
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					if (newValue != null)
						(bindable as Path)?.NotifyTransformChanges();
				});

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

		void NotifyDataChanges()
		{
			var data = Data;

			if (data != null)
			{
				_dataChanged ??= (sender, e) => OnPropertyChanged(nameof(Data));
				_dataProxy ??= new WeakGeometryChangedProxy();
				_dataProxy.Subscribe(data, _dataChanged);
			}
		}

		void StopNotifyingDataChanges()
		{
			_dataProxy?.Unsubscribe();
		}

		void NotifyTransformChanges()
		{
			var renderTransform = RenderTransform;

			if (renderTransform != null)
			{
				_transformChanged ??= OnTransformPropertyChanged;
				_transformProxy ??= new WeakNotifyPropertyChangedProxy();
				_transformProxy.Subscribe(renderTransform, _transformChanged);
			}
		}

		void StopNotifyingTransformChanges()
		{
			_transformProxy?.Unsubscribe();
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
