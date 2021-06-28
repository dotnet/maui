#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
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
