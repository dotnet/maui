#nullable disable
using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A shape that draws an ellipse or circle.
	/// </summary>
	public sealed partial class Ellipse : Shape, IShape
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Ellipse"/> class.
		/// </summary>
		public Ellipse() : base()
		{
			Aspect = Stretch.Fill;
		}

		// TODO this should move to a remapped mapper
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == XProperty.PropertyName ||
				propertyName == YProperty.PropertyName ||
				propertyName == WidthProperty.PropertyName ||
				propertyName == HeightProperty.PropertyName)
			{
				Handler?.UpdateValue(nameof(IShapeView.Shape));
			}
		}

		public override PathF GetPath()
		{
			var width = WidthForPathComputation;
			var height = HeightForPathComputation;

			var path = new PathF();

			var effectiveStroke = EffectiveStrokeThickness;
			float x = (float)effectiveStroke / 2;
			float y = (float)effectiveStroke / 2;
			float w = (float)Math.Max(0, width - effectiveStroke);
			float h = (float)Math.Max(0, height - effectiveStroke);

			path.AppendEllipse(x, y, w, h);

			return path;
		}
	}
}