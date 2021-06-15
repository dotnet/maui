﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using AppCompatActivity = AndroidX.AppCompat.App.AppCompatActivity;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;
using AWindow = Android.Views.Window;

namespace Microsoft.Maui
{
	internal static class PopupManager
	{
		static readonly List<PopupRequestHelper> Subscriptions = new List<PopupRequestHelper>();

		internal static void Subscribe(Activity context, MauiContext mauiContext)
		{
			if (Subscriptions.Any(s => s.Activity == context))
			{
				return;
			}

			Subscriptions.Add(new PopupRequestHelper(context, mauiContext));
		}

		internal static void Unsubscribe(Activity context)
		{
			var toRemove = Subscriptions.Where(s => s.Activity == context).ToList();

			foreach (PopupRequestHelper popupRequestHelper in toRemove)
			{
				popupRequestHelper.Dispose();
				Subscriptions.Remove(popupRequestHelper);
			}
		}

		internal static void ResetBusyCount(Activity context)
		{
			Subscriptions.FirstOrDefault(s => s.Activity == context)?.ResetBusyCount();
		}

		internal sealed class PopupRequestHelper : IDisposable
		{
			int _busyCount;
			bool? _supportsProgress;

			internal PopupRequestHelper(Activity context, MauiContext mauiContext)
			{
				Activity = context;
				MauiContext = mauiContext;

				MessagingCenter.Subscribe<IPage, bool>(Activity, AlertConstants.BusySetSignalName, OnPageBusy);
				MessagingCenter.Subscribe<IPage, AlertArguments>(Activity, AlertConstants.AlertSignalName, OnAlertRequested);
				MessagingCenter.Subscribe<IPage, PromptArguments>(Activity, AlertConstants.PromptSignalName, OnPromptRequested);
				MessagingCenter.Subscribe<IPage, ActionSheetArguments>(Activity, AlertConstants.ActionSheetSignalName, OnActionSheetRequested);
			}

			public Activity Activity { get; }
			public MauiContext MauiContext { get; }

			public void Dispose()
			{
				MessagingCenter.Unsubscribe<IPage, bool>(Activity, AlertConstants.BusySetSignalName);
				MessagingCenter.Unsubscribe<IPage, AlertArguments>(Activity, AlertConstants.AlertSignalName);
				MessagingCenter.Unsubscribe<IPage, PromptArguments>(Activity, AlertConstants.PromptSignalName);
				MessagingCenter.Unsubscribe<IPage, ActionSheetArguments>(Activity, AlertConstants.ActionSheetSignalName);
			}

			public void ResetBusyCount()
			{
				_busyCount = 0;
			}

			void OnPageBusy(IPage sender, bool enabled)
			{
				// Verify that the page making the request is part of this activity 
				if (!PageIsInThisContext(sender))
				{
					return;
				}

				_busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);

				UpdateProgressBarVisibility(_busyCount > 0);
			}

