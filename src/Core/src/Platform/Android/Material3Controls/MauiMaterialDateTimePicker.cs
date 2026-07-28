using System;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Google.Android.Material.TextField;
using static Android.Views.View;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Shared Material 3 base view for the <c>DatePicker</c> and <c>TimePicker</c> fields on Android.
/// Provides the outlined <see cref="TextInputLayout"/> container, the inner readonly edit text,
/// and a trailing icon (calendar/clock) whose tap opens the platform picker dialog. Subclasses
/// only supply the end-icon drawable to use.
/// </summary>
public abstract class MauiMaterialDateTimePicker : MauiMaterialTextInputLayout
{
    MauiMaterialEditText? _inputEditText;
    PickerClickListener? _clickListener;

    protected MauiMaterialDateTimePicker(Context context, int endIconResource, string? hint = null) : base(context)
    {
        // Outlined box is the Material 3 resting-state appearance for date/time fields.
        BoxBackgroundMode = BoxBackgroundOutline;

        // The floating label shown at the top-left, cut into the outlined border (e.g. "Date").
        // Subclasses supply the text; when null no label is shown.
        Hint = hint;

        _inputEditText = new MauiMaterialEditText(Context!);
        AddView(_inputEditText);

        // Keep the field focusable so the TextInputLayout can draw its highlighted (focused)
        // outline while the picker dialog is open (the handler requests focus when showing it).
        // The soft keyboard and blinking cursor are suppressed because the value is chosen through
        // the picker dialog rather than typed. Tapping the field itself does NOT open the dialog.
        _inputEditText.FocusableInTouchMode = true;
        _inputEditText.InputType = InputTypes.Null;
        _inputEditText.KeyListener = null;
        _inputEditText.SetCursorVisible(false);

        _clickListener = new PickerClickListener(this);

        // Only the trailing (calendar/clock) end icon opens the picker dialog.
        EndIconMode = EndIconCustom;
        SetEndIconDrawable(endIconResource);
        SetEndIconOnClickListener(_clickListener);
    }

    protected MauiMaterialDateTimePicker(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    /// <summary>
    /// The readonly edit text hosted inside the outlined layout. Handlers use this to update the
    /// displayed text, text color, and font.
    /// </summary>
    public MauiMaterialEditText? InputEditText => _inputEditText;

    /// <summary>Invoked when the field (via its end icon) is tapped to open the picker dialog.</summary>
    public Action? ShowPicker { get; set; }

    /// <summary>Invoked to dismiss the picker dialog.</summary>
    public Action? HidePicker { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SetEndIconOnClickListener(null);

            _clickListener?.Dispose();
            _clickListener = null;

            _inputEditText = null;
        }

        base.Dispose(disposing);
    }

    sealed class PickerClickListener : Java.Lang.Object, IOnClickListener
    {
        readonly WeakReference<MauiMaterialDateTimePicker> _layout;

        public PickerClickListener(MauiMaterialDateTimePicker layout)
        {
            _layout = new WeakReference<MauiMaterialDateTimePicker>(layout);
        }

        public void OnClick(View? v)
        {
            if (_layout.TryGetTarget(out var layout))
            {
                layout.ShowPicker?.Invoke();
            }
        }
    }
}
