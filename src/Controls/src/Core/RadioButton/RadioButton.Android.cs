using AView = Android.Views.View;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		ContentViewGroup? contentViewGroup { get; set; }

		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			if (radioButton.ResolveControlTemplate() != null)
			{
				if (handler.PlatformView is ContentViewGroup vg && handler.MauiContext != null)
				{
					radioButton.contentViewGroup = vg;

					// Cleanup the old view when reused
					vg.RemoveAllViews();

					if (handler.VirtualView.PresentedContent is IView view)
						vg.AddView(view.ToPlatform(handler.MauiContext));

					// Ensure the ContentViewGroup is focusable and can handle key events
					vg.Focusable = true;
					vg.KeyPress += radioButton.OnKeyPressed;
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

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler is null && contentViewGroup is not null)
			{
				contentViewGroup.KeyPress -= OnKeyPressed;
			}
		}

		void OnKeyPressed(object? sender, AView.KeyEventArgs e)
		{
			e.Handled = false;
			if (e?.Event is not null && e.Event.Action == Android.Views.KeyEventActions.Down &&
			   (e.Event.KeyCode == Android.Views.Keycode.Enter || e.Event.KeyCode == Android.Views.Keycode.Space))
			{
				IsChecked = !IsChecked;
			}
		}
	}
}
