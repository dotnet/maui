using System;
using Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
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
