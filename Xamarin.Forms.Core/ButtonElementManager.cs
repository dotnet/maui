using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Xamarin.Forms
{
	internal static class ButtonElementManager
	{
		public const string PressedVisualState = "Pressed";

		public static void CommandChanged(IButtonController sender)
		{
			if (sender.Command != null)
			{
				CommandCanExecuteChanged(sender, EventArgs.Empty);
			}
			else
			{
				sender.IsEnabledCore = true;
			}
		}

		public static void CommandCanExecuteChanged(object sender, EventArgs e)
		{
			IButtonController ButtonElementManager = (IButtonController)sender;
			ICommand cmd = ButtonElementManager.Command;
			if (cmd != null)
			{
				ButtonElementManager.IsEnabledCore = cmd.CanExecute(ButtonElementManager.CommandParameter);
			}
		}


		public static void ElementClicked(VisualElement visualElement, IButtonController ButtonElementManager)
		{
			if (visualElement.IsEnabled == true)
			{
				ButtonElementManager.Command?.Execute(ButtonElementManager.CommandParameter);
				ButtonElementManager.PropagateUpClicked();
			}
		}

		public static void ElementPressed(VisualElement visualElement, IButtonController ButtonElementManager)
		{
			if (visualElement.IsEnabled == true)
			{
				ButtonElementManager.SetIsPressed(true);
				visualElement.ChangeVisualStateInternal();
				ButtonElementManager.PropagateUpPressed();
			}
		}

		public static void ElementReleased(VisualElement visualElement, IButtonController ButtonElementManager)
		{
			if (visualElement.IsEnabled == true)
			{
				ButtonElementManager.SetIsPressed(false);
				visualElement.ChangeVisualStateInternal();
				ButtonElementManager.PropagateUpReleased();
			}
		}


	}
}
