#nullable enable
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
		readonly List<AlertRequestHelper> Subscriptions = new List<AlertRequestHelper>();

		internal void Subscribe(Window window)
		{
			var platformWindow = window.MauiContext.GetPlatformWindow();

			if (Subscriptions.Any(s => s.Window == platformWindow))
				return;

			Subscriptions.Add(new AlertRequestHelper(platformWindow, window.MauiContext));
		}

		internal void Unsubscribe(Window window)
		{
			var platformWindow = window.MauiContext.GetPlatformWindow();

			var toRemove = Subscriptions.Where(s => s.Window == platformWindow).ToList();

			foreach (AlertRequestHelper alertRequestHelper in toRemove)
			{
				alertRequestHelper.Dispose();
				Subscriptions.Remove(alertRequestHelper);
			}
		}

		internal sealed class AlertRequestHelper : IDisposable
		{
			static Task<bool>? CurrentAlert;
			static Task<string?>? CurrentPrompt;

			internal AlertRequestHelper(UI.Xaml.Window window, IMauiContext mauiContext)
			{
				Window = window;
				MauiContext = mauiContext;

#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
				MessagingCenter.Subscribe<Page, bool>(Window, Page.BusySetSignalName, OnPageBusy);
				MessagingCenter.Subscribe<Page, AlertArguments>(Window, Page.AlertSignalName, OnAlertRequested);
				MessagingCenter.Subscribe<Page, PromptArguments>(Window, Page.PromptSignalName, OnPromptRequested);
				MessagingCenter.Subscribe<Page, ActionSheetArguments>(Window, Page.ActionSheetSignalName, OnActionSheetRequested);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			public UI.Xaml.Window Window { get; }
			public IMauiContext MauiContext { get; }

			public void Dispose()
			{
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
				MessagingCenter.Unsubscribe<Page, bool>(Window, Page.BusySetSignalName);
				MessagingCenter.Unsubscribe<Page, AlertArguments>(Window, Page.AlertSignalName);
				MessagingCenter.Unsubscribe<Page, PromptArguments>(Window, Page.PromptSignalName);
				MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(Window, Page.ActionSheetSignalName);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			void OnPageBusy(Page sender, bool enabled)
			{
				// TODO: Wrap the pages in a Canvas, and dynamically add a ProgressBar
			}

			async void OnAlertRequested(Page sender, AlertArguments arguments)
			{
				string content = arguments.Message ?? string.Empty;
				string title = arguments.Title ?? string.Empty;

				var alertDialog = new AlertDialog
				{
					Content = content,
					Title = title,
					VerticalScrollBarVisibility = UI.Xaml.Controls.ScrollBarVisibility.Auto
				};

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
				alertDialog.XamlRoot = Window.Content.XamlRoot;

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

			async void OnPromptRequested(Page sender, PromptArguments arguments)
			{
				var promptDialog = new PromptDialog
				{
					Title = arguments.Title ?? string.Empty,
					Message = arguments.Message ?? string.Empty,
					Input = arguments.InitialValue ?? string.Empty,
					Placeholder = arguments.Placeholder ?? string.Empty,
					MaxLength = arguments.MaxLength >= 0 ? arguments.MaxLength : 0,
					InputScope = arguments.Keyboard.ToInputScope()
				};

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
				promptDialog.XamlRoot = Window.Content.XamlRoot;

				CurrentPrompt = ShowPrompt(promptDialog);
				arguments.SetResult(await CurrentPrompt.ConfigureAwait(false));
				CurrentPrompt = null;
			}

			void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
			{
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
					var current = sender.ToPlatform(MauiContext);
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
		}
	}
}