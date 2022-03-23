using System;
using System.Collections.Specialized;
using Android.App;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Text.Style;
using AResource = Android.Resource;
using Android.Views;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, Android.Views.View>
	{
		MauiPicker? GetMauiPicker() => PlatformView as MauiPicker;
		static MauiPicker? GetMauiPicker(IPickerHandler handler) => handler.PlatformView as MauiPicker;

		AlertDialog? _dialog;

		protected override View CreatePlatformView() =>
			new MauiPicker(Context);

		protected override void ConnectHandler(Android.Views.View platformView)
		{
			platformView.FocusChange += OnFocusChange;
			platformView.Click += OnClick;

			if (VirtualView.Items is INotifyCollectionChanged notifyCollection)
				notifyCollection.CollectionChanged += OnRowsCollectionChanged;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Android.Views.View platformView)
		{
			platformView.FocusChange -= OnFocusChange;
			platformView.Click -= OnClick;

			if (VirtualView.Items is INotifyCollectionChanged notifyCollection)
				notifyCollection.CollectionChanged -= OnRowsCollectionChanged;

			base.DisconnectHandler(platformView);
		}

		// This is a Android-specific mapping
		public static void MapBackground(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateBackground(picker);
		}

		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		public static void MapTitle(IPickerHandler handler, IPicker picker)
		{
			GetMauiPicker(handler)?.UpdateTitle(picker);
		}

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			GetMauiPicker(handler)?.UpdateTitleColor(picker);
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
			GetMauiPicker(handler)?.UpdateSelectedIndex(picker);
		}

		public static void MapCharacterSpacing(IPickerHandler handler, IPicker picker)
		{
			GetMauiPicker(handler)?.UpdateCharacterSpacing(picker);
		}

		public static void MapFont(IPickerHandler handler, IPicker picker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			GetMauiPicker(handler)?.UpdateFont(picker, fontManager);
		}

		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			GetMauiPicker(handler)?.UpdateHorizontalAlignment(picker.HorizontalTextAlignment);
		}

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
			GetMauiPicker(handler)?.UpdateTextColor(picker);
		}

		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			GetMauiPicker(handler)?.UpdateVerticalAlignment(picker.VerticalTextAlignment);
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
						GetMauiPicker()?.UpdatePicker(VirtualView);
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

		void OnRowsCollectionChanged(object? sender, EventArgs e)
		{
			Reload(this);
		}

		static void Reload(IPickerHandler handler)
		{
			if (handler.VirtualView == null || handler.PlatformView == null)
				return;

			GetMauiPicker(handler)?.UpdatePicker(handler.VirtualView);
		}
	}
}