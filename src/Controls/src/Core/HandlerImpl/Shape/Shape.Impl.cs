using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Shape
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (
				propertyName == HeightProperty.PropertyName ||
				propertyName == WidthProperty.PropertyName)
				UpdateSize();
		}

		void UpdateSize()
		{
			bool getBounds = HeightRequest <= 0 && WidthRequest <= 0;

			if (getBounds)
			{
				PathF path = GetPath();
				RectF pathBounds = path.GetBoundsByFlattening(1);

				if (Aspect != Stretch.None || pathBounds.IsEmpty)
				{
					HeightRequest = WidthRequest = double.NaN;
					
					return;
				}

				pathBounds.Width += (float)StrokeThickness;
				pathBounds.Height += (float)StrokeThickness;

				HeightRequest = pathBounds.Height;
				WidthRequest = pathBounds.Width;
			}
		}
	}
}