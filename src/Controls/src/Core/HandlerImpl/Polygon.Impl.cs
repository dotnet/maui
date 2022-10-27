using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Polygon']/Docs/*" />
	public partial class Polygon : IShape
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == PointsProperty.PropertyName)
				this.HeightRequest = this.WidthRequest = double.NaN;

			if (propertyName == PointsProperty.PropertyName ||
				propertyName == FillRuleProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		public override PathF GetPath()
		{
			var path = new PathF();

			if (Points?.Count > 0)
			{
				path.MoveTo((float)Points[0].X, (float)Points[0].Y);

				for (int index = 1; index < Points.Count; index++)
					path.LineTo((float)Points[index].X, (float)Points[index].Y);

				path.Close();
			}

			return path;
		}
	}
}
