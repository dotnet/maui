using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls;

internal class VisualElementRemappingHandlerAttribute : ElementHandlerAttribute
{
	public VisualElementRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType)
		: base(handlerType)
	{
	}

	protected internal override void RemapForControls()
	{
		VisualElement.RemapForControls();
	}
}

internal class ElementRemappingHandlerAttribute : ElementHandlerAttribute
{
	public ElementRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType)
		: base(handlerType)
	{
	}

	protected internal override void RemapForControls()
	{
		Element.RemapForControls();
	}
}

internal sealed class ApplicationRemappingHandlerAttribute : ElementHandlerAttribute
{
	public ApplicationRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Application.RemapForControls();
}

internal sealed class ButtonRemappingHandlerAttribute : ElementHandlerAttribute
{
	public ButtonRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Button.RemapForControls();
}

internal sealed class CheckBoxRemappingHandlerAttribute : ElementHandlerAttribute
{
	public CheckBoxRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => CheckBox.RemapForControls();
}

internal sealed class ContentPageRemappingHandlerAttribute : ElementHandlerAttribute
{
	public ContentPageRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => ContentPage.RemapForControls();
}

internal sealed class DatePickerRemappingHandlerAttribute : ElementHandlerAttribute
{
	public DatePickerRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => DatePicker.RemapForControls();
}

internal sealed class EditorRemappingHandlerAttribute : ElementHandlerAttribute
{
	public EditorRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Editor.RemapForControls();
}

internal sealed class EntryRemappingHandlerAttribute : ElementHandlerAttribute
{
	public EntryRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Entry.RemapForControls();
}

internal sealed class FlyoutPageRemappingHandlerAttribute : ElementHandlerAttribute
{
	public FlyoutPageRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => FlyoutPage.RemapForControls();
}

internal sealed class ImageButtonRemappingHandlerAttribute : ElementHandlerAttribute
{
	public ImageButtonRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => ImageButton.RemapForControls();
}

internal sealed class LabelRemappingHandlerAttribute : ElementHandlerAttribute
{
	public LabelRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Label.RemapForControls();
}

internal sealed class NavigationPageRemappingHandlerAttribute : ElementHandlerAttribute
{
	public NavigationPageRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => NavigationPage.RemapForControls();
}

internal sealed class PickerRemappingHandlerAttribute : ElementHandlerAttribute
{
	public PickerRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Picker.RemapForControls();
}

internal sealed class RadioButtonRemappingHandlerAttribute : ElementHandlerAttribute
{
	public RadioButtonRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => RadioButton.RemapForControls();
}

internal sealed class RefreshViewRemappingHandlerAttribute : ElementHandlerAttribute
{
	public RefreshViewRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => RefreshView.RemapForControls();
}

internal sealed class ScrollViewRemappingHandlerAttribute : ElementHandlerAttribute
{
	public ScrollViewRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => ScrollView.RemapForControls();
}

internal sealed class SearchBarRemappingHandlerAttribute : ElementHandlerAttribute
{
	public SearchBarRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => SearchBar.RemapForControls();
}

internal sealed class ShapeRemappingHandlerAttribute : ElementHandlerAttribute
{
	public ShapeRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Shapes.Shape.RemapForControls();
}

internal sealed class SliderRemappingHandlerAttribute : ElementHandlerAttribute
{
	public SliderRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Slider.RemapForControls();
}

internal sealed class StepperRemappingHandlerAttribute : ElementHandlerAttribute
{
	public StepperRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Stepper.RemapForControls();
}

internal sealed class SwipeViewRemappingHandlerAttribute : ElementHandlerAttribute
{
	public SwipeViewRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => SwipeView.RemapForControls();
}

internal sealed class TabbedPageRemappingHandlerAttribute : ElementHandlerAttribute
{
	public TabbedPageRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => TabbedPage.RemapForControls();
}

internal sealed class TimePickerRemappingHandlerAttribute : ElementHandlerAttribute
{
	public TimePickerRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => TimePicker.RemapForControls();
}

internal sealed class ToolbarRemappingHandlerAttribute : ElementHandlerAttribute
{
	public ToolbarRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Toolbar.RemapForControls();
}

internal sealed class WebViewRemappingHandlerAttribute : ElementHandlerAttribute
{
	public WebViewRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => WebView.RemapForControls();
}

internal sealed class WindowRemappingHandlerAttribute : ElementHandlerAttribute
{
	public WindowRemappingHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType) : base(handlerType) { }
	protected internal override void RemapForControls() => Window.RemapForControls();
}
