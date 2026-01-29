using System;
using Android.Text;
using Android.Text.Style;
using Google.Android.Material.Dialog;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;
using AResource = Android.Resource;

namespace Microsoft.Maui.Handlers;

// TODO: Material3 - make it public in .net 11
internal partial class PickerHandler2 : ViewHandler<IPicker, MauiMaterialPicker>
{
	AppCompatAlertDialog? _dialog;

	public static PropertyMapper<IPicker, PickerHandler2> Mapper =
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

	public static CommandMapper<IPicker, PickerHandler2> CommandMapper = new(ViewCommandMapper)
	{
		[nameof(IPicker.Focus)] = MapFocus,
		[nameof(IPicker.Unfocus)] = MapUnfocus,
	};

	public PickerHandler2() : base(Mapper, CommandMapper)
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

	public static void MapBackground(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView?.UpdateBackground(picker);
	}

	internal static void MapItems(PickerHandler2 handler, IPicker picker) => Reload(handler);

	public static void MapTitle(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView?.UpdateTitle(picker);
	}

	public static void MapTitleColor(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView?.UpdateTitleColor(picker);
	}

	public static void MapSelectedIndex(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView?.UpdateSelectedIndex(picker);
	}

	public static void MapCharacterSpacing(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView?.UpdateCharacterSpacing(picker);
	}

	public static void MapFont(PickerHandler2 handler, IPicker picker)
	{
		var fontManager = handler.GetRequiredService<IFontManager>();

		handler.PlatformView?.UpdateFont(picker, fontManager);
	}

	public static void MapHorizontalTextAlignment(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView?.UpdateHorizontalAlignment(picker.HorizontalTextAlignment);
	}

	public static void MapTextColor(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView?.UpdateTextColor(picker.TextColor);
	}

	public static void MapVerticalTextAlignment(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView?.UpdateVerticalAlignment(picker.VerticalTextAlignment);
	}

	internal static void MapIsOpen(PickerHandler2 handler, IPicker picker)
	{
		if (handler.IsConnected())
		{
			if (picker.IsOpen)
			{
				handler.ShowDialog();
			}
			else
			{
				handler.DismissPickerDialog();
			}
		}
	}

	internal static void MapFocus(PickerHandler2 handler, IPicker picker, object? args)
	{
		if (handler.IsConnected())
		{
			ViewHandler.MapFocus(handler, picker, args);
			handler.ShowDialog();
		}
	}

	internal static void MapUnfocus(PickerHandler2 handler, IPicker picker, object? args)
	{
		if (handler.IsConnected())
		{
			handler.DismissPickerDialog();
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

	void OnClick(object? sender, EventArgs e)
	{
		if (_dialog is null && VirtualView is not null)
		{
			CreatePickerDialog();
			ShowPickerDialog();
		}
	}

	void CreatePickerDialog()
	{
		// Use MauiMaterialContextThemeWrapper to ensure proper Material 3 theming
		// for the dialog when Material 3 is enabled in the project
		using (var builder = new MaterialAlertDialogBuilder(MauiMaterialContextThemeWrapper.Create(Context)))
		{
			SetupDialogTitle(builder);
			SetupDialogItems(builder);
			builder.SetNegativeButton(AResource.String.Cancel, (o, args) => { });

			_dialog = builder.Create();
		}
	}

	void SetupDialogTitle(MaterialAlertDialogBuilder builder)
	{
		if (VirtualView.TitleColor is null)
		{
			builder.SetTitle(VirtualView.Title ?? string.Empty);
		}
		else
		{
			var title = new SpannableString(VirtualView.Title ?? string.Empty);
			title.SetSpan(new ForegroundColorSpan(VirtualView.TitleColor.ToPlatform()), 0, title.Length(), SpanTypes.ExclusiveExclusive);
			builder.SetTitle(title);
		}
	}

	void SetupDialogItems(MaterialAlertDialogBuilder builder)
	{
		string[] items = VirtualView.GetItemsAsArray();

		// Ensure no null items
		for (var i = 0; i < items.Length; i++)
		{
			var item = items[i];
			if (item is null)
			{
				items[i] = String.Empty;
			}
		}

		builder.SetSingleChoiceItems(items, VirtualView.SelectedIndex, (sender, e) =>
		{
			var selectedIndex = e.Which;
			VirtualView.SelectedIndex = selectedIndex;
			PlatformView?.UpdatePicker(VirtualView);

			DismissPickerDialog();
		});
	}

	void ShowPickerDialog()
	{
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

	void DismissPickerDialog()
	{
		_dialog?.Dismiss();
	}

	void OnDialogDismiss(object? sender, EventArgs e)
	{
		if (_dialog is null)
		{
			return;
		}

		// Clean up ALL event handlers before nulling the reference
		_dialog.ShowEvent -= OnDialogShown;  // Remove ShowEvent handler to prevent leak
		_dialog.DismissEvent -= OnDialogDismiss;
		VirtualView.IsFocused = false;
		VirtualView.IsOpen = false;
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

	static void Reload(PickerHandler2 handler)
	{
		handler.PlatformView?.UpdatePicker(handler.VirtualView);
	}
}