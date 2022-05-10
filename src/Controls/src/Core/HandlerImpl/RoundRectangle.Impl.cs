using System.Runtime.CompilerServices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class RoundRectangle : IShape
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == CornerRadiusProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		public override PathF GetPath()
		{
			var path = new PathF();

			float x = (float)StrokeThickness / 2;
			float y = (float)StrokeThickness / 2;

			float w = (float)(Width - StrokeThickness);
			float h = (float)(Height - StrokeThickness);

			double density = 1.0d;
#if ANDROID
			density = DeviceDisplay.MainDisplayInfo.Density;
#endif
			float topLeftCornerRadius = (float)(CornerRadius.TopLeft * density);
			float topRightCornerRadius = (float)(CornerRadius.TopRight * density);
			float bottomLeftCornerRadius = (float)(CornerRadius.BottomLeft * density);
			float bottomRightCornerRadius = (float)(CornerRadius.BottomRight * density);

			path.AppendRoundedRectangle(x, y, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			return path;
		}
	}
}