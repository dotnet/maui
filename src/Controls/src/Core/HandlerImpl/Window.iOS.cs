#nullable enable

using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal UIWindow? NativeWindow
		{
			get
			{
				if (Page?.Handler?.NativeView is UIView view)
				{
					return view.Window;
				}

				return null;
			}
		}
	}
}
