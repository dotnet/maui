using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class SizeExtensions 
	{ 
		public static Windows.Foundation.Size ToNative(this Size size) => new Windows.Foundation.Size(size.Width, size.Height);
	}
}