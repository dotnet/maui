namespace Microsoft.Maui.Controls;

static class InputTransparentContainerElement
{
	/// <summary>Bindable property for <see cref="IInputTransparentContainerElement.CascadeInputTransparent"/>.</summary>
	public static readonly BindableProperty CascadeInputTransparentProperty =
		BindableProperty.Create("CascadeInputTransparent", typeof(bool), typeof(Layout), true,
			propertyChanged: OnCascadeInputTransparentPropertyChanged);

	static void OnCascadeInputTransparentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		// We only need to update if the cascade changes anything, namely when InputTransparent=true.
		// When InputTransparent=false, then the cascade property has no effect.
		if (bindable is Layout layout && layout.InputTransparent)
		{
			(layout as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.InputTransparentProperty.PropertyName);
		}
	}
}
