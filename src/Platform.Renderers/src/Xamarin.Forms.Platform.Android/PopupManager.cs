using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Internals;
using AppCompatActivity = AndroidX.AppCompat.App.AppCompatActivity;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Xamarin.Forms.Platform.Android
{
	internal static class PopupManager
	{
		static readonly List<PopupRequestHelper> s_subscriptions = new List<PopupRequestHelper>();

		internal static void Subscribe(Activity context)
		{
			if (s_subscriptions.Any(s => s.Activity == context))
			{
				return;
			}

			s_subscriptions.Add(new PopupRequestHelper(context));
		}

		internal static void Unsubscribe(Context context)
		{
			var toRemove = s_subscriptions.Where(s => s.Activity == context).ToList();
			foreach (PopupRequestHelper popupRequestHelper in toRemove)
			{
				popupRequestHelper.Dispose();
				s_subscriptions.Remove(popupRequestHelper);
			}
		}

		internal static void ResetBusyCount(Activity context)
		{
			s_subscriptions.FirstOrDefault(s => s.Activity == context)?.ResetBusyCount();
		}

		internal sealed class PopupRequestHelper : IDisposable
		{
			int _busyCount;
			bool? _supportsProgress;

			internal PopupRequestHelper(Activity context)
			{
				Activity = context;
				MessagingCenter.Subscribe<Page, bool>(Activity, Page.BusySetSignalName, OnPageBusy);
				MessagingCenter.Subscribe<Page, AlertArguments>(Activity, Page.AlertSignalName, OnAlertRequested);
				MessagingCenter.Subscribe<Page, PromptArguments>(Activity, Page.PromptSignalName, OnPromptRequested);
				MessagingCenter.Subscribe<Page, ActionSheetArguments>(Activity, Page.ActionSheetSignalName, OnActionSheetRequested);
			}

			public Activity Activity { get; }

			public void Dispose()
			{
				MessagingCenter.Unsubscribe<Page, bool>(Activity, Page.BusySetSignalName);
				MessagingCenter.Unsubscribe<Page, AlertArguments>(Activity, Page.AlertSignalName);
				MessagingCenter.Unsubscribe<Page, PromptArguments>(Activity, Page.PromptSignalName);
				MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(Activity, Page.ActionSheetSignalName);
			}

			public void ResetBusyCount()
			{
				_busyCount = 0;
			}

			void OnPageBusy(Page sender, bool enabled)
			{
				// Verify that the page making the request is part of this activity 
				if (!PageIsInThisContext(sender))
				{
					return;
				}

				_busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);

				UpdateProgressBarVisibility(_busyCount > 0);
			}

			void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
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
				//to match current functionality of renderer we set cancelable on outside
				//and return null
				dialog.SetCanceledOnTouchOutside(true);
				dialog.SetCancelEvent((o, e) => arguments.SetResult(null));
				dialog.Show();
			}

			void OnAlertRequested(Page sender, AlertArguments arguments)
			{
				// Verify that the page making the request is part of this activity 
				if (!PageIsInThisContext(sender))
				{
					return;
				}

				var alert = new DialogBuilder(Activity).Create();
				alert.SetTitle(arguments.Title);
				alert.SetMessage(arguments.Message);
				if (arguments.Accept != null)
					alert.SetButton((int)DialogButtonType.Positive, arguments.Accept, (o, args) => arguments.SetResult(true));
				alert.SetButton((int)DialogButtonType.Negative, arguments.Cancel, (o, args) => arguments.SetResult(false));
				alert.SetCancelEvent((o, args) => { arguments.SetResult(false); });
				alert.Show();
			}

			void OnPromptRequested(Page sender, PromptArguments arguments)
			{
				// Verify that the page making the request is part of this activity 
				if (!PageIsInThisContext(sender))
				{
					return;
				}

				var alertDialog = new DialogBuilder(Activity).Create();
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
					{
						return _supportsProgress.Value;
					}

					int progressCircularId = Activity.Resources.GetIdentifier("progress_circular", "id", "android");
					if (progressCircularId > 0)
						_supportsProgress = Activity.FindViewById(progressCircularId) != null;
					else
						_supportsProgress = true;
					return _supportsProgress.Value;
				}
			}

			bool PageIsInThisContext(Page page)
			{
				var renderer = Platform.GetRenderer(page);

				if (renderer?.View?.Context == null)
				{
					return false;
				}

				return renderer.View.Context.Equals(Activity);
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
						_appcompatBuilder.SetTitle(title);
					}
					else
					{
						_legacyBuilder.SetTitle(title);
					}
				}

				public void SetItems(string[] items, EventHandler<DialogClickEventArgs> handler)
				{
					if (_useAppCompat)
					{
						_appcompatBuilder.SetItems(items, handler);
					}
					else
					{
						_legacyBuilder.SetItems(items, handler);
					}
				}

				public void SetPositiveButton(string text, EventHandler<DialogClickEventArgs> handler)
				{
					if (_useAppCompat)
					{
						_appcompatBuilder.SetPositiveButton(text, handler);
					}
					else
					{
						_legacyBuilder.SetPositiveButton(text, handler);
					}
				}

				public void SetNegativeButton(string text, EventHandler<DialogClickEventArgs> handler)
				{
					if (_useAppCompat)
					{
						_appcompatBuilder.SetNegativeButton(text, handler);
					}
					else
					{
						_legacyBuilder.SetNegativeButton(text, handler);
					}
				}

				public FlexibleAlertDialog Create()
				{
					if (_useAppCompat)
					{
						return new FlexibleAlertDialog(_appcompatBuilder.Create());
					}

					return new FlexibleAlertDialog(_legacyBuilder.Create());
				}

				public void Dispose()
				{
					if (_useAppCompat)
					{
						_appcompatBuilder.Dispose();
					}
					else
					{
						_legacyBuilder.Dispose();
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

				public void SetView(global::Android.Views.View view)
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

				public Window Window => _useAppCompat ? _appcompatAlertDialog.Window : _legacyAlertDialog.Window;

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