			void OnActionSheetRequested(IPage sender, ActionSheetArguments arguments)
			{
				// Verify that the page making the request is part of this activity 
				if (!PageIsInThisContext(sender))
				{
					return;
				}

				var builder = new DialogBuilder(Activity);

				builder.SetTitle(arguments.Title);
				string[] items = arguments.Buttons.ToArray();
				builder.SetItems(items, (o, args) => arguments.Result.TrySetResult(items[args.Which]));

				if (arguments.Cancel != null)
					builder.SetPositiveButton(arguments.Cancel, (o, args) => arguments.Result.TrySetResult(arguments.Cancel));

				if (arguments.Destruction != null)
					builder.SetNegativeButton(arguments.Destruction, (o, args) => arguments.Result.TrySetResult(arguments.Destruction));

				var dialog = builder.Create();
				builder.Dispose();

				if (dialog == null)
					return;

				// To match current functionality of handler we set cancelable on outside
				// and return null
				if (dialog.Window != null)
				{
					if (arguments.FlowDirection == FlowDirection.MatchParent && sender is IView view)
					{
						// TODO: Update FlowDirection			
					}
					else if (arguments.FlowDirection == FlowDirection.LeftToRight)
						dialog.Window.DecorView.LayoutDirection = LayoutDirection.Ltr;
					else if (arguments.FlowDirection == FlowDirection.RightToLeft)
						dialog.Window.DecorView.LayoutDirection = LayoutDirection.Rtl;
				}

				dialog.SetCanceledOnTouchOutside(true);
				dialog.SetCancelEvent((o, e) => arguments.SetResult(null));
				dialog.Show();

				var listView = dialog.GetListView();

				if (listView != null)
				{
					listView.TextDirection = GetTextDirection(sender, arguments.FlowDirection);
					LayoutDirection layoutDirection = GetLayoutDirection(sender, arguments.FlowDirection);

					if (dialog.GetButton((int)DialogButtonType.Negative)?.Parent is View parentView)
					{
						if (arguments.Cancel != null)
							parentView.LayoutDirection = layoutDirection;
						if (arguments.Destruction != null)
							parentView.LayoutDirection = layoutDirection;
					}
				}
			}

			void OnAlertRequested(IPage sender, AlertArguments arguments)
			{
				// Verify that the page making the request is part of this activity 
				if (!PageIsInThisContext(sender))
				{
					return;
				}
		
				int messageID = 16908299;
				var alert = new DialogBuilder(Activity).Create();

				if (alert == null)
					return;

				if (alert.Window != null)
				{
					if (arguments.FlowDirection == FlowDirection.MatchParent && sender is IView view)
					{
						// TODO: Update FlowDirection
					}
					else if (arguments.FlowDirection == FlowDirection.LeftToRight)
						alert.Window.DecorView.LayoutDirection = LayoutDirection.Ltr;
					else if (arguments.FlowDirection == FlowDirection.RightToLeft)
						alert.Window.DecorView.LayoutDirection = LayoutDirection.Rtl;
				}

				alert.SetTitle(arguments.Title);
				alert.SetMessage(arguments.Message);
				if (arguments.Accept != null)
					alert.SetButton((int)DialogButtonType.Positive, arguments.Accept, (o, args) => arguments.SetResult(true));
				alert.SetButton((int)DialogButtonType.Negative, arguments.Cancel, (o, args) => arguments.SetResult(false));
				alert.SetCancelEvent((o, args) => { arguments.SetResult(false); });
				alert.Show();

				TextView textView = (TextView)alert.findViewByID(messageID);
				textView.TextDirection = GetTextDirection(sender, arguments.FlowDirection);


				if (alert.GetButton((int)DialogButtonType.Negative).Parent is View parentView)
					parentView.LayoutDirection = GetLayoutDirection(sender, arguments.FlowDirection);
			}

			LayoutDirection GetLayoutDirection(IPage sender, FlowDirection flowDirection)
			{
				if (flowDirection == FlowDirection.LeftToRight)
					return LayoutDirection.Ltr;
				else if (flowDirection == FlowDirection.RightToLeft)
					return LayoutDirection.Rtl;

				// TODO: Check EffectiveFlowDirection

				return LayoutDirection.Ltr;
			}

			TextDirection GetTextDirection(IPage sender, FlowDirection flowDirection)
			{
				if (flowDirection == FlowDirection.LeftToRight)
					return TextDirection.Ltr;
				else if (flowDirection == FlowDirection.RightToLeft)
					return TextDirection.Rtl;

				// TODO: Check EffectiveFlowDirection

				return TextDirection.Ltr;
			}

