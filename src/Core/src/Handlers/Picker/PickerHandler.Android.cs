using System;
using Android.App;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Text.Style;
using AResource = Android.Resource;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiPicker>
	{
		static Drawable? s_defaultBackground;
		static ColorStateList? s_defaultTitleColors { get; set; }
		static ColorStateList? s_defaultTextColors { get; set; }
		AlertDialog? _dialog;

		protected override MauiPicker CreatePlatformView() =>
			new MauiPicker(Context);

		protected override void ConnectHandler(MauiPicker platformView)
		{
			platformView.FocusChange += OnFocusChange;
			platformView.Click += OnClick;

			base.ConnectHandler(platformView);

			SetupDefaults(platformView);
		}

		protected override void DisconnectHandler(MauiPicker platformView)
		{
			platformView.FocusChange -= OnFocusChange;
			platformView.Click -= OnClick;

			base.DisconnectHandler(platformView);
		}

		void SetupDefaults(MauiPicker platformView)
		{
			s_defaultBackground = platformView.Background;
			s_defaultTitleColors = platformView.HintTextColors;
			s_defaultTextColors = platformView.TextColors;
		}

		// This is a Android-specific mapping
		public static void MapBackground(PickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateBackground(picker, s_defaultBackground);
		}

		public static void MapReload(PickerHandler handler, IPicker picker, object? args) => handler.Reload();

		public static void MapTitle(PickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitle(picker);
		}

		public static void MapTitleColor(PickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitleColor(picker, s_defaultTitleColors);
		}

		public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateSelectedIndex(picker);
		}

		public static void MapCharacterSpacing(PickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(picker);
		}

		public static void MapFont(PickerHandler handler, IPicker picker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(picker, fontManager);
		}

		public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateHorizontalAlignment(picker.HorizontalTextAlignment);
		}

		public static void MapTextColor(PickerHandler handler, IPicker picker)
		{
			handler.PlatformView.UpdateTextColor(picker, s_defaultTextColors);
		}

		public static void MapVerticalTextAlignment(PickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateVerticalAlignment(picker.VerticalTextAlignment);
		}

		void OnFocusChange(object? sender, global::Android.Views.View.FocusChangeEventArgs e)
		{
			if (PlatformView == null)
				return;

			if (e.HasFocus)
			{
				if (PlatformView.Clickable)
					PlatformView.CallOnClick();
				else
					OnClick(PlatformView, EventArgs.Empty);
			}
			else if (_dialog != null)
			{
				_dialog.Hide();
				PlatformView.ClearFocus();
				_dialog = null;
			}
		}

		void OnClick(object? sender, EventArgs e)
		{
			if (_dialog == null && VirtualView != null)
			{
				using (var builder = new AlertDialog.Builder(Context))
				{
					if (VirtualView.TitleColor == null)
					{
						builder.SetTitle(VirtualView.Title ?? string.Empty);
					}
					else
					{
						var title = new SpannableString(VirtualView.Title ?? string.Empty);
						title.SetSpan(new ForegroundColorSpan(VirtualView.TitleColor.ToPlatform()), 0, title.Length(), SpanTypes.ExclusiveExclusive);
						builder.SetTitle(title);
					}

					string[] items = VirtualView.GetItemsAsArray();

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

				_dialog.SetCanceledOnTouchOutside(true);

				_dialog.DismissEvent += (sender, args) =>
				{
					_dialog?.Dispose();
					_dialog = null;
				};

				_dialog.Show();
			}
		}

		void Reload()
		{
			if (VirtualView == null || PlatformView == null)
				return;

			PlatformView.UpdatePicker(VirtualView);
		}
	}
}