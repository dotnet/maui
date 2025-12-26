using System;
using Android.App;
using Android.Content;
using Android.Text.Format;
using Android.Views;
using Google.Android.Material.TimePicker;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers;

internal partial class MaterialTimePickerHandler : ViewHandler<ITimePicker, MauiMaterialTimePicker>
{
    internal MaterialTimePicker? _dialog;
    internal bool _isUpdatingIsOpen;
    internal MaterialTimePickerPositiveButtonClickListener? _positiveButtonClickListener;
    internal MaterialTimePickerDismissListener? _dismissListener;

    public static PropertyMapper<ITimePicker, MaterialTimePickerHandler> Mapper =
                new(ViewMapper)
                {
                    [nameof(ITimePicker.Background)] = MapBackground,
                    [nameof(ITimePicker.CharacterSpacing)] = MapCharacterSpacing,
                    [nameof(ITimePicker.Font)] = MapFont,
                    [nameof(ITimePicker.Format)] = MapFormat,
                    [nameof(ITimePicker.TextColor)] = MapTextColor,
                    [nameof(ITimePicker.Time)] = MapTime,
                    [nameof(ITimePicker.IsOpen)] = MapIsOpen,
                };

    public static CommandMapper<ITimePicker, ITimePickerHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public MaterialTimePickerHandler() : base(Mapper, CommandMapper)
    {
    }

    protected override void ConnectHandler(MauiMaterialTimePicker platformView)
    {
        base.ConnectHandler(platformView);

        _positiveButtonClickListener = new MaterialTimePickerPositiveButtonClickListener(this);
        _dismissListener = new MaterialTimePickerDismissListener(this);

        platformView.ShowPicker = ShowPickerDialog;
        platformView.HidePicker = HidePickerDialog;
    }

    protected override void DisconnectHandler(MauiMaterialTimePicker platformView)
    {
        if (_dialog is not null)
        {
            RemoveListeners();
            _dialog.Dismiss();
            _dialog = null;
        }

        _positiveButtonClickListener?.Dispose();
        _positiveButtonClickListener = null;

        _dismissListener?.Dispose();
        _dismissListener = null;

        platformView.ShowPicker = null;
        platformView.HidePicker = null;

        base.DisconnectHandler(platformView);
    }

    void RemoveListeners()
    {
        if (_dialog is not null)
        {
            if (_dismissListener is not null)
                _dialog.RemoveOnDismissListener(_dismissListener);
            if (_positiveButtonClickListener is not null)
                _dialog.RemoveOnPositiveButtonClickListener(_positiveButtonClickListener);
        }
    }


    internal void HidePickerDialog()
    {
        if (_dialog is not null)
        {
            RemoveListeners();
            _dialog.Dismiss();
        }

        _dialog = null;
        UpdateIsOpenState(false);
    }

    void ShowPickerDialog()
    {
        if (VirtualView is null)
        {
            return;
        }

        ShowPickerDialog(VirtualView.Time);
    }

    void ShowPickerDialog(TimeSpan? time)
    {
        var fragmentManager = Context?.GetFragmentManager();

        if (fragmentManager is null)
        {
            return;
        }

        if (_dialog is not null && _dialog.IsVisible)
        {
            return;
        }

        var hour = time?.Hours ?? 0;
        var minute = time?.Minutes ?? 0;

        _dialog = CreateTimePickerDialog(hour, minute);
        _dialog?.Show(fragmentManager, "MaterialTimePicker");

        UpdateIsOpenState(true);
    }

    protected virtual MaterialTimePicker? CreateTimePickerDialog(int hour, int minute)
    {
        var dialog = new MaterialTimePicker.Builder()
            .SetHour(hour)
            .SetMinute(minute)
            .SetTimeFormat(Use24HourView ? TimeFormat.Clock24h : TimeFormat.Clock12h)
            .SetInputMode(MaterialTimePicker.InputModeClock)  // Dial/Clock face mode
            .Build();

        if (_positiveButtonClickListener is not null && _dismissListener is not null)
        {
            dialog?.AddOnPositiveButtonClickListener(_positiveButtonClickListener);
            dialog?.AddOnDismissListener(_dismissListener);
        }

        return dialog;
    }

