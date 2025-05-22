namespace Microsoft.Maui.Controls;

static class ClippedToBoundsElement
{
	public static readonly BindableProperty IsClippedToBoundsProperty =
			BindableProperty.Create("IsClippedToBounds", typeof(bool), typeof(Layout), false,
			propertyChanged: IsClippedToBoundsPropertyChanged);

	static void IsClippedToBoundsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is IView view)
		{
			view.Handler?.UpdateValue(nameof(Maui.ILayout.ClipsToBounds));
		}
	}
}
