using System;
using System.Collections.Specialized;
using Android.App;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Text.Style;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;
using AResource = Android.Resource;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiPicker>
	{
		AppCompatAlertDialog? _dialog;

		protected override MauiPicker CreatePlatformView() =>
			new MauiPicker(Context);

		protected override void ConnectHandler(MauiPicker platformView)
		{
			platformView.Click += OnClick;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiPicker platformView)
		{
			platformView.Click -= OnClick;

			if (_dialog != null)
			{
				_dialog.ShowEvent -= OnDialogShown;
				_dialog.DismissEvent -= OnDialogDismiss;
				_dialog.Hide();
				_dialog.Dispose();
				_dialog = null;
			}

			base.DisconnectHandler(platformView);
		}

		// This is a Android-specific mapping
		public static void MapBackground(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateBackground(picker);
		}

		// TODO Uncomment me on NET8 [Obsolete]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		internal static void MapItems(IPickerHandler handler, IPicker picker) => Reload(handler);

		public static void MapTitle(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitle(picker);
		}

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitleColor(picker);
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateSelectedIndex(picker);
		}

		public static void MapCharacterSpacing(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(picker);
		}

		public static void MapFont(IPickerHandler handler, IPicker picker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(picker, fontManager);
		}

		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateHorizontalAlignment(picker.HorizontalTextAlignment);
		}

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView.UpdateTextColor(picker);
		}

		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateVerticalAlignment(picker.VerticalTextAlignment);
		}

		internal static void MapFocus(IPickerHandler handler, IPicker picker, object? args)
		{
			if (handler.IsConnected())
			{
				ViewHandler.MapFocus(handler, picker, args);
				handler.PlatformView.CallOnClick();
			}
		}

		internal static void MapUnfocus(IPickerHandler handler, IPicker picker, object? args)
		{
			if (handler.IsConnected() && handler is PickerHandler pickerHandler)
			{
				pickerHandler.DismissDialog();
				ViewHandler.MapUnfocus(handler, picker, args);
			}
		}

		void DismissDialog()
		{
			_dialog?.Dismiss();
		}

		void OnClick(object? sender, EventArgs e)
		{
			if (_dialog == null && VirtualView != null)
			{
				using (var builder = new AppCompatAlertDialog.Builder(Context))
				{
					if (VirtualView.TitleColor == null)
					{
						builder.SetTitle(VirtualView.Title ?? string.Empty);
					}
					else
					{
						var title = new SpannableString(VirtualView.Title ?? string.Empty);
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
						title.SetSpan(new ForegroundColorSpan(VirtualView.TitleColor.ToPlatform()), 0, title.Length(), SpanTypes.ExclusiveExclusive);
#pragma warning restore CA1416
						builder.SetTitle(title);
					}

					string[] items = VirtualView.GetItemsAsArray();

					for (var i = 0; i < items.Length; i++)
					{
						var item = items[i];
						if (item == null)
							items[i] = String.Empty;
					}

					builder.SetItems(items, (s, e) =>
					{
						var selectedIndex = e.Which;
						VirtualView.SelectedIndex = selectedIndex;
						base.PlatformView?.UpdatePicker(VirtualView);
					});

					builder.SetNegativeButton(AResource.String.Cancel, (o, args) => { });

					_dialog = builder.Create();
				}

				if (_dialog == null)
					return;

				_dialog.UpdateFlowDirection(PlatformView);

				_dialog.SetCanceledOnTouchOutside(true);

				_dialog.ShowEvent += OnDialogShown;

				_dialog.DismissEvent += OnDialogDismiss;

				_dialog.Show();
			}
		}

		void OnDialogDismiss(object? sender, EventArgs e)
		{
			if (_dialog is null)
			{
				return;
			}

			_dialog.DismissEvent -= OnDialogDismiss;
			VirtualView.IsFocused = false;
			_dialog = null;
		}

		void OnDialogShown(object? sender, EventArgs e)
		{
			if (_dialog is null)
			{
				return;
			}

			_dialog.ShowEvent -= OnDialogShown;
			VirtualView.IsFocused = true;
		}

		static void Reload(IPickerHandler handler)
		{
			handler.PlatformView.UpdatePicker(handler.VirtualView);
		}
	}
}