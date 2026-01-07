using System;
using System.Collections.Generic;
using Android.Content;
using AndroidX.Fragment.App;
using Google.Android.Material.DatePicker;

namespace Microsoft.Maui.Handlers;

internal partial class MaterialDatePickerHandler : ViewHandler<IDatePicker, MauiMaterialDatePicker>
{
    internal MaterialDatePicker? _dialog;
    internal bool _isUpdatingIsOpen;
    internal MaterialDatePickerPositiveButtonClickListener? _positiveButtonClickListener;
    internal MaterialDatePickerDismissListener? _dismissListener;
    public static PropertyMapper<IDatePicker, MaterialDatePickerHandler> Mapper =
                    new(ViewMapper)
                    {
                        [nameof(IDatePicker.Background)] = MapBackground,
                        [nameof(IDatePicker.CharacterSpacing)] = MapCharacterSpacing,
                        [nameof(IDatePicker.Date)] = MapDate,
                        [nameof(IDatePicker.Font)] = MapFont,
                        [nameof(IDatePicker.Format)] = MapFormat,
                        [nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
                        [nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
                        [nameof(IDatePicker.TextColor)] = MapTextColor,
                        [nameof(IDatePicker.IsOpen)] = MapIsOpen,

                    };

    static void MapBackground(MaterialDatePickerHandler handler, IDatePicker datePicker)
    {
        handler.PlatformView?.UpdateBackground(datePicker);
    }

    static void MapIsOpen(MaterialDatePickerHandler handler, IDatePicker picker)
    {
        if (handler.IsConnected() && handler is MaterialDatePickerHandler platformHandler && !platformHandler._isUpdatingIsOpen)
        {
            if (picker.IsOpen)
            {
                platformHandler.ShowPickerDialog();
            }
            else
            {
                platformHandler.HidePickerDialog();
            }
        }
    }

    static void MapTextColor(MaterialDatePickerHandler handler, IDatePicker picker)
    {
        handler.PlatformView?.UpdateTextColor(picker);
    }

    static void MapMinimumDate(MaterialDatePickerHandler handler, IDatePicker picker)
    {
    }

    static void MapMaximumDate(MaterialDatePickerHandler handler, IDatePicker picker)
    {
    }

    static void MapFormat(MaterialDatePickerHandler handler, IDatePicker picker)
    {
        handler.PlatformView?.UpdateFormat(picker);
    }

    static void MapFont(MaterialDatePickerHandler handler, IDatePicker picker)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();

        handler.PlatformView?.UpdateFont(picker, fontManager);
    }

    static void MapDate(MaterialDatePickerHandler handler, IDatePicker picker)
    {
        handler.PlatformView?.UpdateDate(picker);
    }

    static void MapCharacterSpacing(MaterialDatePickerHandler handler, IDatePicker picker)
    {
        handler.PlatformView?.UpdateCharacterSpacing(picker);
    }

    public static CommandMapper<ITimePicker, ITimePickerHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public MaterialDatePickerHandler() : base(Mapper, CommandMapper)
    {
    }

    protected override MauiMaterialDatePicker CreatePlatformView()
    {
        return new MauiMaterialDatePicker(MauiMaterialContextThemeWrapper.Create(Context));
    }

    protected override void ConnectHandler(MauiMaterialDatePicker platformView)
    {
        base.ConnectHandler(platformView);

        _positiveButtonClickListener = new MaterialDatePickerPositiveButtonClickListener(this);
        _dismissListener = new MaterialDatePickerDismissListener(this);

        platformView.ShowPicker = ShowPickerDialog;
        platformView.HidePicker = HidePickerDialog;
    }

    protected override void DisconnectHandler(MauiMaterialDatePicker platformView)
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

    protected virtual MaterialDatePicker? CreateDatePickerDialog(int year, int month, int day)
    {
        long selection = GetUtcMilliseconds(year, month, day);

        var builder = MaterialDatePicker.Builder.DatePicker()
            .SetSelection(selection)
            .SetInputMode(MaterialDatePicker.InputModeCalendar);

        var constraints = BuildCalendarConstraints();
        if (constraints is not null)
        {
            builder.SetCalendarConstraints(constraints);
        }

        var dialog = builder.Build();

        if (_positiveButtonClickListener is not null && _dismissListener is not null)
        {
            dialog.AddOnPositiveButtonClickListener(_positiveButtonClickListener);
            dialog.AddOnDismissListener(_dismissListener);
        }

        return dialog;
    }

    CalendarConstraints? BuildCalendarConstraints()
    {
        var minDate = VirtualView?.MinimumDate;
        var maxDate = VirtualView?.MaximumDate;

        if (!minDate.HasValue && !maxDate.HasValue)
        {
            return null;
        }

        var constraintsBuilder = new CalendarConstraints.Builder();
        var validators = new List<CalendarConstraints.IDateValidator>(2);

        if (minDate.HasValue)
        {
            long minMillis = GetUtcMilliseconds(minDate.Value.Year, minDate.Value.Month, minDate.Value.Day);
            constraintsBuilder.SetStart(minMillis);
            validators.Add(DateValidatorPointForward.From(minMillis));
        }

        if (maxDate.HasValue)
        {
            long maxMillis = GetUtcMilliseconds(maxDate.Value.Year, maxDate.Value.Month, maxDate.Value.Day);
            constraintsBuilder.SetEnd(maxMillis);
            // Add 1 day (86400000 milliseconds) because Before() is exclusive, we want inclusive
            validators.Add(DateValidatorPointBackward.Before(maxMillis + 86400000));
        }

        if (validators.Count > 0)
        {
            var validator = validators.Count == 1
                ? validators[0]
                : CompositeDateValidator.AllOf(validators);
            constraintsBuilder.SetValidator(validator);
        }

        return constraintsBuilder.Build();
    }

    static long GetUtcMilliseconds(int year, int month, int day)
    {
        var date = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
        return date.ToUnixTimeMilliseconds();
    }

    void ShowPickerDialog()
    {
        if (VirtualView is null)
        {
            return;
        }

        ShowPickerDialog(VirtualView.Date);
    }

    void ShowPickerDialog(DateTime? date)
    {
        // Get FragmentActivity - MaterialDatePicker requires AndroidX FragmentManager
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

        var year = date?.Year ?? DateTime.Today.Year;
        var month = date?.Month ?? DateTime.Today.Month;
        var day = date?.Day ?? DateTime.Today.Day;

        _dialog = CreateDatePickerDialog(year, month, day);
        _dialog?.Show(fragmentManager, "MaterialDatePicker");

        UpdateIsOpenState(true);
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
}

internal class MaterialDatePickerPositiveButtonClickListener : Java.Lang.Object, IMaterialPickerOnPositiveButtonClickListener
{
    readonly WeakReference<MaterialDatePickerHandler> _handler;

    public MaterialDatePickerPositiveButtonClickListener(MaterialDatePickerHandler handler)
    {
        _handler = new WeakReference<MaterialDatePickerHandler>(handler);
    }

    public void OnPositiveButtonClick(Java.Lang.Object? selection)
    {
        if (!_handler.TryGetTarget(out var handler) || handler.VirtualView is null)
        {
            return;
        }

        // Get the selected date from the dialog
        if (selection is Java.Lang.Long selectionLong)
        {
            var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(selectionLong.LongValue());
            handler.VirtualView.Date = dateTimeOffset.UtcDateTime.Date;
        }

        handler.VirtualView.IsFocused = false;

        // HidePickerDialog removes all listeners and dismisses properly
        handler.HidePickerDialog();
    }
}

internal class MaterialDatePickerDismissListener : Java.Lang.Object, IDialogInterfaceOnDismissListener
{
    readonly WeakReference<MaterialDatePickerHandler> _handler;

    public MaterialDatePickerDismissListener(MaterialDatePickerHandler handler)
    {
        _handler = new WeakReference<MaterialDatePickerHandler>(handler);
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