			void OnPromptRequested(IPage sender, PromptArguments arguments)
			{
				// Verify that the page making the request is part of this activity 
				if (!PageIsInThisContext(sender))
				{
					return;
				}

				var alertDialog = new DialogBuilder(Activity).Create();

				if (alertDialog == null)
					return;

				alertDialog.SetTitle(arguments.Title);
				alertDialog.SetMessage(arguments.Message);

				var frameLayout = new FrameLayout(Activity);
				var editText = new EditText(Activity) { Hint = arguments.Placeholder, Text = arguments.InitialValue };
				var layoutParams = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
				{
					LeftMargin = (int)(22 * Activity.Resources.DisplayMetrics.Density),
					RightMargin = (int)(22 * Activity.Resources.DisplayMetrics.Density)
				};

				editText.LayoutParameters = layoutParams;
				editText.InputType = arguments.Keyboard.ToInputType();
				if (arguments.Keyboard == Keyboard.Numeric)
					editText.KeyListener = LocalizedDigitsKeyListener.Create(editText.InputType);

				if (arguments.MaxLength > -1)
					editText.SetFilters(new IInputFilter[] { new InputFilterLengthFilter(arguments.MaxLength) });

				frameLayout.AddView(editText);
				alertDialog.SetView(frameLayout);

				alertDialog.SetButton((int)DialogButtonType.Positive, arguments.Accept, (o, args) => arguments.SetResult(editText.Text));
				alertDialog.SetButton((int)DialogButtonType.Negative, arguments.Cancel, (o, args) => arguments.SetResult(null));
				alertDialog.SetCancelEvent((o, args) => { arguments.SetResult(null); });

				alertDialog.Window.SetSoftInputMode(SoftInput.StateVisible);
				alertDialog.Show();
				editText.RequestFocus();
			}

			void UpdateProgressBarVisibility(bool isBusy)
			{
				if (!SupportsProgress)
					return;
#pragma warning disable 612, 618

				Activity.SetProgressBarIndeterminate(true);
				Activity.SetProgressBarIndeterminateVisibility(isBusy);
#pragma warning restore 612, 618
			}

			internal bool SupportsProgress
			{
				get
				{
					if (_supportsProgress.HasValue)
						return _supportsProgress.Value;
					
					int progressCircularId = Activity.Resources.GetIdentifier("progress_circular", "id", "android");
				
					if (progressCircularId > 0)
						_supportsProgress = Activity.FindViewById(progressCircularId) != null;
					else
						_supportsProgress = true;

					return _supportsProgress.Value;
				}
			}

			bool PageIsInThisContext(IPage page)
			{
				var nativeView = page.ToNative(MauiContext);

				if (nativeView.Context == null)
				{
					return false;
				}

				return nativeView.Context.Equals(Activity);
			}

			// This is a proxy dialog builder class to support both pre-appcompat and appcompat dialogs for Alert,
			// ActionSheet, Prompt, etc. 
			internal sealed class DialogBuilder
			{
				AppCompatAlertDialog.Builder _appcompatBuilder;
				AlertDialog.Builder _legacyBuilder;

				bool _useAppCompat;

				public DialogBuilder(Activity activity)
				{
					if (activity is AppCompatActivity)
					{
						_appcompatBuilder = new AppCompatAlertDialog.Builder(activity);
						_useAppCompat = true;
					}
					else
					{
						_legacyBuilder = new AlertDialog.Builder(activity);
					}
				}

				public void SetTitle(string title)
				{
					if (_useAppCompat)
					{
						_appcompatBuilder?.SetTitle(title);
					}
					else
					{
						_legacyBuilder?.SetTitle(title);
					}
				}

				public void SetItems(string[] items, EventHandler<DialogClickEventArgs> handler)
				{
					if (_useAppCompat)
					{
						_appcompatBuilder?.SetItems(items, handler);
					}
					else
					{
						_legacyBuilder?.SetItems(items, handler);
					}
				}

				public void SetPositiveButton(string text, EventHandler<DialogClickEventArgs> handler)
				{
					if (_useAppCompat)
					{
						_appcompatBuilder?.SetPositiveButton(text, handler);
					}
					else
					{
						_legacyBuilder?.SetPositiveButton(text, handler);
					}
				}

