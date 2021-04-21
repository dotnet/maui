using System;
using System.Collections.Specialized;
using System.Linq;
using Android.App;
using AResource = Android.Resource;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiPicker>
	{
		AlertDialog? _dialog;

		protected override MauiPicker CreateNativeView() =>
			new MauiPicker(Context);

		protected override void ConnectHandler(MauiPicker nativeView)
		{
			nativeView.FocusChange += OnFocusChange;
			nativeView.Click += OnClick;

			if (VirtualView != null && VirtualView.Items is INotifyCollectionChanged notifyCollection)
				notifyCollection.CollectionChanged += OnCollectionChanged;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiPicker nativeView)
		{
			nativeView.FocusChange -= OnFocusChange;
			nativeView.Click -= OnClick;

			if (VirtualView != null && VirtualView.Items is INotifyCollectionChanged notifyCollection)
				notifyCollection.CollectionChanged -= OnCollectionChanged;

			base.DisconnectHandler(nativeView);
		}
		public static void MapTitle(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateTitle(picker);
		}

		public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateSelectedIndex(picker);
		}

		public static void MapCharacterSpacing(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateCharacterSpacing(picker);
		}

		public static void MapFont(PickerHandler handler, IPicker picker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(picker, fontManager);
		}

		public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker picker)
		{
			var nativePicker = handler.NativeView;
			var hasRtlSupport = nativePicker?.Context!.HasRtlSupport() ?? false;
			nativePicker?.UpdateHorizontalAlignment(picker.HorizontalTextAlignment, hasRtlSupport);
		}

		[MissingMapper]
		public static void MapTextColor(PickerHandler handler, IPicker view) { }

		void OnFocusChange(object? sender, global::Android.Views.View.FocusChangeEventArgs e)
		{
			if (NativeView == null)
				return;

			if (e.HasFocus)
			{
				if (NativeView.Clickable)
					NativeView.CallOnClick();
				else
					OnClick(NativeView, EventArgs.Empty);
			}
			else if (_dialog != null)
			{
				_dialog.Hide();
				NativeView.ClearFocus();
				_dialog = null;
			}
		}

		void OnClick(object? sender, EventArgs e)
		{
			if (_dialog == null && VirtualView != null)
			{
				using (var builder = new AlertDialog.Builder(Context))
				{
					builder.SetTitle(VirtualView.Title ?? string.Empty);

					string[] items = VirtualView.Items.ToArray();

					builder.SetItems(items, (EventHandler<Android.Content.DialogClickEventArgs>)((s, e) =>
					{
						var selectedIndex = e.Which;
						VirtualView.SelectedIndex = selectedIndex;
						base.NativeView?.UpdatePicker(VirtualView);
					}));

					builder.SetNegativeButton(AResource.String.Cancel, (o, args) => { });

					_dialog = builder.Create();
				}

				if (_dialog == null)
					return;

				_dialog.SetCanceledOnTouchOutside(true);

				_dialog.DismissEvent += (sender, args) =>
				{
					_dialog.Dispose();
					_dialog = null;
				};

				_dialog.Show();
			}
		}

		void OnCollectionChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			NativeView.UpdatePicker(VirtualView);
		}
	}
}