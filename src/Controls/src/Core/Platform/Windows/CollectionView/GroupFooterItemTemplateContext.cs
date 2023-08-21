// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Platform
{
	internal class GroupFooterItemTemplateContext : ItemTemplateContext
	{
		public GroupFooterItemTemplateContext(DataTemplate formsDataTemplate, object item,
			BindableObject container, double? height = null, double? width = null, Thickness? itemSpacing = null, IMauiContext mauiContext = null)
			: base(formsDataTemplate, item, container, height, width, itemSpacing, mauiContext)
		{
		}

		public static void EnsureSelectionDisabled(DependencyObject element, object item)
		{
			if (item is GroupFooterItemTemplateContext)
			{
				// Prevent the group footer from being selectable
				(element as FrameworkElement).IsHitTestVisible = false;
			}

		}
	}
}