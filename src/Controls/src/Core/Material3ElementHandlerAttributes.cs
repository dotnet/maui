using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls;

// Keep these manual attributes small; https://github.com/dotnet/maui/issues/35285 tracks source-generating them.
#if ANDROID
sealed class ActivityIndicatorHandlerAttribute() : ElementHandlerAttribute(typeof(ActivityIndicatorHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(ActivityIndicatorHandler2);

		return typeof(ActivityIndicatorHandler);
	}
}

sealed class DatePickerHandlerAttribute() : ElementHandlerAttribute(typeof(DatePickerHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(DatePickerHandler2);

		return typeof(DatePickerHandler);
	}
}

sealed class EditorHandlerAttribute() : ElementHandlerAttribute(typeof(EditorHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(EditorHandler2);

		return typeof(EditorHandler);
	}
}

sealed class EntryHandlerAttribute() : ElementHandlerAttribute(typeof(EntryHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(EntryHandler2);

		return typeof(EntryHandler);
	}
}

sealed class ImageHandlerAttribute() : ElementHandlerAttribute(typeof(ImageHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(ImageHandler2);

		return typeof(ImageHandler);
	}
}

sealed class LabelHandlerAttribute() : ElementHandlerAttribute(typeof(LabelHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(LabelHandler2);

		return typeof(LabelHandler);
	}
}

sealed class PickerHandlerAttribute() : ElementHandlerAttribute(typeof(PickerHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(PickerHandler2);

		return typeof(PickerHandler);
	}
}

sealed class ProgressBarHandlerAttribute() : ElementHandlerAttribute(typeof(ProgressBarHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(ProgressBarHandler2);

		return typeof(ProgressBarHandler);
	}
}

sealed class RadioButtonHandlerAttribute() : ElementHandlerAttribute(typeof(RadioButtonHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(RadioButtonHandler2);

		return typeof(RadioButtonHandler);
	}
}

sealed class SearchBarHandlerAttribute() : ElementHandlerAttribute(typeof(SearchBarHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(SearchBarHandler2);

		return typeof(SearchBarHandler);
	}
}

sealed class SliderHandlerAttribute() : ElementHandlerAttribute(typeof(SliderHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(SliderHandler2);

		return typeof(SliderHandler);
	}
}

sealed class SwitchHandlerAttribute() : ElementHandlerAttribute(typeof(SwitchHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(SwitchHandler2);

		return typeof(SwitchHandler);
	}
}

sealed class TimePickerHandlerAttribute() : ElementHandlerAttribute(typeof(TimePickerHandler))
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
	{
		if (RuntimeFeature.IsMaterial3Enabled)
			return typeof(TimePickerHandler2);

		return typeof(TimePickerHandler);
	}
}
#else
sealed class ActivityIndicatorHandlerAttribute() : ElementHandlerAttribute(typeof(ActivityIndicatorHandler));
sealed class DatePickerHandlerAttribute() : ElementHandlerAttribute(typeof(DatePickerHandler));
sealed class EditorHandlerAttribute() : ElementHandlerAttribute(typeof(EditorHandler));
sealed class EntryHandlerAttribute() : ElementHandlerAttribute(typeof(EntryHandler));
sealed class ImageHandlerAttribute() : ElementHandlerAttribute(typeof(ImageHandler));
sealed class LabelHandlerAttribute() : ElementHandlerAttribute(typeof(LabelHandler));
sealed class PickerHandlerAttribute() : ElementHandlerAttribute(typeof(PickerHandler));
sealed class ProgressBarHandlerAttribute() : ElementHandlerAttribute(typeof(ProgressBarHandler));
sealed class RadioButtonHandlerAttribute() : ElementHandlerAttribute(typeof(RadioButtonHandler));
sealed class SearchBarHandlerAttribute() : ElementHandlerAttribute(typeof(SearchBarHandler));
sealed class SliderHandlerAttribute() : ElementHandlerAttribute(typeof(SliderHandler));
sealed class SwitchHandlerAttribute() : ElementHandlerAttribute(typeof(SwitchHandler));
sealed class TimePickerHandlerAttribute() : ElementHandlerAttribute(typeof(TimePickerHandler));
#endif
