using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class SizeExtensions 
	{ 
		public static global::Windows.Foundation.Size ToNative(this Size size) => new global::Windows.Foundation.Size(size.Width, size.Height);
	}
}