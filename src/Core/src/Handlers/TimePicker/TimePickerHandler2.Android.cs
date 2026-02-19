using System;
using Android.Content;
using Android.Text.Format;
using Android.Views;
using AndroidX.Fragment.App;
using Google.Android.Material.TimePicker;

namespace Microsoft.Maui.Handlers;

// TODO: Material3: Make it public in .NET 11
internal partial class TimePickerHandler2 : ViewHandler<ITimePicker, MauiMaterialTimePicker>
{
    internal MaterialTimePicker? _dialog;
    internal bool _isUpdatingIsOpen;
    internal MaterialTimePickerPositiveButtonClickListener? _positiveButtonClickListener;
    internal MaterialTimePickerDismissListener? _dismissListener;

    public static PropertyMapper<ITimePicker, TimePickerHandler2> Mapper =
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

    public static CommandMapper<ITimePicker, TimePickerHandler2> CommandMapper = new(ViewCommandMapper)
    {
    };

    public TimePickerHandler2() : base(Mapper, CommandMapper)
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

            if (_dialog.IsAdded)
            {
                _dialog.DismissAllowingStateLoss();
            }

            _dialog = null;
        }

        _positiveButtonClickListener = null;
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
            {
                _dialog.RemoveOnDismissListener(_dismissListener);
            }
            if (_positiveButtonClickListener is not null)
            {
                _dialog.RemoveOnPositiveButtonClickListener(_positiveButtonClickListener);
            }
        }
    }


    internal void HidePickerDialog()
    {
        if (_dialog is null)
        {
            UpdateIsOpenState(false);
            return;
        }

        RemoveListeners();

        if (_dialog.IsAdded)
        {
            _dialog.DismissAllowingStateLoss();
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
        // Get FragmentActivity - MaterialTimePicker requires AndroidX FragmentManager
        if (Context?.GetActivity() is not FragmentActivity fragmentActivity ||
            fragmentActivity.IsDestroyed ||
            fragmentActivity.IsFinishing)
        {
            return;
        }

        var fragmentManager = fragmentActivity.SupportFragmentManager;
        if (fragmentManager is null)
        {
            return;
        }

        // Prevent duplicate dialogs
        if (_dialog is not null && (_dialog.IsVisible || _dialog.IsAdded))
        {
            return;
        }

        var hour = time?.Hours ?? 0;
        var minute = time?.Minutes ?? 0;

        _dialog = CreateTimePickerDialog(hour, minute);
        if (_dialog is null)
        {
            return;
        }

        _dialog.Show(fragmentManager, "MaterialTimePicker");

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

    public static void MapBackground(TimePickerHandler2 handler, ITimePicker timePicker)
    {
        handler.PlatformView?.UpdateBackground(timePicker);
    }

    public static void MapIsOpen(TimePickerHandler2 handler, ITimePicker picker)
    {
        if (handler.IsConnected() && !handler._isUpdatingIsOpen)
        {
            if (picker.IsOpen)
            {
                handler.ShowPickerDialog();
            }
            else
            {
                handler.HidePickerDialog();
            }
        }
    }

    public static void MapTime(TimePickerHandler2 handler, ITimePicker picker)
    {
        handler.PlatformView?.UpdateTime(picker);
    }

    public static void MapTextColor(TimePickerHandler2 handler, ITimePicker picker)
    {
        handler.PlatformView?.UpdateTextColor(picker);
    }

    public static void MapFormat(TimePickerHandler2 handler, ITimePicker picker)
    {
        handler.PlatformView?.UpdateFormat(picker);
    }

    public static void MapFont(TimePickerHandler2 handler, ITimePicker picker)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();

        handler.PlatformView?.UpdateFont(picker, fontManager);
    }

    public static void MapCharacterSpacing(TimePickerHandler2 handler, ITimePicker picker)
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
    readonly WeakReference<TimePickerHandler2> _handler;

    public MaterialTimePickerPositiveButtonClickListener(TimePickerHandler2 handler)
    {
        _handler = new WeakReference<TimePickerHandler2>(handler);
    }

    public void OnClick(View? v)
    {
        if (!_handler.TryGetTarget(out var handler) || handler.VirtualView is null || handler._dialog is null)
        {
            return;
        }

        handler.VirtualView.Time = new TimeSpan(handler._dialog.Hour, handler._dialog.Minute, 0);
        handler.VirtualView.IsFocused = false;

        // HidePickerDialog removes all listeners and dismisses properly
        handler.HidePickerDialog();
    }
}

internal class MaterialTimePickerDismissListener : Java.Lang.Object, IDialogInterfaceOnDismissListener
{
    readonly WeakReference<TimePickerHandler2> _handler;

    public MaterialTimePickerDismissListener(TimePickerHandler2 handler)
    {
        _handler = new WeakReference<TimePickerHandler2>(handler);
    }

    public void OnDismiss(IDialogInterface? dialog)
    {
        if (!_handler.TryGetTarget(out var handler))
        {
            return;
        }

        // Dialog was dismissed (back button, outside tap, cancel button, etc.)
        // Clean up without trying to dismiss again
        handler._dialog = null;

        handler.UpdateIsOpenState(false);
    }
}