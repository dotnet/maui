#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
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
			var nativeWindow = window.MauiContext.GetNativeWindow();

			if (Subscriptions.Any(s => s.Window == nativeWindow))
				return;

			Subscriptions.Add(new AlertRequestHelper(nativeWindow, window.MauiContext));
		}

		internal void Unsubscribe(Window window)
		{
			var nativeWindow = window.MauiContext.GetNativeWindow();

			var toRemove = Subscriptions.Where(s => s.Window == nativeWindow).ToList();

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

				WeakReferenceMessenger.Default.Register<AlertRequestHelper, PageBusyMessage>(this, static (r,m) => r.OnPageBusy(m));
				WeakReferenceMessenger.Default.Register<AlertRequestHelper, PageAlertMessage>(this, static (r, m) => r.OnAlertRequested(m));
				WeakReferenceMessenger.Default.Register<AlertRequestHelper, PromptMessage>(this, static (r, m) => r.OnPromptRequested(m));
				WeakReferenceMessenger.Default.Register<AlertRequestHelper, ActionSheetMessage>(this, static (r, m) => r.OnActionSheetRequested(m));
			}

			public UI.Xaml.Window Window { get; }
			public IMauiContext MauiContext { get; }

			public void Dispose()
			{
				WeakReferenceMessenger.Default.UnregisterAll(this);
			}

			void OnPageBusy(PageBusyMessage message)
			{
				var sender = message.Page;
				var busy = message.IsBusy;

				// TODO: Wrap the pages in a Canvas, and dynamically add a ProgressBar
			}

			async void OnAlertRequested(PageAlertMessage message)
			{
				var arguments = message.Arguments;

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

				// TODO: Check EffectiveFlowDirection

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

			async void OnPromptRequested(PromptMessage message)
			{
				var arguments = message.Arguments;

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
				promptDialog.XamlRoot = Window.Content.XamlRoot;

				CurrentPrompt = ShowPrompt(promptDialog);
				arguments.SetResult(await CurrentPrompt.ConfigureAwait(false));
				CurrentPrompt = null;
			}

			void OnActionSheetRequested(ActionSheetMessage message)
			{
				var sender = message.Page;
				var arguments = message.Arguments;

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
					if (UI.Xaml.Window.Current.Content is FrameworkElement mainPage)
						actionSheet.ShowAt(mainPage);
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