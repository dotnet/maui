#nullable disable
using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls.Shapes
{
	[ElementHandler(typeof(RoundRectangleHandler))]
	public sealed partial class RoundRectangle : Shape, IShape, IRoundRectangle
	{
		public RoundRectangle() : base()
		{
			Aspect = Stretch.Fill;
		}

		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(RoundRectangle), new CornerRadius());

		/// <summary>
		/// Gets or sets the corner radius for the round rectangle.
		/// </summary>
		/// <value>A <see cref="CornerRadius"/> value that specifies the radius for each corner of the round rectangle.</value>
		/// <remarks>When specifying corner radii, the order of values is top left, top right, bottom left, and bottom right.</remarks>
		public CornerRadius CornerRadius
		{
			set { SetValue(CornerRadiusProperty, value); }
			get { return (CornerRadius)GetValue(CornerRadiusProperty); }
		}
		double _fallbackWidth;
		double _fallbackHeight;

		internal override double WidthForPathComputation
		{
			get
			{
				var width = Width;

				return width == -1 ? _fallbackWidth : width;
			}
		}

		internal override double HeightForPathComputation
		{
			get
			{
				var height = Height;

				return height == -1 ? _fallbackHeight : height;
			}
		}

		// TODO this should move to a remapped mapper
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == CornerRadiusProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		public override PathF GetPath()
		{
			float w = (float)WidthForPathComputation;
			float h = (float)HeightForPathComputation;

			float topLeftCornerRadius = (float)CornerRadius.TopLeft;
			float topRightCornerRadius = (float)CornerRadius.TopRight;
			float bottomLeftCornerRadius = (float)CornerRadius.BottomLeft;
			float bottomRightCornerRadius = (float)CornerRadius.BottomRight;

			var path = new PathF();

			path.AppendRoundedRectangle(0, 0, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			return path;
		}

		PathF IShape.PathForBounds(Graphics.Rect viewBounds)
		{
			_fallbackHeight = viewBounds.Height;
			_fallbackWidth = viewBounds.Width;

			var path = GetPath();

			TransformPathForBounds(path, viewBounds);

			return path;
		}

		internal PathF GetInnerPath(float strokeThickness)
		{
			float w = (float)(WidthForPathComputation - strokeThickness);
			float h = (float)(HeightForPathComputation - strokeThickness);
			float x = strokeThickness / 2;
			float y = strokeThickness / 2;

			float topLeftCornerRadius = (float)Math.Max(0, CornerRadius.TopLeft - strokeThickness);
			float topRightCornerRadius = (float)Math.Max(0, CornerRadius.TopRight - strokeThickness);
			float bottomLeftCornerRadius = (float)Math.Max(0, CornerRadius.BottomLeft - strokeThickness);
			float bottomRightCornerRadius = (float)Math.Max(0, CornerRadius.BottomRight - strokeThickness);

			var path = new PathF();

			path.AppendRoundedRectangle(x, y, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			return path;
		}

		PathF IRoundRectangle.InnerPathForBounds(Rect viewBounds, float strokeThickness)
		{
			_fallbackHeight = viewBounds.Height;
			_fallbackWidth = viewBounds.Width;

			var path = GetInnerPath(strokeThickness);

			TransformPathForBounds(path, viewBounds);

			return path;
		}
	}
}