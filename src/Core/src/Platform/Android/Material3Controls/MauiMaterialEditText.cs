using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
namespace Microsoft.Maui.Platform;

// TODO: Material3: Make it public in .NET 11
internal class MauiMaterialEditText : TextInputEditText
{
	public event EventHandler? SelectionChanged;

	public MauiMaterialEditText(Context context) : base(context)
	{
	}

	protected MauiMaterialEditText(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
	{
	}

	public MauiMaterialEditText(Context context, IAttributeSet? attrs) : base(context, attrs)
	{
	}

	public MauiMaterialEditText(Context context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
	{
	}

	protected override void OnSelectionChanged(int selStart, int selEnd)
	{
		base.OnSelectionChanged(selStart, selEnd);

		SelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
	{
		// Get the measure spec mode and size
		var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
		var heightSize = MeasureSpec.GetSize(heightMeasureSpec);

		if (heightMode == MeasureSpecMode.AtMost && heightSize > 0)
		{
			heightMeasureSpec = MeasureSpec.MakeMeasureSpec(heightSize, MeasureSpecMode.Exactly);
		}

		base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
	}
}

// TODO: Material3: Make it public in .NET 11
internal class MauiMaterialTextInputLayout : TextInputLayout
{
	public MauiMaterialTextInputLayout(Context context) : base(context)
	{
	}

	protected MauiMaterialTextInputLayout(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
	{
	}

	public MauiMaterialTextInputLayout(Context context, IAttributeSet? attrs) : base(context, attrs)
	{
	}

	public MauiMaterialTextInputLayout(Context context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
	{
	}

	protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
	{
		// Get the measure spec mode and size
		var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
		var widthSize = MeasureSpec.GetSize(widthMeasureSpec);

		// If we have an AtMost constraint with a specific size, treat it as Exactly
		// to force the TextInputLayout to expand to the available width
		// This ensures the Material TextInputLayout fills the available space
		// instead of wrapping its content
		if (widthMode == MeasureSpecMode.AtMost && widthSize > 0)
		{
			widthMeasureSpec = MeasureSpec.MakeMeasureSpec(widthSize, MeasureSpecMode.Exactly);
		}

		base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
	}
}