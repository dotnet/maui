using System;
using System.Collections.Generic;
using ElmSharp;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;
using EProgressBar = ElmSharp.ProgressBar;

namespace Xamarin.Forms.Platform.Tizen
{
	public class PopupManager : IDisposable
	{
		ITizenPlatform _platform;
		Native.Dialog _pageBusyDialog;
		int _pageBusyCount;
		readonly HashSet<EvasObject> _alerts = new HashSet<EvasObject>();

		public PopupManager(ITizenPlatform platform)
		{
			_platform = platform;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, OnBusySetRequest);
			MessagingCenter.Subscribe<Page, AlertArguments>(this, Page.AlertSignalName, OnAlertRequest);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName, OnActionSheetRequest);
			MessagingCenter.Subscribe<Page, PromptArguments>(this, Page.PromptSignalName, OnPromptRequested);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				MessagingCenter.Unsubscribe<Page, AlertArguments>(this, Page.AlertSignalName);
				MessagingCenter.Unsubscribe<Page, bool>(this, Page.BusySetSignalName);
				MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName);
				MessagingCenter.Unsubscribe<Page, PromptArguments>(this, Page.PromptSignalName);
			}
		}

		void OnBusySetRequest(Page sender, bool enabled)
		{
			// Verify that the page making the request is child of this platform
			if (!_platform.PageIsChildOfPlatform(sender))
				return;

			if (null == _pageBusyDialog)
			{
				_pageBusyDialog = new Native.Dialog(Forms.NativeParent)
				{
					Orientation = PopupOrientation.Center,
					BackgroundColor = EColor.Transparent
				};

				if (Device.Idiom == TargetIdiom.Phone)
				{
					_pageBusyDialog.SetPartColor("bg_title", EColor.Transparent);
					_pageBusyDialog.SetPartColor("bg_content", EColor.Transparent);
				}
				else if (Device.Idiom == TargetIdiom.Watch)
				{
					_pageBusyDialog.Style = "circle";
				}

				var activity = new EProgressBar(_pageBusyDialog)
				{
					Style = "process_large",
					IsPulseMode = true,
				};
				activity.PlayPulse();
				activity.Show();

				_pageBusyDialog.Content = activity;

			}
			_pageBusyCount = Math.Max(0, enabled ? _pageBusyCount + 1 : _pageBusyCount - 1);
			if (_pageBusyCount > 0)
			{
				_pageBusyDialog.Show();
			}
			else
			{
				_pageBusyDialog.Dismiss();
				_pageBusyDialog = null;
			}
		}

		void OnAlertRequest(Page sender, AlertArguments arguments)
		{
			// Verify that the page making the request is child of this platform
			if (!_platform.PageIsChildOfPlatform(sender))
				return;

			var alert = Native.Dialog.CreateDialog(Forms.NativeParent, (arguments.Accept != null));

			alert.Title = arguments.Title;
			var message = arguments.Message.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace(Environment.NewLine, "<br>");
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
			if (!_platform.PageIsChildOfPlatform(sender))
				return;

			var alert = Native.Dialog.CreateDialog(Forms.NativeParent);

			alert.Title = arguments.Title;
			var box = new Box(alert);

			if (null != arguments.Destruction)
			{
				var destruction = new Native.Button(alert)
				{
					Text = arguments.Destruction,
					Style = ButtonStyle.Text,
					TextColor = EColor.Red,
					AlignmentX = -1
				};
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
				var button = new Native.Button(alert)
				{
					Text = buttonName,
					Style = ButtonStyle.Text,
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
				var cancel = new EButton(Forms.NativeParent) { Text = arguments.Cancel };
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
			if (!_platform.PageIsChildOfPlatform(sender))
				return;

			var prompt = Native.Dialog.CreateDialog(Forms.NativeParent, (args.Accept != null));
			prompt.Title = args.Title;

			var entry = new Entry
			{
				MinimumWidthRequest = 200,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.FromRgb(250, 250, 250),
				TextColor = Color.Black,
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

			var layout = new StackLayout
			{
				Spacing = 10,
				Children =
				{
					new Label
					{
						LineBreakMode = LineBreakMode.CharacterWrap,
						TextColor = Device.Idiom == TargetIdiom.Watch ? Color.White : Color.Accent,
						Text = args.Message,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						HorizontalTextAlignment = TextAlignment.Center,
						FontSize = Device.GetNamedSize(NamedSize.Subtitle, typeof(Label)),
					},
					entry,
				}
			};

			layout.Parent = sender;
			var layoutrenderer = Platform.GetOrCreateRenderer(layout);

			var request = layout.Measure(Device.Idiom == TargetIdiom.Watch ? sender.Width * 0.7 : sender.Width, sender.Height);
			(layoutrenderer as LayoutRenderer).RegisterOnLayoutUpdated();
			layoutrenderer.NativeView.MinimumHeight = Forms.ConvertToScaledPixel(request.Request.Height);
			layoutrenderer.NativeView.MinimumWidth = Forms.ConvertToScaledPixel(request.Request.Width);

			prompt.Content = layoutrenderer.NativeView;

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

	}
}