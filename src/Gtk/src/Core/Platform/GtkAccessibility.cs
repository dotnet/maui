using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// A platform view whose accessible label, description and relations belong to an inner widget (the focusable control of a
	/// composite) instead of the container the handler owns.
	/// </summary>
	internal interface IGtkAccessibleHost
	{
		/// <summary>The widget that carries the accessible label, description and labelled-by relation of the MAUI view.</summary>
		global::Gtk.Widget AccessibleTarget { get; }
	}

	/// <summary>
	/// Supplies the Controls-level automation values (<c>AutomationProperties</c>) that <see cref="IView"/> does not expose. The Controls
	/// assembly registers the implementation; without it those mappings are no-ops.
	/// </summary>
	internal interface IGtkAutomationSource
	{
		/// <summary><c>AutomationProperties.IsInAccessibleTree</c>, or <see langword="null"/> when unset.</summary>
		bool? GetIsInAccessibleTree(IView view);

		/// <summary><c>AutomationProperties.ExcludedWithChildren</c>, or <see langword="null"/> when unset.</summary>
		bool? GetExcludedWithChildren(IView view);

		/// <summary><c>AutomationProperties.LabeledBy</c>, or <see langword="null"/>. Re-applies the relation once a not yet created target gets its handler.</summary>
		IView? GetLabeledBy(IView view);
	}

	/// <summary>
	/// Maps the MAUI semantics to the GTK 4 accessibility API (<c>GtkAccessible</c>): properties (label, description, level),
	/// states (hidden), relations (labelled-by) and roles. What GTK keeps up to date by itself (disabled from sensitivity, checked of
	/// toggles, read-only of editables) is left to GTK.
	/// </summary>
	internal static class GtkAccessibility
	{
		sealed class State
		{
			public global::Gtk.AccessibleRole DefaultRole;
			public bool HiddenByUs;
			public bool LabelSet;
			public bool DescriptionSet;
			public bool LevelSet;
		}

		/// <summary>The property name <c>AutomationProperties.IsInAccessibleTree</c> raises on the handler.</summary>
		internal const string IsInAccessibleTreeKey = "IsInAccessibleTree";

		/// <summary>The property name <c>AutomationProperties.ExcludedWithChildren</c> raises on the handler.</summary>
		internal const string ExcludedWithChildrenKey = "ExcludedWithChildren";

		/// <summary>The property name <c>AutomationProperties.LabeledBy</c> raises on the handler.</summary>
		internal const string LabeledByKey = "LabeledBy";

		/// <summary>The command <c>SemanticExtensions.SetSemanticFocus</c> invokes on the handler.</summary>
		internal const string SetSemanticFocusCommand = "SetSemanticFocus";

		static readonly ConditionalWeakTable<global::Gtk.Widget, State> s_states = new();

		/// <summary>The Controls-level automation values; set by the Controls registration.</summary>
		internal static IGtkAutomationSource? AutomationSource { get; set; }

		/// <summary>The widget that carries label, description and relations of a handler's platform view.</summary>
		internal static global::Gtk.Widget Target(global::Gtk.Widget platformView) =>
			(platformView as IGtkAccessibleHost)?.AccessibleTarget ?? platformView;

		static State StateOf(global::Gtk.Widget widget)
		{
			if (!s_states.TryGetValue(widget, out var state))
			{
				state = new State { DefaultRole = widget.AccessibleRole };
				s_states.Add(widget, state);
			}

			return state;
		}

		/// <summary>Sets (or, for <see langword="null"/> or empty, removes) a string accessible property.</summary>
		internal static void SetProperty(global::Gtk.IAccessible accessible, global::Gtk.AccessibleProperty property, string? value)
		{
			if (string.IsNullOrEmpty(value))
			{
				accessible.ResetProperty(property);
				return;
			}

			Update(accessible, property, value);
		}

		/// <summary>Sets an integer accessible property (<c>Level</c>).</summary>
		internal static void SetProperty(global::Gtk.IAccessible accessible, global::Gtk.AccessibleProperty property, int value) =>
			Update(accessible, property, value);

		/// <summary>Sets a floating point accessible property (<c>ValueNow</c>, <c>ValueMin</c>, <c>ValueMax</c>).</summary>
		internal static void SetProperty(global::Gtk.IAccessible accessible, global::Gtk.AccessibleProperty property, double value) =>
			Update(accessible, property, value);

		static void Update(global::Gtk.IAccessible accessible, global::Gtk.AccessibleProperty property, object value)
		{
			var gvalue = global::Gtk.AccessibleExtensions.InitValue(property);
			try
			{
				gvalue.Val = value;
				global::Gtk.AccessibleExtensions.UpdateProperty(accessible, property, gvalue);
			}
			finally
			{
				gvalue.Dispose();
			}
		}

		/// <summary>Sets a boolean accessible state (<c>Hidden</c>, <c>Disabled</c>, ...).</summary>
		internal static void SetState(global::Gtk.IAccessible accessible, global::Gtk.AccessibleState state, bool value)
		{
			var gvalue = global::Gtk.AccessibleExtensions.InitValue(state);
			try
			{
				// "Undefined-able" booleans (selected, expanded) are integers in GTK.
				gvalue.Val = gvalue.Val is int ? (object)(value ? 1 : 0) : value;
				global::Gtk.AccessibleExtensions.UpdateState(accessible, state, gvalue);
			}
			finally
			{
				gvalue.Dispose();
			}
		}

		/// <summary>
		/// Sets the accessible role. GTK fixes the role when the widget joins a window (its accessibility context is realized), so this works until the
		/// widget is rooted (while a handler is being created and connected) and is ignored afterwards.
		/// </summary>
		/// <returns><see langword="true"/> when the widget has the role afterwards.</returns>
		internal static bool TrySetRole(global::Gtk.Widget widget, global::Gtk.AccessibleRole role)
		{
			if (widget.AccessibleRole == role)
				return true;

			if (widget.Root is not null)
				return false;

			StateOf(widget);
			widget.AccessibleRole = role;
			return widget.AccessibleRole == role;
		}

		static void Sync(global::Gtk.IAccessible accessible, global::Gtk.AccessibleProperty property, string? value, ref bool isSet)
		{
			if (!string.IsNullOrEmpty(value))
			{
				SetProperty(accessible, property, value);
				isSet = true;
			}
			else if (isSet)
			{
				isSet = false;
				accessible.ResetProperty(property);
			}
		}

		/// <summary>Label (<c>SemanticProperties.Description</c>), description (<c>Hint</c>) and heading (<c>HeadingLevel</c>).</summary>
		internal static void UpdateSemantics(global::Gtk.Widget platformView, IView view)
		{
			var semantics = view.Semantics;
			var widget = Target(platformView);

			var state = StateOf(widget);

			// Only what this mapping set is removed again: GTK fills in some properties itself (a label from its text).
			Sync(widget, global::Gtk.AccessibleProperty.Label, semantics?.Description, ref state.LabelSet);
			Sync(widget, global::Gtk.AccessibleProperty.Description, semantics?.Hint, ref state.DescriptionSet);

			int level = semantics is null ? 0 : (int)semantics.HeadingLevel;

			if (level > 0)
			{
				TrySetRole(widget, global::Gtk.AccessibleRole.Heading);
				if (widget.AccessibleRole == global::Gtk.AccessibleRole.Heading)
				{
					SetProperty(widget, global::Gtk.AccessibleProperty.Level, level);
					state.LevelSet = true;
				}
			}
			else
			{
				if (widget.AccessibleRole == global::Gtk.AccessibleRole.Heading && state.DefaultRole != global::Gtk.AccessibleRole.Heading)
					TrySetRole(widget, state.DefaultRole);

				if (state.LevelSet)
				{
					state.LevelSet = false;
					widget.ResetProperty(global::Gtk.AccessibleProperty.Level);
				}
			}
		}

		/// <summary>
		/// <c>AutomationProperties.IsInAccessibleTree</c> / <c>ExcludedWithChildren</c>: the hidden state, which (as ARIA's <c>aria-hidden</c>)
		/// removes the widget together with its descendants from the accessibility tree.
		/// </summary>
		internal static void UpdateHidden(global::Gtk.Widget platformView, IView view)
		{
			var source = AutomationSource;
			bool? inTree = source?.GetIsInAccessibleTree(view);
			bool? excluded = source?.GetExcludedWithChildren(view);

			bool hidden = inTree == false || excluded == true;
			var state = StateOf(platformView);

			if (hidden)
			{
				state.HiddenByUs = true;
				SetState(platformView, global::Gtk.AccessibleState.Hidden, true);
			}
			else if (state.HiddenByUs)
			{
				state.HiddenByUs = false;
				// Back to what GTK itself reports: invisible widgets are hidden.
				SetState(platformView, global::Gtk.AccessibleState.Hidden, !platformView.Visible);
			}
		}

		/// <summary><c>AutomationProperties.LabeledBy</c>: the labelled-by relation.</summary>
		internal static void UpdateLabeledBy(global::Gtk.Widget platformView, IView view)
		{
			var widget = Target(platformView);
			var label = AutomationSource?.GetLabeledBy(view);

			if (label?.Handler?.PlatformView is global::Gtk.Widget labelWidget && !ReferenceEquals(labelWidget, widget))
				global::Gtk.AccessibleExtensions.UpdateRelation(widget, global::Gtk.AccessibleRelation.LabelledBy, new global::Gtk.IAccessible[] { Target(labelWidget) });
			else
				widget.ResetRelation(global::Gtk.AccessibleRelation.LabelledBy);
		}

		/// <summary>
		/// <c>SetSemanticFocus</c>: moves the keyboard focus to the widget; a widget that cannot take it (a label) announces its label instead (on the widget: GTK 4.22's Windows AccessKit backend crashes on announcements of a toplevel window).
		/// </summary>
		internal static void SetSemanticFocus(global::Gtk.Widget platformView)
		{
			var widget = Target(platformView);
			if (widget.GrabFocus())
				return;

			string? text = widget.Accessible.Name;
			if (string.IsNullOrEmpty(text) && widget is global::Gtk.Label label)
				text = label.LabelProp;

			if (!string.IsNullOrEmpty(text))
				widget.Announce(text, global::Gtk.AccessibleAnnouncementPriority.Medium);
		}
	}
}
