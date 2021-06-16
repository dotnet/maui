#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	internal static class AlertManager
	{
		static readonly List<AlertRequestHelper> Subscriptions = new List<AlertRequestHelper>();

		internal static void Subscribe(Application application, MauiContext mauiContext)
		{
			if (Subscriptions.Any(s => s.Application == application))
			{
				return;
			}

			Subscriptions.Add(new AlertRequestHelper(application, mauiContext));
		}

		internal static void Unsubscribe(Application application)
		{
			var toRemove = Subscriptions.Where(s => s.Application == application).ToList();

			foreach (AlertRequestHelper alertRequestHelper in toRemove)
			{
				alertRequestHelper.Dispose();
				Subscriptions.Remove(alertRequestHelper);
			}
		}

		internal sealed class AlertRequestHelper : IDisposable
		{
			static Task<bool> CurrentAlert;
			static Task<string> CurrentPrompt;

			internal AlertRequestHelper(Application application, MauiContext mauiContext)
			{
				Application = application;
				MauiContext = mauiContext;

				MessagingCenter.Subscribe<IPage, bool>(Application, AlertConstants.BusySetSignalName, OnPageBusy);
				MessagingCenter.Subscribe<IPage, AlertArguments>(Application, AlertConstants.AlertSignalName, OnAlertRequested);
				MessagingCenter.Subscribe<IPage, PromptArguments>(Application, AlertConstants.PromptSignalName, OnPromptRequested);
				MessagingCenter.Subscribe<IPage, ActionSheetArguments>(Application, AlertConstants.ActionSheetSignalName, OnActionSheetRequested);
			}

			public Application Application { get; }
			public MauiContext MauiContext { get; }

			public void Dispose()
			{
				MessagingCenter.Unsubscribe<IPage, bool>(Application, AlertConstants.BusySetSignalName);
				MessagingCenter.Unsubscribe<IPage, AlertArguments>(Application, AlertConstants.AlertSignalName);
				MessagingCenter.Unsubscribe<IPage, PromptArguments>(Application, AlertConstants.PromptSignalName);
				MessagingCenter.Unsubscribe<IPage, ActionSheetArguments>(Application, AlertConstants.ActionSheetSignalName);
			}

			void OnPageBusy(IPage sender, bool enabled)
			{
				// TODO: Wrap the pages in a Canvas, and dynamically add a ProgressBar
			}

			async void OnAlertRequested(IPage sender, AlertArguments arguments)
			{
				string content = arguments.Message ?? string.Empty;
				string title = arguments.Title ?? string.Empty;

				var alertDialog = new AlertDialog
				{
					Content = content,
					Title = title,
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto
				};

				if (arguments.FlowDirection == FlowDirection.RightToLeft)
				{
					alertDialog.FlowDirection = UI.Xaml.FlowDirection.RightToLeft;
				}
				else if (arguments.FlowDirection == FlowDirection.LeftToRight)
				{
					alertDialog.FlowDirection = UI.Xaml.FlowDirection.LeftToRight;
				}

				// TODO: Check EffectiveFlowDirection

				if (arguments.Cancel != null)
					alertDialog.SecondaryButtonText = arguments.Cancel;

				if (arguments.Accept != null)
					alertDialog.PrimaryButtonText = arguments.Accept;

				// This is a temporary workaround
				var nativePage = sender.ToNative(MauiContext);
				alertDialog.XamlRoot = nativePage.XamlRoot;

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

			async void OnPromptRequested(IPage sender, PromptArguments arguments)
			{
				var promptDialog = new PromptDialog
				{
					Title = arguments.Title ?? string.Empty,
					Message = arguments.Message ?? string.Empty,
					Input = arguments.InitialValue ?? string.Empty,
					Placeholder = arguments.Placeholder ?? string.Empty,
					MaxLength = arguments.MaxLength >= 0 ? arguments.MaxLength : 0,
					// TODO: Implement InputScope property after port the keyboardExtensions
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
				var nativePage = sender.ToNative(MauiContext);
				promptDialog.XamlRoot = nativePage.XamlRoot;

				CurrentPrompt = ShowPrompt(promptDialog);
				arguments.SetResult(await CurrentPrompt.ConfigureAwait(false));
				CurrentPrompt = null;
			}

			void OnActionSheetRequested(IPage sender, ActionSheetArguments arguments)
			{
				bool userDidSelect = false;

				if (arguments.FlowDirection == FlowDirection.MatchParent)
				{
					// TODO: Check EffectiveFlowDirection
				}

				var actionSheetContent = new ActionSheetContent(arguments);

				var actionSheet = new Flyout
				{
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
					var pageParent = sender.ToNative(MauiContext).Parent as FrameworkElement;

					if (pageParent != null)
						actionSheet.ShowAt(pageParent);
				}
				catch (ArgumentException) // If the page is not in the visual tree
				{
					if (Window.Current.Content is FrameworkElement mainPage)
						actionSheet.ShowAt(mainPage);
				}
			}

			static async Task<bool> ShowAlert(ContentDialog alert)
			{
				ContentDialogResult result = await alert.ShowAsync();

				return result == ContentDialogResult.Primary;
			}

			static async Task<string> ShowPrompt(PromptDialog prompt)
			{
				ContentDialogResult result = await prompt.ShowAsync();

				if (result == ContentDialogResult.Primary)
					return prompt.Input;

				return null;
			}
		}
	}
}