    public static void MapBackground(ITimePickerHandler handler, ITimePicker timePicker)
    {
        handler.PlatformView?.UpdateBackground(timePicker);
    }

    public static void MapIsOpen(MaterialTimePickerHandler handler, ITimePicker picker)
    {
        if (handler.IsConnected() && handler is MaterialTimePickerHandler timePickerHandler && !timePickerHandler._isUpdatingIsOpen)
        {
            if (picker.IsOpen)
            {
                timePickerHandler.ShowPickerDialog();
            }
            else
            {
                timePickerHandler.HidePickerDialog();
            }
        }
    }

    public static void MapTime(MaterialTimePickerHandler handler, ITimePicker picker)
    {
        handler.PlatformView?.UpdateFormat(picker);
    }

    public static void MapTextColor(MaterialTimePickerHandler handler, ITimePicker picker)
    {
        handler.PlatformView?.UpdateTextColor(picker);
    }

    public static void MapFormat(MaterialTimePickerHandler handler, ITimePicker picker)
    {
        handler.PlatformView?.UpdateFormat(picker);
    }

    public static void MapFont(MaterialTimePickerHandler handler, ITimePicker picker)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();

        handler.PlatformView?.UpdateFont(picker, fontManager);
    }

    public static void MapCharacterSpacing(MaterialTimePickerHandler handler, ITimePicker picker)
    {
        handler.PlatformView?.UpdateCharacterSpacing(picker);
    }

    protected override MauiMaterialTimePicker CreatePlatformView()
    {
        return new MauiMaterialTimePicker(Context);
    }

    internal void UpdateIsOpenState(bool isOpen)
    {
        if (VirtualView is null || _isUpdatingIsOpen)
        {
            return;
        }

        _isUpdatingIsOpen = true;
        VirtualView.IsOpen = isOpen;
        _isUpdatingIsOpen = false;
    }

    bool Use24HourView => VirtualView is not null && (DateFormat.Is24HourFormat(PlatformView?.Context)
            && VirtualView.Format == "t" || VirtualView.Format == "HH:mm");
}

internal class MaterialTimePickerPositiveButtonClickListener : Java.Lang.Object, View.IOnClickListener
{
    readonly WeakReference<MaterialTimePickerHandler> _handler;

    public MaterialTimePickerPositiveButtonClickListener(MaterialTimePickerHandler handler)
    {
        _handler = new WeakReference<MaterialTimePickerHandler>(handler);
    }

    public void OnClick(View? v)
    {
        if (!_handler.TryGetTarget(out var handler) || handler.VirtualView is null || handler._dialog is null)
            return;

        handler.VirtualView.Time = new TimeSpan(handler._dialog.Hour, handler._dialog.Minute, 0);
        handler.VirtualView.IsFocused = false;

        // HidePickerDialog removes all listeners and dismisses properly
        handler.HidePickerDialog();
    }
}

internal class MaterialTimePickerDismissListener : Java.Lang.Object, IDialogInterfaceOnDismissListener
{
    readonly WeakReference<MaterialTimePickerHandler> _handler;

    public MaterialTimePickerDismissListener(MaterialTimePickerHandler handler)
    {
        _handler = new WeakReference<MaterialTimePickerHandler>(handler);
    }

    public void OnDismiss(IDialogInterface? dialog)
    {
        if (!_handler.TryGetTarget(out var handler))
            return;

        // Dialog was dismissed (back button, outside tap, cancel button, etc.)
        // Clean up without trying to dismiss again
        handler._dialog = null;

        handler.UpdateIsOpenState(false);
    }
}