				public void SetNegativeButton(string text, EventHandler<DialogClickEventArgs> handler)
				{
					if (_useAppCompat)
					{
						_appcompatBuilder?.SetNegativeButton(text, handler);
					}
					else
					{
						_legacyBuilder?.SetNegativeButton(text, handler);
					}
				}

				public FlexibleAlertDialog Create()
				{
					if (_useAppCompat && _appcompatBuilder != null)
					{
						return new FlexibleAlertDialog(_appcompatBuilder.Create());
					}

					if (_legacyBuilder != null)
						return new FlexibleAlertDialog(_legacyBuilder.Create());

					return null;
				}

				public void Dispose()
				{
					if (_useAppCompat)
					{
						_appcompatBuilder?.Dispose();
					}
					else
					{
						_legacyBuilder?.Dispose();
					}
				}
			}

			internal sealed class FlexibleAlertDialog
			{
				readonly AppCompatAlertDialog _appcompatAlertDialog;
				readonly AlertDialog _legacyAlertDialog;
				bool _useAppCompat;

				public FlexibleAlertDialog(AlertDialog alertDialog)
				{
					_legacyAlertDialog = alertDialog;
				}

				public FlexibleAlertDialog(AppCompatAlertDialog alertDialog)
				{
					_appcompatAlertDialog = alertDialog;
					_useAppCompat = true;
				}

				public void SetTitle(string title)
				{
					if (_useAppCompat)
					{
						_appcompatAlertDialog.SetTitle(title);
					}
					else
					{
						_legacyAlertDialog.SetTitle(title);
					}
				}

				public void SetMessage(string message)
				{
					if (_useAppCompat)
					{
						_appcompatAlertDialog.SetMessage(message);
					}
					else
					{
						_legacyAlertDialog.SetMessage(message);
					}
				}

				public void SetButton(int whichButton, string text, EventHandler<DialogClickEventArgs> handler)
				{
					if (_useAppCompat)
					{
						_appcompatAlertDialog.SetButton(whichButton, text, handler);
					}
					else
					{
						_legacyAlertDialog.SetButton(whichButton, text, handler);
					}
				}

				public Button GetButton(int whichButton)
				{
					if (_useAppCompat)
					{
						return _appcompatAlertDialog.GetButton(whichButton);
					}
					else
					{
						return _legacyAlertDialog.GetButton(whichButton);
					}
				}

				public View GetListView()
				{
					if (_useAppCompat)
					{
						return _appcompatAlertDialog.ListView;
					}
					else
					{
						return _legacyAlertDialog.ListView;
					}
				}

				public void SetCancelEvent(EventHandler cancel)
				{
					if (_useAppCompat)
					{
						_appcompatAlertDialog.CancelEvent += cancel;
					}
					else
					{
						_legacyAlertDialog.CancelEvent += cancel;
					}
				}

				public void SetCanceledOnTouchOutside(bool canceledOnTouchOutSide)
				{
					if (_useAppCompat)
					{
						_appcompatAlertDialog.SetCanceledOnTouchOutside(canceledOnTouchOutSide);
					}
					else
					{
						_legacyAlertDialog.SetCanceledOnTouchOutside(canceledOnTouchOutSide);
					}
				}

				public void SetView(View view)
				{
					if (_useAppCompat)
					{
						_appcompatAlertDialog.SetView(view);
					}
					else
					{
						_legacyAlertDialog.SetView(view);
					}
				}

				public View findViewByID(int id)
				{
					if (_useAppCompat)
					{
						return _appcompatAlertDialog.FindViewById(id);
					}
					else
					{
						return _legacyAlertDialog.FindViewById(id);
					}
				}

				public AWindow Window => _useAppCompat ? _appcompatAlertDialog?.Window : _legacyAlertDialog?.Window;

				public void Show()
				{
					if (_useAppCompat)
					{
						_appcompatAlertDialog.Show();
					}
					else
					{
						_legacyAlertDialog.Show();
					}
				}
			}
		}
	}
}