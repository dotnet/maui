using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Path']/Docs" />
	public partial class Path : IShape
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == DataProperty.PropertyName ||
				propertyName == RenderTransformProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Path.xml" path="//Member[@MemberName='GetPath']/Docs" />
		public override PathF GetPath()
		{
			var path = new PathF();

			if (Data != null)
				Data.AppendPath(path);

			return path;
		}
	}
}
