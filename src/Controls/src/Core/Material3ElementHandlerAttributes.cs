using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls;

// Keep these manual attributes small; https://github.com/dotnet/maui/issues/35285 tracks source-generating them.
#if ANDROID
abstract class Material3ElementHandlerAttribute : ElementHandlerAttribute
{
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	readonly Type _material3HandlerType;

	protected Material3ElementHandlerAttribute(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type material3HandlerType)
		: base(handlerType)
	{
		_material3HandlerType = material3HandlerType;
	}

	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public override Type GetHandlerType()
		=> RuntimeFeature.IsMaterial3Enabled ? _material3HandlerType : base.GetHandlerType();
}

sealed class ActivityIndicatorHandlerAttribute() : Material3ElementHandlerAttribute(typeof(ActivityIndicatorHandler), typeof(ActivityIndicatorHandler2));
sealed class DatePickerHandlerAttribute() : Material3ElementHandlerAttribute(typeof(DatePickerHandler), typeof(DatePickerHandler2));
sealed class EditorHandlerAttribute() : Material3ElementHandlerAttribute(typeof(EditorHandler), typeof(EditorHandler2));
sealed class EntryHandlerAttribute() : Material3ElementHandlerAttribute(typeof(EntryHandler), typeof(EntryHandler2));
sealed class ImageHandlerAttribute() : Material3ElementHandlerAttribute(typeof(ImageHandler), typeof(ImageHandler2));
sealed class LabelHandlerAttribute() : Material3ElementHandlerAttribute(typeof(LabelHandler), typeof(LabelHandler2));
sealed class PickerHandlerAttribute() : Material3ElementHandlerAttribute(typeof(PickerHandler), typeof(PickerHandler2));
sealed class ProgressBarHandlerAttribute() : Material3ElementHandlerAttribute(typeof(ProgressBarHandler), typeof(ProgressBarHandler2));
sealed class RadioButtonHandlerAttribute() : Material3ElementHandlerAttribute(typeof(RadioButtonHandler), typeof(RadioButtonHandler2));
sealed class SearchBarHandlerAttribute() : Material3ElementHandlerAttribute(typeof(SearchBarHandler), typeof(SearchBarHandler2));
sealed class SliderHandlerAttribute() : Material3ElementHandlerAttribute(typeof(SliderHandler), typeof(SliderHandler2));
sealed class SwitchHandlerAttribute() : Material3ElementHandlerAttribute(typeof(SwitchHandler), typeof(SwitchHandler2));
sealed class TimePickerHandlerAttribute() : Material3ElementHandlerAttribute(typeof(TimePickerHandler), typeof(TimePickerHandler2));
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
