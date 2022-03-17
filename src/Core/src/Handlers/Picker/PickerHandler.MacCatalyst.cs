using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiPicker>
	{
		protected override MauiPicker CreatePlatformView()
		{	
			var platformPicker = new MauiPicker(null) { BorderStyle = UITextBorderStyle.RoundedRect };
	
			platformPicker.ShouldBeginEditing += (textField) => 
			{
				var alertController = CreateAlert(textField, null);
				var platformWindow = MauiContext?.GetPlatformWindow();
				platformWindow?.BeginInvokeOnMainThread(() =>
				{
					_ = platformWindow?.RootViewController?.PresentViewControllerAsync(alertController, true);
				});
                return false;
            };
	
			return platformPicker;
			//platformPicker.ShowsMenuAsPrimaryAction = true;
			//platformPicker.ChangesSelectionAsPrimaryAction = true; //monterey only
			//platformPicker.Menu = CreateMenu(null);
			//return CreateButtonWithMenu();
		}

		UIAlertController CreateAlert(UITextField uITextField, UIPickerView? pickerView)
		{
			var frame = new RectangleF(0, 0, 269, 240);
			pickerView = new UIPickerView(frame);
			pickerView.Model = new PickerSource(VirtualView);
			pickerView?.ReloadAllComponents();

			var okAlertController = UIAlertController.Create ("", "", UIAlertControllerStyle.ActionSheet);

    		okAlertController.AddAction (UIAlertAction.Create ("Done", UIAlertActionStyle.Default, action => 
			{
				var pickerSource = pickerView?.Model as PickerSource;
				var count = VirtualView?.GetCount() ?? 0;
				if (pickerSource != null && pickerSource.SelectedIndex == -1 && count > 0)
					UpdatePickerSelectedIndex(pickerView, 0);

				if (VirtualView?.SelectedIndex == -1 && count > 0)
				{
					(PlatformView as MauiPicker)?.SetSelectedIndex(VirtualView, 0);
				}

				UpdatePickerFromPickerSource(pickerSource);
				uITextField.ResignFirstResponder();
			}));
			
			if(okAlertController.View != null && pickerView != null)
			{
				okAlertController.View.AddSubview(pickerView);
				var height = NSLayoutConstraint.Create(okAlertController.View,  NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 350);
				okAlertController.View.AddConstraint(height);
			}
			
			UIPopoverPresentationController presentationPopover = okAlertController.PopoverPresentationController;
			if (presentationPopover!=null)
			{
    			presentationPopover.SourceView = uITextField;
				presentationPopover.SourceRect = uITextField.Bounds;
    			presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up | UIPopoverArrowDirection.Down;
			}

			return okAlertController;
		}

		UIButton CreateButtonWithMenu()
		{
			UIButton button = new UIButton(UIButtonType.RoundedRect);

        	button.SetTitle("Click Test", UIControlState.Normal);
        	button.SetTitleColor(UIColor.SystemRed, UIControlState.Normal);
        	button.ShowsMenuAsPrimaryAction = true;
			button.ChangesSelectionAsPrimaryAction = true; //monterey only
			button.Menu = CreateMenu(button);
			return button;
		}

		UIMenu CreateMenu(UIButton button)
        {
            
            //you can set the icon as the second parameter
            var Action1 = UIAction.Create("Action1",null,null,(arg)=> { 
            	button.SetTitle ("Action1", UIControlState.Normal);
            });

            var Action2 = UIAction.Create("Action2", null, null, (arg) => {
				button.SetTitle ("Action2", UIControlState.Normal);
            });

            var Action3 = UIAction.Create("Action3", null, null, (arg) => {
				button.SetTitle ("Action3", UIControlState.Normal);
            });

            var Menus = UIMenu.Create(new UIMenuElement[] {Action1,Action2,Action3 });
            return Menus;
        }
	}
}
