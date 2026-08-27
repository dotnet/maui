using System;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Google.Android.Material.TextField;
using Microsoft.Maui.Graphics;

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

    protected MauiMaterialDateTimePickerBase(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    // The derived picker constructors (MauiMaterialDatePicker/MauiMaterialTimePicker) theme-wrap the
    // context via MauiMaterialContextThemeWrapper.Create before calling base; the wrapped context then
    // propagates to the inner edit text created below.
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

        // Keep the field non-focusable at rest so it is never an initial-focus candidate; focusability
        // is enabled only while the picker dialog is open (see RequestInputFocus).
        _inputEditText.Focusable = false;
        _inputEditText.FocusableInTouchMode = false;

        // Only the trailing (calendar/clock) end icon opens the picker dialog. The click listener that
        // wires the tap is attached by the handler in ConnectHandler so it can be torn down
        // deterministically in DisconnectHandler (the view itself is reused across reconnects).
        EndIconMode = EndIconCustom;
        SetEndIconDrawable(endIconResource);

        // EndIconCustom does not auto-show the icon; without this the icon stays out of the layout
        // and the accessibility tree, so UI automation and screen readers cannot locate it.
        EndIconVisible = true;
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

    // Applies IView.Background to the outlined box fill. In BoxBackgroundOutline mode the visible fill/stroke
    // are drawn on the inner edit text, so setting the outer ViewGroup background (the generic handler path)
    // would render behind the box; target the box color instead (matching the Material SearchBar).
    internal void UpdateBoxBackground(IView view)
    {
        if (view.Background is SolidPaint solidPaint)
        {
            var colorInt = (int)solidPaint.Color.ToPlatform();
            SetBoxBackgroundColorStateList(ColorStateListExtensions.CreateEditText(colorInt, colorInt));
        }
        else if (view.Background is null)
        {
            // Restore the outlined default (transparent box fill) if a color was previously applied.
            var transparent = global::Android.Graphics.Color.Transparent.ToArgb();
            SetBoxBackgroundColorStateList(ColorStateListExtensions.CreateEditText(transparent, transparent));
        }
    }

    /// <summary>Removes and disposes the end-icon tap listener. Called from the handler's DisconnectHandler.</summary>
    internal void DisconnectClickListener()
    {
        SetEndIconOnClickListener(null);
        _clickListener?.Dispose();
        _clickListener = null;
    }

    // Focuses the inner edit text so the outlined layout shows its focused stroke while the picker
    // dialog is open. Focusability is toggled on only for the dialog's lifetime so the read-only
    // field is never an initial-focus candidate at rest.
    internal void RequestInputFocus()
    {
        EnableInputFocusable()?.RequestFocus();
    }

    // Routes a programmatic IView.Focus request to the inner edit text (the outer TextInputLayout
    // never takes focus). Uses Focus(request) so the FocusRequest result is completed for the framework.
    internal void FocusInput(FocusRequest request)
    {
        if (EnableInputFocusable() is not { } editText)
        {
            // No inner edit text (e.g. the JNI activation path): complete the request so the framework's
            // Focus() resolves to false instead of throwing "No result value was set."
            request.TrySetResult(false);
            return;
        }

        editText.Focus(request);
    }

    // The read-only field is non-focusable at rest; enable focusability before requesting focus.
    // Resolves the inner edit text via the InputEditText property so it works on the JNI activation path.
    MauiMaterialEditText? EnableInputFocusable()
    {
        if (InputEditText is not { } editText)
        {
            return null;
        }

        editText.Focusable = true;
        editText.FocusableInTouchMode = true;
        return editText;
    }

    // Always resets focusability so Android does not resolve focus onto the read-only field, even
    // when a prior RequestFocus() failed (e.g. the dialog window stole focus first).
    internal void ClearInputFocus()
    {
        if (InputEditText is not { } editText)
        {
            return;
        }

        if (editText.IsFocused)
        {
            editText.ClearFocus();
        }

        editText.Focusable = false;
        editText.FocusableInTouchMode = false;
    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        if (MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.AtMost)
        {
            // A Material outlined field has a fixed minimum height (box + label + text). Measure at the
            // intrinsic height and keep it even when the AtMost constraint is smaller, so the value is
            // never clipped; a too-small constraint is exceeded rather than hiding the field's content.
            base.OnMeasure(widthMeasureSpec, MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
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
