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
		/// <summary>
		/// Initializes a new instance of the <see cref="Path"/> class.
		/// </summary>
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

		WeakGeometryChangedProxy _dataProxy;
		WeakNotifyPropertyChangedProxy _renderTransformProxy;
		EventHandler _dataChanged;
		PropertyChangedEventHandler _renderTransformChanged;

		~Path()
		{
			_dataProxy?.Unsubscribe();
			_renderTransformProxy?.Unsubscribe();
		}

		static void OnGeometryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (newValue is Geometry geometry)
				((Path)bindable).NotifyGeometryChanges(geometry);
			else
				((Path)bindable).StopNotifyingGeometryChanges();
		}

		void NotifyGeometryChanges(Geometry geometry)
		{
			_dataChanged ??= (sender, e) => OnPropertyChanged(nameof(Data));
			_dataProxy ??= new();
			_dataProxy.Subscribe(geometry, _dataChanged);
		}

		void StopNotifyingGeometryChanges()
		{
			_dataProxy?.Unsubscribe();
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (newValue is Transform transform)
				((Path)bindable).NotifyTransformChanges(transform);
			else
				((Path)bindable).StopNotifyingTransformChanges();
		}

		void NotifyTransformChanges(Transform transform)
		{
			_renderTransformChanged ??= (sender, e) =>
			{
				if (e.PropertyName == Transform.ValueProperty.PropertyName)
					OnPropertyChanged(nameof(RenderTransform));
			};
			_renderTransformProxy ??= new();
			_renderTransformProxy.Subscribe(transform, _renderTransformChanged);
		}

		void StopNotifyingTransformChanges()
		{
			_renderTransformProxy?.Unsubscribe();
		}

		class WeakGeometryChangedProxy : WeakEventProxy<Geometry, EventHandler>
		{
			void OnGeometryChanged(object sender, EventArgs e)
			{
				if (TryGetHandler(out var handler))
				{
					handler(sender, e);
				}
				else
				{
					Unsubscribe();
				}
			}

			public override void Subscribe(Geometry source, EventHandler handler)
			{
				if (TryGetSource(out var s))
				{
					s.PropertyChanged -= OnGeometryChanged;

					if (s is PathGeometry pg)
						pg.InvalidatePathGeometryRequested -= OnGeometryChanged;
				}

				source.PropertyChanged += OnGeometryChanged;

				if (source is PathGeometry pathGeometry)
					pathGeometry.InvalidatePathGeometryRequested += OnGeometryChanged;

				base.Subscribe(source, handler);
			}

			public override void Unsubscribe()
			{
				if (TryGetSource(out var s))
				{
					s.PropertyChanged -= OnGeometryChanged;

					if (s is PathGeometry pg)
						pg.InvalidatePathGeometryRequested -= OnGeometryChanged;
				}

				base.Unsubscribe();
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
