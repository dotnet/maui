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

    protected MauiMaterialDateTimePicker(Context context, int endIconResource) : base(context)
    {
        // Outlined box is the Material 3 resting-state appearance for date/time fields.
        BoxBackgroundMode = BoxBackgroundOutline;

        _inputEditText = new MauiMaterialEditText(Context!);
        AddView(_inputEditText);

        // Make the field focusable on tap so the TextInputLayout draws its highlighted (focused)
        // outline just like the Entry control. The soft keyboard and blinking cursor are suppressed
        // because the value is chosen through the picker dialog rather than typed.
        _inputEditText.FocusableInTouchMode = true;
        _inputEditText.Clickable = true;
        _inputEditText.InputType = InputTypes.Null;
        _inputEditText.KeyListener = null;
        _inputEditText.SetCursorVisible(false);

        _clickListener = new PickerClickListener(this);
        _inputEditText.SetOnClickListener(_clickListener);
        _inputEditText.SetOnTouchListener(_clickListener);

        // Custom mode shows the icon regardless of focus state (matching Material 2 behavior).
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

            _inputEditText?.SetOnClickListener(null);
            _inputEditText?.SetOnTouchListener(null);

            _clickListener?.Dispose();
            _clickListener = null;

            _inputEditText = null;
        }

        base.Dispose(disposing);
    }

    sealed class PickerClickListener : Java.Lang.Object, IOnClickListener, IOnTouchListener
    {
        readonly WeakReference<MauiMaterialDateTimePicker> _layout;
        readonly int _touchSlop;
        float _downX;
        float _downY;

        public PickerClickListener(MauiMaterialDateTimePicker layout)
        {
            _layout = new WeakReference<MauiMaterialDateTimePicker>(layout);
            _touchSlop = layout.Context is { } context ? ViewConfiguration.Get(context)?.ScaledTouchSlop ?? 0 : 0;
        }

        public void OnClick(View? v)
        {
            Open();
        }

        // A focusable-in-touch-mode view swallows its first tap just to acquire focus and only
        // fires OnClick on the second tap. To open on the very first tap we detect the tap here
        // (an ACTION_UP with no significant movement) and still return false so the view can take
        // focus and the outlined layout shows its highlighted state. OnClick remains wired for
        // the end icon and accessibility activation.
        public bool OnTouch(View? v, MotionEvent? e)
        {
            switch (e?.Action)
            {
                case MotionEventActions.Down:
                    _downX = e.RawX;
                    _downY = e.RawY;
                    break;
                case MotionEventActions.Up:
                    if (Math.Abs(e.RawX - _downX) <= _touchSlop &&
                        Math.Abs(e.RawY - _downY) <= _touchSlop)
                    {
                        Open();
                    }
                    break;
            }

            return false;
        }

        void Open()
        {
            if (_layout.TryGetTarget(out var layout))
            {
                layout.ShowPicker?.Invoke();
            }
        }
    }
}
