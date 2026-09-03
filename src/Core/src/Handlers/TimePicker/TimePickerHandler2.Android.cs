using System;
using Android.Content;
using Android.Text.Format;
using Android.Views;
using AndroidX.Fragment.App;
using Google.Android.Material.TimePicker;

namespace Microsoft.Maui.Handlers;

public partial class TimePickerHandler2 : ViewHandler<ITimePicker, MauiMaterialTimePicker>
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
        [nameof(IView.Focus)] = MapFocus,
        [nameof(IView.Unfocus)] = MapUnfocus,
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
        platformView.ConnectClickListener();

        // Focus lives on the inner edit text (the outer TextInputLayout never receives focus), so
        // subscribe here to keep VirtualView.IsFocused and the Focused/Unfocused events in sync.
        platformView.InputEditText?.FocusChange += OnInputFocusChange;
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

        _positiveButtonClickListener?.Dispose();
        _positiveButtonClickListener = null;
        _dismissListener?.Dispose();
        _dismissListener = null;
        platformView.ShowPicker = null;
        platformView.HidePicker = null;
        platformView.DisconnectClickListener();

        // Reset focusability enabled by RequestInputFocus/FocusInput; the platform view can be reused
        // across reconnects, so a leftover focusable read-only field could become an initial-focus candidate.
        // Clear focus while the listener is still attached so VirtualView.IsFocused is also reset.
        platformView.ClearInputFocus();
        platformView.InputEditText?.FocusChange -= OnInputFocusChange;

        base.DisconnectHandler(platformView);
    }

    void OnInputFocusChange(object? sender, View.FocusChangeEventArgs e)
    {
        if (VirtualView is null)
        {
            return;
        }

        VirtualView.IsFocused = e.HasFocus;
    }

    // The outer TextInputLayout never takes focus, so route IView.Focus/Unfocus to the inner edit text.
    public static void MapFocus(TimePickerHandler2 handler, ITimePicker picker, object? args)
    {
        if (args is FocusRequest request)
        {
            handler.PlatformView?.FocusInput(request);
        }
    }

    public static void MapUnfocus(TimePickerHandler2 handler, ITimePicker picker, object? args)
    {
        handler.PlatformView?.ClearInputFocus();
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
            PlatformView?.ClearInputFocus();
            UpdateIsOpenState(false);
            return;
        }

        RemoveListeners();

        if (_dialog.IsAdded)
        {
            _dialog.DismissAllowingStateLoss();
        }

        _dialog = null;
        PlatformView?.ClearInputFocus();
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
        if (fragmentManager is null || fragmentManager.IsStateSaved)
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

        // Focus the field before Show() so the outlined layout shows its highlighted (focused) state
        // and to avoid racing with the dialog window taking focus. This also covers opens triggered
        // programmatically via IsOpen.
        PlatformView?.RequestInputFocus();

        try
        {
            _dialog.Show(fragmentManager, "MaterialTimePicker");
        }
        catch (Java.Lang.IllegalStateException)
        {
            // A rejected fragment transaction (e.g. state saved in a race after the guard above) must not
            // strand the field focused with no dialog; restore the resting state and abort the open.
            _dialog = null;
            PlatformView?.ClearInputFocus();
            return;
        }

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
        handler.PlatformView?.UpdateBoxBackground(timePicker);
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
        handler.PlatformView?.InputEditText?.UpdateTime(picker);
    }

    public static void MapTextColor(TimePickerHandler2 handler, ITimePicker picker)
    {
        handler.PlatformView?.InputEditText?.UpdateTextColor(picker);
    }

    public static void MapFormat(TimePickerHandler2 handler, ITimePicker picker)
    {
        handler.PlatformView?.InputEditText?.UpdateFormat(picker);
    }

    public static void MapFont(TimePickerHandler2 handler, ITimePicker picker)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();

        handler.PlatformView?.InputEditText?.UpdateFont(picker, fontManager);
    }

    public static void MapCharacterSpacing(TimePickerHandler2 handler, ITimePicker picker)
    {
        handler.PlatformView?.InputEditText?.UpdateCharacterSpacing(picker);
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

public class MaterialTimePickerPositiveButtonClickListener : Java.Lang.Object, View.IOnClickListener
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

public class MaterialTimePickerDismissListener : Java.Lang.Object, IDialogInterfaceOnDismissListener
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
        handler.PlatformView?.ClearInputFocus();
        handler.UpdateIsOpenState(false);
    }
}