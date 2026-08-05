using System;
using Android.Content;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Google.Android.Material.TextField;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Shared Material 3 base view for the <c>DatePicker</c> and <c>TimePicker</c> fields on Android.
/// Provides the outlined <see cref="TextInputLayout"/> container, the inner readonly edit text,
/// and a trailing icon (calendar/clock) whose tap opens the platform picker dialog. Subclasses
/// only supply the end-icon drawable to use.
/// </summary>
public class MauiMaterialDateTimePickerBase : MauiMaterialTextInputLayout
{
    MauiMaterialEditText? _inputEditText;
    PickerClickListener? _clickListener;

    // The context is expected to already be theme-wrapped by the handler's CreatePlatformView;
    // the wrapped context then propagates to the inner edit text created below.
    protected MauiMaterialDateTimePickerBase(Context context, int endIconResource) : base(context)
    {
        // Outlined box is the Material 3 resting-state appearance for date/time fields.
        BoxBackgroundMode = BoxBackgroundOutline;
        _inputEditText = new MauiMaterialDateTimePickerEditText(Context!);
        AddView(_inputEditText);

        // The soft keyboard and blinking cursor are suppressed because the value is chosen through
        // the picker dialog rather than typed. Tapping the field itself does NOT open the dialog.
        _inputEditText.InputType = InputTypes.Null;
        _inputEditText.KeyListener = null;
        _inputEditText.SetCursorVisible(false);

        // Set focusability after InputType/KeyListener so RequestFocus() can highlight the outline
        // while the picker dialog is open.
        _inputEditText.FocusableInTouchMode = true;

        // Only the trailing (calendar/clock) end icon opens the picker dialog. The click listener that
        // wires the tap is attached by the handler in ConnectHandler so it can be torn down
        // deterministically in DisconnectHandler (the view itself is reused across reconnects).
        EndIconMode = EndIconCustom;
        SetEndIconDrawable(endIconResource);
    }

    /// <summary>
    /// The readonly edit text hosted inside the outlined layout. Handlers use this to update the
    /// displayed text, text color, and font.
    /// </summary>
    public MauiMaterialEditText? InputEditText => _inputEditText ??= EditText as MauiMaterialEditText;

    /// <summary>Invoked when the field (via its end icon) is tapped to open the picker dialog.</summary>
    public Action? ShowPicker { get; set; }

    /// <summary>Invoked to dismiss the picker dialog.</summary>
    public Action? HidePicker { get; set; }

    /// <summary>Attaches the end-icon tap listener. Called from the handler's ConnectHandler.</summary>
    internal void ConnectClickListener()
    {
        _clickListener ??= new PickerClickListener(this);
        SetEndIconOnClickListener(_clickListener);
    }

    /// <summary>Removes and disposes the end-icon tap listener. Called from the handler's DisconnectHandler.</summary>
    internal void DisconnectClickListener()
    {
        SetEndIconOnClickListener(null);
        _clickListener?.Dispose();
        _clickListener = null;
    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        if (MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.AtMost)
        {
            var maximumHeight = MeasureSpec.GetSize(heightMeasureSpec);
            var intrinsicHeightMeasureSpec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

            base.OnMeasure(widthMeasureSpec, intrinsicHeightMeasureSpec);
            SetMeasuredDimension(MeasuredWidth, Math.Min(MeasuredHeight, maximumHeight));
            return;
        }

        base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
    }

    sealed class PickerClickListener : Java.Lang.Object, IOnClickListener
    {
        readonly WeakReference<MauiMaterialDateTimePickerBase> _layout;

        public PickerClickListener(MauiMaterialDateTimePickerBase layout)
        {
            _layout = new WeakReference<MauiMaterialDateTimePickerBase>(layout);
        }

        public void OnClick(View? v)
        {
            if (_layout.TryGetTarget(out var layout))
            {
                layout.ShowPicker?.Invoke();
            }
        }
    }

    sealed class MauiMaterialDateTimePickerEditText : MauiMaterialEditText
    {
        public MauiMaterialDateTimePickerEditText(Context context) : base(context)
        {
        }

        protected override IMovementMethod? DefaultMovementMethod => null;
    }
}
