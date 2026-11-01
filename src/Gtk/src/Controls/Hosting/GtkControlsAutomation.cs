using System;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Controls.Gtk.Hosting
{
	/// <summary>
	/// The Controls values the core GTK accessibility mapping cannot see: <see cref="AutomationProperties.IsInAccessibleTreeProperty"/>,
	/// <see cref="AutomationProperties.ExcludedWithChildrenProperty"/> and <see cref="AutomationProperties.LabeledByProperty"/>.
	/// </summary>
#pragma warning disable CS0618 // AutomationProperties is obsolete but still supported.
	sealed class GtkControlsAutomation : IGtkAutomationSource
	{
		internal static readonly GtkControlsAutomation Instance = new();

		public bool? GetIsInAccessibleTree(IView view) =>
			view is BindableObject bindable ? AutomationProperties.GetIsInAccessibleTree(bindable) : null;

		public bool? GetExcludedWithChildren(IView view) =>
			view is BindableObject bindable ? AutomationProperties.GetExcludedWithChildren(bindable) : null;

		public IView? GetLabeledBy(IView view)
		{
			if (view is not BindableObject bindable || AutomationProperties.GetLabeledBy(bindable) is not VisualElement label)
				return null;

			// A label that is created after the view it labels (declaration order in a page): relate them once its handler exists.
			if (label.Handler is null)
			{
				void Connected(object? sender, EventArgs e)
				{
					if (label.Handler is null)
						return;

					label.HandlerChanged -= Connected;
					(view as VisualElement)?.Handler?.UpdateValue(GtkAccessibility.LabeledByKey);
				}

				label.HandlerChanged += Connected;
			}

			return label;
		}
	}
#pragma warning restore CS0618
}
