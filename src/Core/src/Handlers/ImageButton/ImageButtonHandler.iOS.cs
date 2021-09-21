using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, UIButton>
	{
		protected override UIButton CreateNativeView()
		{
			return new UIButton(UIButtonType.System);
		}

		void OnSetImageSource(UIImage? obj)
		{
			NativeView.ImageView.Image = obj;
		}

		protected override void DisconnectHandler(UIButton nativeView)
		{
			base.DisconnectHandler(nativeView);
			SourceLoader.Reset();
		}
	}
}
