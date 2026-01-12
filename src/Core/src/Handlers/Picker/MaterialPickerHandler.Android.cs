using System;
using Android.Text;
using Android.Text.Style;
using Google.Android.Material.Dialog;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;
using AResource = Android.Resource;

namespace Microsoft.Maui.Handlers;

internal partial class MaterialPickerHandler : ViewHandler<IPicker, MauiMaterialPicker>
{
	AppCompatAlertDialog? _dialog;

	public static PropertyMapper<IPicker, MaterialPickerHandler> Mapper =
		new(ViewMapper)
		{
			[nameof(IPicker.Background)] = MapBackground,
			[nameof(IPicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IPicker.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IPicker.Items)] = MapItems,
			[nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
			[nameof(IPicker.TextColor)] = MapTextColor,
			[nameof(IPicker.Title)] = MapTitle,
			[nameof(IPicker.TitleColor)] = MapTitleColor,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(IPicker.IsOpen)] = MapIsOpen,
		};

	public static CommandMapper<IPicker, MaterialPickerHandler> CommandMapper = new(ViewCommandMapper)
	{
		[nameof(IPicker.Focus)] = MapFocus,
		[nameof(IPicker.Unfocus)] = MapUnfocus,
	};

	public MaterialPickerHandler() : base(Mapper, CommandMapper)
	{
	}

	protected override MauiMaterialPicker CreatePlatformView()
	{
		return new MauiMaterialPicker(Context);
	}

	protected override void ConnectHandler(MauiMaterialPicker platformView)
	{
		platformView.Click += OnClick;

		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(MauiMaterialPicker platformView)
	{
		platformView.Click -= OnClick;

		if (_dialog is not null)
		{
			_dialog.ShowEvent -= OnDialogShown;
			_dialog.DismissEvent -= OnDialogDismiss;
			_dialog.Dismiss();
			_dialog = null;
		}

		base.DisconnectHandler(platformView);
	}

	public static void MapBackground(MaterialPickerHandler handler, IPicker picker)
	{
		handler.PlatformView?.UpdateBackground(picker);
	}

	internal static void MapItems(MaterialPickerHandler handler, IPicker picker) => Reload(handler);

	public static void MapTitle(MaterialPickerHandler handler, IPicker picker)
	{
		handler.PlatformView?.UpdateTitle(picker);
	}

	public static void MapTitleColor(MaterialPickerHandler handler, IPicker picker)
	{
		handler.PlatformView?.UpdateTitleColor(picker);
	}

	public static void MapSelectedIndex(MaterialPickerHandler handler, IPicker picker)
	{
		handler.PlatformView?.UpdateSelectedIndex(picker);
	}

	public static void MapCharacterSpacing(MaterialPickerHandler handler, IPicker picker)
	{
		handler.PlatformView?.UpdateCharacterSpacing(picker);
	}

	public static void MapFont(MaterialPickerHandler handler, IPicker picker)
	{
		var fontManager = handler.GetRequiredService<IFontManager>();

		handler.PlatformView?.UpdateFont(picker, fontManager);
	}

	public static void MapHorizontalTextAlignment(MaterialPickerHandler handler, IPicker picker)
	{
		handler.PlatformView?.UpdateHorizontalAlignment(picker.HorizontalTextAlignment);
	}

	public static void MapTextColor(MaterialPickerHandler handler, IPicker picker)
	{
		handler.PlatformView.UpdateTextColor(picker.TextColor);
	}

	public static void MapVerticalTextAlignment(MaterialPickerHandler handler, IPicker picker)
	{
		handler.PlatformView?.UpdateVerticalAlignment(picker.VerticalTextAlignment);
	}

	internal static void MapIsOpen(MaterialPickerHandler handler, IPicker picker)
	{
		if (handler.IsConnected())
		{
			if (picker.IsOpen)
			{
				handler.ShowDialog();
			}
			else
			{
				handler.DismissDialog();
			}
		}
	}

	internal static void MapFocus(MaterialPickerHandler handler, IPicker picker, object? args)
	{
		if (handler.IsConnected())
		{
			ViewHandler.MapFocus(handler, picker, args);
			handler.ShowDialog();
		}
	}

	internal static void MapUnfocus(MaterialPickerHandler handler, IPicker picker, object? args)
	{
		if (handler.IsConnected())
		{
			handler.DismissDialog();
			ViewHandler.MapUnfocus(handler, picker, args);
		}
	}

	void ShowDialog()
	{
		if (PlatformView.Clickable)
		{
			PlatformView.CallOnClick();
		}
		else
		{
			OnClick(PlatformView, EventArgs.Empty);
		}
	}

	void DismissDialog()
	{
		_dialog?.Dismiss();
	}

	void OnClick(object? sender, EventArgs e)
	{
		if (_dialog is null && VirtualView is not null)
		{
			// Use MauiMaterialContextThemeWrapper to ensure proper Material 3 theming
			// for the dialog when Material 3 is enabled in the project
			using (var builder = new MaterialAlertDialogBuilder(MauiMaterialContextThemeWrapper.Create(Context)))
			{
				if (VirtualView.TitleColor is null)
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
					if (item is null)
					{
						items[i] = String.Empty;
					}
				}

				builder.SetSingleChoiceItems(items, VirtualView.SelectedIndex, (s, e) =>
				{
					var selectedIndex = e.Which;
					VirtualView.SelectedIndex = selectedIndex;
					base.PlatformView?.UpdatePicker(VirtualView);

					_dialog?.Dismiss();
				});

				builder.SetNegativeButton(AResource.String.Cancel, (o, args) => { });

				_dialog = builder.Create();
			}

			if (_dialog is null)
			{
				return;
			}

			_dialog.UpdateFlowDirection(PlatformView);
			_dialog.SetCanceledOnTouchOutside(true);

			_dialog.ShowEvent += OnDialogShown;
			_dialog.DismissEvent += OnDialogDismiss;

			_dialog.Show();
			VirtualView.IsOpen = true;
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

	static void Reload(MaterialPickerHandler handler)
	{
		handler.PlatformView.UpdatePicker(handler.VirtualView);
	}
}