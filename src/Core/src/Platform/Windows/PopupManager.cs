#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	internal static class PopupManager
	{
		static readonly List<PopupRequestHelper> Subscriptions = new List<PopupRequestHelper>();

		internal static void Subscribe(Application application, MauiContext mauiContext)
		{
			if (Subscriptions.Any(s => s.Application == application))
			{
				return;
			}

			Subscriptions.Add(new PopupRequestHelper(application, mauiContext));
		}

		internal static void Unsubscribe(Application application)
		{
			var toRemove = Subscriptions.Where(s => s.Application == application).ToList();

			foreach (PopupRequestHelper popupRequestHelper in toRemove)
			{
				popupRequestHelper.Dispose();
				Subscriptions.Remove(popupRequestHelper);
			}
		}

		internal sealed class PopupRequestHelper : IDisposable
		{
			static Task<bool> CurrentAlert;

			internal PopupRequestHelper(Application application, MauiContext mauiContext)
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

			}

			async void OnAlertRequested(IPage sender, AlertArguments arguments)
			{
				string content = arguments.Message ?? string.Empty;
				string title = arguments.Title ?? string.Empty;

				var alertDialog = new ContentDialog
				{
					Content = content,
					Title = title
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

			void OnPromptRequested(IPage sender, PromptArguments arguments)
			{


			}

			void OnActionSheetRequested(IPage sender, ActionSheetArguments arguments)
			{

			}

			static async Task<bool> ShowAlert(ContentDialog alert)
			{
				ContentDialogResult result = await alert.ShowAsync();

				return result == ContentDialogResult.Primary;
			}
		}
	}
}