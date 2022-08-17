﻿namespace Microsoft.Maui.Controls
{
	public abstract class FlyoutBase : Element, IFlyout
	{
		public static readonly BindableProperty ContextFlyoutProperty = BindableProperty.Create("ContextFlyout", typeof(FlyoutBase), typeof(FlyoutBase), null,
			propertyChanged: (bo, oldV, newV) =>
			{
				if (oldV is BindableObject oldMenu)
					VisualElement.SetInheritedBindingContext(oldMenu, bo.BindingContext);

				if (newV is BindableObject newMenu)
					VisualElement.SetInheritedBindingContext(newMenu, bo.BindingContext);

			});

		public static void SetContextFlyout(BindableObject b, FlyoutBase value)
		{
			b.SetValue(ContextFlyoutProperty, value);
		}

		public static FlyoutBase GetContextFlyout(BindableObject b)
		{
			return (FlyoutBase)b.GetValue(ContextFlyoutProperty);
		}
	}
}
