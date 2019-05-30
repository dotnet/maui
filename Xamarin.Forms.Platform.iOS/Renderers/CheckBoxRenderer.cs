using System;

namespace Xamarin.Forms.Platform.iOS
{
	public class CheckBoxRenderer : CheckBoxRendererBase<FormsCheckBox>
	{
		public CheckBoxRenderer()
		{
		}


        protected override FormsCheckBox CreateNativeControl()
        {
            return new FormsCheckBox();
        }

    }
}
