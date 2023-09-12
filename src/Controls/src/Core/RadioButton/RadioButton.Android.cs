using AView = Android.Views.View;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			if (radioButton.ResolveControlTemplate() != null)
			{
				if (handler.PlatformView is ContentViewGroup vg && handler.MauiContext != null)
				{
					// Cleanup the old view when reused
					vg.RemoveAllViews();

					if (handler.VirtualView.PresentedContent is IView view)
						vg.AddView(view.ToPlatform(handler.MauiContext));
				}

				return;
			}

			RadioButtonHandler.MapContent(handler, radioButton);
		}

		static AView? CreatePlatformView(ViewHandler<IRadioButton, AView> radioButton)
		{
			// If someone is using a completely different type for IRadioButton
			if (radioButton.VirtualView is not RadioButton rb)
				return null;

			if (rb.ResolveControlTemplate() == null)
			{
				return null;
			}

			var viewGroup = new ContentViewGroup(radioButton.Context)
			{
				CrossPlatformLayout = radioButton.VirtualView
			};

			return viewGroup;
		}
	}
}
