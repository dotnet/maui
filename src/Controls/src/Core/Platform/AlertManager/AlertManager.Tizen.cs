using System;
using System.Collections.Generic;
using System.Linq;
using ElmSharp;
using Microsoft.Maui.Controls.Internals;
using Tizen.UIExtensions.ElmSharp;
using EBox = ElmSharp.Box;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;
using EProgressBar = ElmSharp.ProgressBar;
using EWindow = ElmSharp.Window;
using GColor = Microsoft.Maui.Graphics.Color;
using TButton = Tizen.UIExtensions.ElmSharp.Button;
using TColor = Tizen.UIExtensions.Common.Color;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class AlertManager
	{
		readonly List<AlertRequestHelper> Subscriptions = new List<AlertRequestHelper>();

		internal void Subscribe(Window window)
		{
			IMauiContext mauiContext = window?.MauiContext;
			EWindow nativeWindow = mauiContext.GetNativeWindow();

			if (mauiContext == null || nativeWindow == null)
				return;

			if (Subscriptions.Any(s => s.Window == nativeWindow))
			{
				return;
			}

			Subscriptions.Add(new AlertRequestHelper(nativeWindow, mauiContext));
		}

		internal void Unsubscribe(Window window)
		{
			IMauiContext mauiContext = window?.MauiContext;
			EWindow nativeWindow = mauiContext.GetNativeWindow();

			var toRemove = Subscriptions.Where(s => s.Window == nativeWindow).ToList();

			foreach (AlertRequestHelper alertRequestHelper in toRemove)
			{
				alertRequestHelper.Dispose();
				Subscriptions.Remove(alertRequestHelper);
			}
		}
	}

	internal sealed class AlertRequestHelper : IDisposable
	{
		int _busyCount;
		Dialog _pageBusyDialog;
		readonly HashSet<EvasObject> _alerts = new HashSet<EvasObject>();

		internal AlertRequestHelper(EWindow window, IMauiContext mauiContext)
		{
			Window = window;
			MauiContext = mauiContext;

			MessagingCenter.Subscribe<Page, bool>(Window, Page.BusySetSignalName, OnBusySetRequest);
			MessagingCenter.Subscribe<Page, AlertArguments>(Window, Page.AlertSignalName, OnAlertRequest);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(Window, Page.ActionSheetSignalName, OnActionSheetRequest);
			MessagingCenter.Subscribe<Page, PromptArguments>(Window, Page.PromptSignalName, OnPromptRequested);
		}

		public EWindow Window { get; }
		public IMauiContext MauiContext { get; }

		public void Dispose()
		{
			MessagingCenter.Unsubscribe<Page, AlertArguments>(Window, Page.AlertSignalName);
			MessagingCenter.Unsubscribe<Page, bool>(Window, Page.BusySetSignalName);
			MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(Window, Page.ActionSheetSignalName);
			MessagingCenter.Unsubscribe<Page, PromptArguments>(Window, Page.PromptSignalName);
		}

		void OnBusySetRequest(Page sender, bool enabled)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsInThisContext(sender))
			{
				return;
			}
			_busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);

			if (null == _pageBusyDialog)
			{
				_pageBusyDialog = new Dialog(MauiContext.GetNativeParent())
				{
					Orientation = PopupOrientation.Center,
					BackgroundColor = EColor.Transparent
				};

				_pageBusyDialog.SetTitleBackgroundColor(EColor.Transparent);
				_pageBusyDialog.SetContentBackgroundColor(EColor.Transparent);

				var activity = new EProgressBar(_pageBusyDialog) { IsPulseMode = true }.SetLargeStyle();
				activity.PlayPulse();
				activity.Show();

				_pageBusyDialog.Content = activity;
			}

			if (_busyCount > 0)
			{
				_pageBusyDialog.Show();
			}
			else
			{
				_pageBusyDialog.Dismiss();
				_pageBusyDialog.Unrealize();
				_pageBusyDialog = null;
			}
		}

		void OnAlertRequest(Page sender, AlertArguments arguments)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsInThisContext(sender))
				return;

			var alert = Dialog.CreateDialog(MauiContext.GetNativeParent(), (arguments.Accept != null));

			alert.Title = arguments.Title;
			var message = arguments.Message?.Replace("&", "&amp;", StringComparison.Ordinal).Replace("<", "&lt;", StringComparison.Ordinal).Replace(">", "&gt;", StringComparison.Ordinal).Replace(Environment.NewLine, "<br>", StringComparison.Ordinal);
			alert.Message = message;

			var cancel = new EButton(alert) { Text = arguments.Cancel };
			alert.NegativeButton = cancel;
			cancel.Clicked += (s, evt) =>
			{
				arguments.SetResult(false);
				alert.Dismiss();
			};

			if (arguments.Accept != null)
			{
				var ok = new EButton(alert) { Text = arguments.Accept };
				alert.NeutralButton = ok;
				ok.Clicked += (s, evt) =>
				{
					arguments.SetResult(true);
					alert.Dismiss();
				};
			}

			alert.BackButtonPressed += (s, evt) =>
			{
				arguments.SetResult(false);
				alert.Dismiss();
			};

			alert.Show();
			_alerts.Add(alert);
			alert.Dismissed += (s, e) => _alerts.Remove(alert);
		}

		void OnActionSheetRequest(Page sender, ActionSheetArguments arguments)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsInThisContext(sender))
				return;

			var alert = Dialog.CreateDialog(MauiContext.GetNativeParent());

			alert.Title = arguments.Title;
			var box = new EBox(alert);

			if (null != arguments.Destruction)
			{
				var destruction = new TButton(alert)
				{
					Text = arguments.Destruction,
					AlignmentX = -1
				};
				//TextColor should be set after applying style
				destruction.TextColor = TColor.Red;

				destruction.Clicked += (s, evt) =>
				{
					arguments.SetResult(arguments.Destruction);
					alert.Dismiss();
				};
				destruction.Show();
				box.PackEnd(destruction);
			}

			foreach (string buttonName in arguments.Buttons)
			{
				var button = new TButton(alert)
				{
					Text = buttonName,
					AlignmentX = -1
				};

				button.Clicked += (s, evt) =>
				{
					arguments.SetResult(buttonName);
					alert.Dismiss();
				};
				button.Show();
				box.PackEnd(button);
			}

			box.Show();
			alert.Content = box;

			if (null != arguments.Cancel)
			{
				var cancel = new TButton(alert) { Text = arguments.Cancel };
				alert.NegativeButton = cancel;
				cancel.Clicked += (s, evt) =>
				{
					alert.Dismiss();
				};
			}

			alert.BackButtonPressed += (s, evt) =>
			{
				alert.Dismiss();
			};

			alert.Show();

			_alerts.Add(alert);
			alert.Dismissed += (s, e) => _alerts.Remove(alert);
		}

		void OnPromptRequested(Page sender, PromptArguments args)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsInThisContext(sender))
				return;

			var prompt = Dialog.CreateDialog(MauiContext.GetNativeParent(), (args.Accept != null));
			prompt.Title = args.Title;

			var entry = new Entry
			{
				MinimumWidthRequest = 200,
				HorizontalOptions = LayoutOptions.Fill,
				BackgroundColor = GColor.FromRgb(250, 250, 250),
				TextColor = GColor.FromRgb(0, 0, 0),
				Keyboard = args.Keyboard,
			};

			if (!string.IsNullOrEmpty(args.Placeholder))
			{
				entry.Placeholder = args.Placeholder;
			}

			if (args.MaxLength > 0)
			{
				entry.MaxLength = args.MaxLength;
			}

			var layout = new VerticalStackLayout
			{
				Spacing = 10,
			};
			layout.Add(new Label
			{
				LineBreakMode = LineBreakMode.CharacterWrap,
				TextColor = Application.AccentColor,
				Text = args.Message,
				HorizontalOptions = LayoutOptions.Fill,
				HorizontalTextAlignment = TextAlignment.Center,
#pragma warning disable CS0612 // Type or member is obsolete
				FontSize = Device.GetNamedSize(NamedSize.Subtitle, typeof(Label)),
#pragma warning disable CS0612 // Type or member is obsolete
			});
			layout.Add(entry);
			layout.Parent = sender;
			var nativeView = layout.ToPlatform(MauiContext);

			var width = sender.Width <= -1 ? double.PositiveInfinity : sender.Width;
			var height = sender.Height <= -1 ? double.PositiveInfinity : sender.Height;
			var request = layout.CrossPlatformMeasure(width, height);

			nativeView.MinimumHeight = request.Height.ToScaledPixel();
			nativeView.MinimumWidth = request.Width.ToScaledPixel();

			prompt.Content = nativeView;

			var cancel = new EButton(prompt) { Text = args.Cancel };
			prompt.NegativeButton = cancel;
			cancel.Clicked += (s, evt) =>
			{
				args.SetResult(null);
				prompt.Dismiss();
			};

			if (args.Accept != null)
			{
				var ok = new EButton(prompt) { Text = args.Accept };
				prompt.NeutralButton = ok;
				ok.Clicked += (s, evt) =>
				{
					args.SetResult(entry.Text);
					prompt.Dismiss();
				};
			}

			entry.Completed += (s, e) =>
			{
				args.SetResult(entry.Text);
				prompt.Dismiss();
			};

			prompt.BackButtonPressed += (s, evt) =>
			{
				prompt.Dismiss();
			};

			prompt.Show();

			_alerts.Add(prompt);
			prompt.Dismissed += (s, e) => _alerts.Remove(prompt);
		}

		bool PageIsInThisContext(IView sender)
		{
			var context = sender.Handler?.MauiContext ?? null;
			return context?.GetNativeWindow() == Window;
		}
	}
}
