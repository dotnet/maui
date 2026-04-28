#nullable disable
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Manages visual states for controls (Normal, Focused, Disabled, etc.) and transitions between them.
	/// </summary>
	public static class VisualStateManager
	{
		public class CommonStates
		{
			public const string Normal = "Normal";
			public const string Disabled = "Disabled";
			public const string Focused = "Focused";
			public const string Selected = "Selected";
			public const string PointerOver = "PointerOver";
			internal const string Unfocused = "Unfocused";
		}

		/// <summary>Bindable property for attached property <c>VisualStateGroups</c>.</summary>
		public static readonly BindableProperty VisualStateGroupsProperty =
			BindableProperty.CreateAttached("VisualStateGroups", typeof(VisualStateGroupList), typeof(VisualElement),
				defaultValue: null, propertyChanged: VisualStateGroupsPropertyChanged,
				defaultValueCreator: bindable => new VisualStateGroupList(true) { VisualElement = (VisualElement)bindable });

		static void VisualStateGroupsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is VisualStateGroupList { VisualElement: { } oldElement } oldVisualStateGroupList)
			{
				var vsgSpecificity = oldVisualStateGroupList.Specificity;
				var specificity = vsgSpecificity.CopyStyle(1, 0, 0, 0);

				foreach (var group in oldVisualStateGroupList)
				{
					if (group.CurrentState is { } state)
						foreach (var setter in state.Setters)
							setter.UnApply(oldElement, specificity);
				}
				oldVisualStateGroupList.VisualElement = null;
			}

			var visualElement = (VisualElement)bindable;

			if (newValue != null)
				((VisualStateGroupList)newValue).VisualElement = visualElement;

			visualElement.ChangeVisualState();

			UpdateStateTriggers(visualElement);
		}

		/// <summary>
		/// Gets the collection of <see cref="VisualStateGroup"/> objects associated with the specified <paramref name="visualElement"/>.
		/// </summary>
		/// <param name="visualElement">The visual element to get visual state groups from.</param>
		/// <returns>The collection of visual state groups.</returns>
		public static IList<VisualStateGroup> GetVisualStateGroups(VisualElement visualElement)
			=> (IList<VisualStateGroup>)visualElement.GetValue(VisualStateGroupsProperty);

		/// <summary>
		/// Sets the collection of <see cref="VisualStateGroup"/> objects for the specified <paramref name="visualElement"/>.
		/// </summary>
		/// <param name="visualElement">The visual element to set visual state groups on.</param>
		/// <param name="value">The collection of visual state groups to set.</param>
		public static void SetVisualStateGroups(VisualElement visualElement, VisualStateGroupList value)
			=> visualElement.SetValue(VisualStateGroupsProperty, value);

		/// <summary>
		/// Transitions the <paramref name="visualElement"/> to the specified visual state.
		/// </summary>
		/// <param name="visualElement">The visual element to transition.</param>
		/// <param name="name">The name of the visual state to transition to.</param>
		/// <returns><see langword="true"/> if the transition was successful; otherwise, <see langword="false"/>.</returns>
		public static bool GoToState(VisualElement visualElement, string name)
		{
			var context = visualElement.GetContext(VisualStateGroupsProperty);
			if (context is null)
			{
				return false;
			}

			var vsgSpecificityValue = context.Values.GetSpecificityAndValue();
			var groups = (VisualStateGroupList)vsgSpecificityValue.Value;
			if (groups?.IsDefault != false)
			{
				return false;
			}

			var vsgSpecificity = vsgSpecificityValue.Key;
			groups.Specificity = vsgSpecificity;

			var specificity = vsgSpecificity.CopyStyle(1, 0, 0, 0);

			foreach (VisualStateGroup group in groups)
			{
				if (group.CurrentState?.Name == name)
				{
					// We're already in the target state; nothing else to do
					return true;
				}

				// See if this group contains the new state
				var target = group.GetState(name);
				if (target == null)
				{
					continue;
				}

				// If we've got a new state to transition to, unapply the setters from the current state
				if (group.CurrentState != null)
				{
					foreach (Setter setter in group.CurrentState.Setters)
					{
						setter.UnApply(visualElement, specificity);
					}
				}

				// Update the current state
				group.CurrentState = target;

				// Apply the setters from the new state
				foreach (Setter setter in target.Setters)
				{
					setter.Apply(visualElement, specificity);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="element"/> has any visual state groups defined.
		/// </summary>
		/// <param name="element">The visual element to check.</param>
		/// <returns><see langword="true"/> if the element has visual state groups; otherwise, <see langword="false"/>.</returns>
		public static bool HasVisualStateGroups(this VisualElement element)
		{
			if (!element.IsSet(VisualStateGroupsProperty))
				return false;

			if (GetVisualStateGroups(element) is VisualStateGroupList vsgl)
				return !vsgl.IsDefault;

			return true;
		}

		internal static void UpdateStateTriggers(VisualElement visualElement)
		{
			var groups = (IList<VisualStateGroup>)visualElement.GetValue(VisualStateGroupsProperty);

			foreach (VisualStateGroup group in groups)
			{
				group.VisualElement = visualElement;
				group.UpdateStateTriggers();
			}
		}
	}
}
