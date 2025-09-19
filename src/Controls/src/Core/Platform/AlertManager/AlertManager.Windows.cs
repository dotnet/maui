using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class AlertManager
	{
		private partial IAlertManagerSubscription CreateSubscription(IMauiContext mauiContext)
		{
			var platformWindow = mauiContext.GetPlatformWindow();

			return new AlertRequestHelper(Window, platformWindow);
		}

		internal sealed partial class AlertRequestHelper
		{
			Task<bool>? CurrentAlert;
			Task<string?>? CurrentPrompt;

			internal AlertRequestHelper(Window virtualView, UI.Xaml.Window platformView)
			{
				VirtualView = virtualView;
				PlatformView = platformView;
			}

			public Window VirtualView { get; }

			public UI.Xaml.Window PlatformView { get; }

			// TODO: This method is obsolete in .NET 10 and will be removed in .NET11.
			public partial void OnPageBusy(Page sender, bool enabled)
			{
				// TODO: Wrap the pages in a Canvas, and dynamically add a ProgressBar
			}

			public async partial void OnAlertRequested(Page sender, AlertArguments arguments)
			{
				if (!PageIsInThisWindow(sender))
					return;

				string content = arguments.Message ?? string.Empty;
				string title = arguments.Title ?? string.Empty;

				var alertDialog = new AlertDialog
				{
					Content = content,
					Title = title,
					VerticalScrollBarVisibility = UI.Xaml.Controls.ScrollBarVisibility.Auto
				};

				if (PlatformView.Content is FrameworkElement windowContent)
				{
					alertDialog.RequestedTheme = windowContent.RequestedTheme;
				}

				if (arguments.FlowDirection == FlowDirection.RightToLeft)
				{
					alertDialog.FlowDirection = UI.Xaml.FlowDirection.RightToLeft;
				}
				else if (arguments.FlowDirection == FlowDirection.LeftToRight)
				{
					alertDialog.FlowDirection = UI.Xaml.FlowDirection.LeftToRight;
				}
				else
				{
					if (sender is IVisualElementController visualElementController)
					{
						if (visualElementController.EffectiveFlowDirection.IsRightToLeft())
							alertDialog.FlowDirection = UI.Xaml.FlowDirection.RightToLeft;
						else if (visualElementController.EffectiveFlowDirection.IsLeftToRight())
							alertDialog.FlowDirection = UI.Xaml.FlowDirection.LeftToRight;
					}
				}

				if (arguments.Cancel != null)
					alertDialog.SecondaryButtonText = arguments.Cancel;

				if (arguments.Accept != null)
					alertDialog.PrimaryButtonText = arguments.Accept;

				// This is a temporary workaround
				alertDialog.XamlRoot = PlatformView.Content.XamlRoot;

				var currentAlert = CurrentAlert;

				while (currentAlert != null)
				{
					await currentAlert;
					currentAlert = CurrentAlert;
				}

				CurrentAlert = ShowAlert(alertDialog);
				arguments.SetResult(await CurrentAlert.ConfigureAwait(false));
				CurrentAlert = null;
			}

			public async partial void OnPromptRequested(Page sender, PromptArguments arguments)
			{
				if (!PageIsInThisWindow(sender))
					return;

				var promptDialog = new PromptDialog
				{
					Title = arguments.Title ?? string.Empty,
					Message = arguments.Message ?? string.Empty,
					Input = arguments.InitialValue ?? string.Empty,
					Placeholder = arguments.Placeholder ?? string.Empty,
					MaxLength = arguments.MaxLength >= 0 ? arguments.MaxLength : 0,
					InputScope = arguments.Keyboard.ToInputScope(),
					DefaultButton = ContentDialogButton.Primary
				};

				if (PlatformView.Content is FrameworkElement windowContent)
				{
					promptDialog.RequestedTheme = windowContent.RequestedTheme;
				}

				if (arguments.Cancel != null)
					promptDialog.SecondaryButtonText = arguments.Cancel;

				if (arguments.Accept != null)
					promptDialog.PrimaryButtonText = arguments.Accept;

				var currentAlert = CurrentPrompt;

				while (currentAlert != null)
				{
					await currentAlert;
					currentAlert = CurrentPrompt;
				}

				// This is a temporary workaround
				promptDialog.XamlRoot = PlatformView.Content.XamlRoot;

				CurrentPrompt = ShowPrompt(promptDialog);
				arguments.SetResult(await CurrentPrompt.ConfigureAwait(false));
				CurrentPrompt = null;
			}

			public partial void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
			{
				if (!PageIsInThisWindow(sender))
					return;

				bool userDidSelect = false;

				if (arguments.FlowDirection == FlowDirection.MatchParent)
				{
					if (sender is IVisualElementController visualElementController)
					{
						if (visualElementController.EffectiveFlowDirection.IsRightToLeft())
							arguments.FlowDirection = FlowDirection.RightToLeft;
						else if (visualElementController.EffectiveFlowDirection.IsLeftToRight())
							arguments.FlowDirection = FlowDirection.LeftToRight;
					}
				}

				var actionSheetContent = new ActionSheetContent(arguments);

				var actionSheet = new Flyout
				{
					FlyoutPresenterStyle = (UI.Xaml.Style)UI.Xaml.Application.Current.Resources["MauiFlyoutPresenterStyle"],
					Placement = UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Full,
					Content = actionSheetContent
				};

				actionSheetContent.OptionSelected += (s, e) =>
				{
					userDidSelect = true;
					actionSheet.Hide();
				};

				actionSheet.Closed += (s, e) =>
				{
					if (!userDidSelect)
						arguments.SetResult(null);
				};

				try
				{
					var current = sender.ToPlatform();
					var pageParent = current?.Parent as FrameworkElement;

					if (pageParent != null)
						actionSheet.ShowAt(pageParent);
					else
					{
						if (current != null && current is FrameworkElement mainPage)
							actionSheet.ShowAt(current);
						else
							arguments.SetResult(null);
					}
				}
				catch (ArgumentException) // If the page is not in the visual tree
				{
					if (UI.Xaml.Window.Current != null && UI.Xaml.Window.Current.Content is FrameworkElement mainPage)
						actionSheet.ShowAt(mainPage);
					else
						arguments.SetResult(null);
				}
			}

			static async Task<bool> ShowAlert(ContentDialog alert)
			{
				ContentDialogResult result = await alert.ShowAsync();

				return result == ContentDialogResult.Primary;
			}

			static async Task<string?> ShowPrompt(PromptDialog prompt)
			{
				ContentDialogResult result = await prompt.ShowAsync();

				if (result == ContentDialogResult.Primary)
					return prompt.Input;

				return null;
			}

			bool PageIsInThisWindow(Page page) =>
				page?.Window == VirtualView;
		}
	}
}
