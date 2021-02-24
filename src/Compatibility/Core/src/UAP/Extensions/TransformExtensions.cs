using Microsoft.Maui.Controls.Shapes;

#if WINDOWS_UWP
using WMatrix = Microsoft.UI.Xaml.Media.Matrix;
using WMatrixTransform = Microsoft.UI.Xaml.Media.MatrixTransform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
using WMatrix = System.Windows.Media.Matrix;
using WMatrixTransform = System.Windows.Media.MatrixTransform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
#endif
{
	public static class TransformExtensions
	{
		public static WMatrixTransform ToWindows(this Transform transform)
		{
			Matrix matrix = transform.Value;

			return new WMatrixTransform
			{
				Matrix = new WMatrix()
				{
					M11 = matrix.M11,
					M12 = matrix.M12,
					M21 = matrix.M21,
					M22 = matrix.M22,
					OffsetX = matrix.OffsetX,
					OffsetY = matrix.OffsetY
				}
			};
		}
	}
}
