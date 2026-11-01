using System;
using System.Runtime.CompilerServices;
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
			// The mapping runs on every change, so keep one pending registration per view (replaced when the label changes) instead of one per run.
			if (label.Handler is null)
			{
				lock (Pending)
				{
					if (Pending.TryGetValue(bindable, out var existing))
					{
						if (ReferenceEquals(existing.Label, label))
							return label;

						existing.Cancel();
					}

					Pending.Remove(bindable);
					Pending.Add(bindable, new PendingRelation(label, view));
				}
			}

			return label;
		}

		/// <summary>The view's one outstanding wait for its label's handler; holds the view weakly so a label does not keep the labelled view alive.</summary>
		sealed class PendingRelation
		{
			readonly WeakReference<IView> view;

			internal PendingRelation(VisualElement label, IView view)
			{
				Label = label;
				this.view = new WeakReference<IView>(view);
				label.HandlerChanged += Connected;
			}

			internal VisualElement Label { get; }

			internal void Cancel() => Label.HandlerChanged -= Connected;

			void Connected(object? sender, EventArgs e)
			{
				if (Label.Handler is null)
					return;

				Cancel();
				if (view.TryGetTarget(out var target))
				{
					if (target is BindableObject bindable)
						lock (Pending)
							Pending.Remove(bindable);

					(target as VisualElement)?.Handler?.UpdateValue(GtkAccessibility.LabeledByKey);
				}
			}
		}

		static readonly ConditionalWeakTable<BindableObject, PendingRelation> Pending = new();
	}
#pragma warning restore CS0618
}
