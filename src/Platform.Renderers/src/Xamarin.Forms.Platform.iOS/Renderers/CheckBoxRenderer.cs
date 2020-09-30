using System;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	public class CheckBoxRenderer : CheckBoxRendererBase<FormsCheckBox>
	{
		[Preserve(Conditional = true)]
		public CheckBoxRenderer()
		{
		}


        protected override FormsCheckBox CreateNativeControl()
        {
            return new FormsCheckBox();
        }

    }
}
