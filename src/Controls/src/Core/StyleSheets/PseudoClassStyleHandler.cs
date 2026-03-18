#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.StyleSheets
{
	/// <summary>
	/// Handles CSS pseudo-class selectors (:hover, :focus, :disabled) by mapping them to VisualStateManager states.
	/// </summary>
	internal static class PseudoClassStyleHandler
	{
		private const string CommonStatesGroupName = "CommonStates";

		/// <summary>
		/// Checks if a selector contains a pseudo-class selector (Hover, Focused, or Disabled).
		/// Returns the pseudo-class type and the remaining selector without the pseudo-class.
		/// </summary>
		internal static bool TryExtractPseudoClass(Selector selector, out PseudoClass pseudoClass, out Selector remaining)
		{
			pseudoClass = PseudoClass.None;
			remaining = selector;

			if (selector == null || selector == Selector.Invalid)
				return false;

			// Check if selector is an And expression
			if (selector is Selector.And andSel)
			{
				// Check if either side is a pseudo-class
				if (TryGetPseudoClassType(andSel.Right, out var pc))
				{
					pseudoClass = pc;
					remaining = andSel.Left; // Left side is the remaining selector
					return true;
				}

				if (TryGetPseudoClassType(andSel.Left, out pc))
				{
					pseudoClass = pc;
					remaining = andSel.Right;
					return true;
				}
			}
			else if (TryGetPseudoClassType(selector, out var pc))
			{
				pseudoClass = pc;
				remaining = Selector.Universal;
				return true;
			}

			return false;
		}

		private static bool TryGetPseudoClassType(Selector selector, out PseudoClass pseudoClass)
		{
			pseudoClass = PseudoClass.None;

			if (selector == null || selector == Selector.Invalid)
				return false;

			var typeName = selector.GetType().Name;

			if (string.Equals(typeName, "Hover", StringComparison.Ordinal))
			{
				pseudoClass = PseudoClass.Hover;
				return true;
			}
			else if (string.Equals(typeName, "Focused", StringComparison.Ordinal))
			{
				pseudoClass = PseudoClass.Focused;
				return true;
			}
			else if (string.Equals(typeName, "Disabled", StringComparison.Ordinal))
			{
				pseudoClass = PseudoClass.Disabled;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Applies pseudo-class styles to an element by creating VisualStateManager states.
		/// </summary>
		internal static void ApplyPseudoClassStyle(VisualElement element, PseudoClass pseudoClass, Style style, 
			Selector.SelectorSpecificity specificity, IDictionary<string, string> variables)
		{
			if (element == null || pseudoClass == PseudoClass.None)
				return;

			var vsm = VisualStateManager.GetVisualStateGroups(element);
			if (vsm == null)
			{
				vsm = new VisualStateManager.VisualStateGroupList { IsDefault = false };
				VisualStateManager.SetVisualStateGroups(element, vsm);
			}

			// Find or create the CommonStates group
			var commonStatesGroup = vsm.FirstOrDefault(g => string.Equals(g.Name, CommonStatesGroupName, StringComparison.Ordinal));
			if (commonStatesGroup == null)
			{
				commonStatesGroup = new VisualStateManager.VisualStateGroup { Name = CommonStatesGroupName };
				vsm.Add(commonStatesGroup);
			}

			// Map pseudo-class to VSM state name
			var stateName = GetVsmStateName(pseudoClass);
			var visualState = commonStatesGroup.States.FirstOrDefault(s => string.Equals(s.Name, stateName, StringComparison.Ordinal));
			if (visualState == null)
			{
				visualState = new VisualStateManager.VisualState { Name = stateName, TargetType = element.GetType() };
				commonStatesGroup.States.Add(visualState);
			}

			// Add setters from the style to the visual state
			foreach (var decl in style.Declarations)
			{
				var property = ((IStylable)element).GetProperty(decl.Key, false);
				if (property == null)
					continue;

				var resolvedValue = CssVariableResolver.Resolve(decl.Value, variables);
				if (string.IsNullOrEmpty(resolvedValue))
					continue;

				resolvedValue = CssValueResolver.ResolveUnits(resolvedValue);

				object value;
				var resolvedDecl = new KeyValuePair<string, string>(decl.Key, resolvedValue);
				
				// Try to convert the value
				try
				{
					if (!Style.ConvertedValues.TryGetValue(resolvedDecl, out value))
						value = Style.Convert(element, resolvedValue, property);
					Style.ConvertedValues[resolvedDecl] = value;
				}
				catch
				{
					// If conversion fails, skip this declaration
					continue;
				}

				// Create a setter for this property
				var setter = new Setter { Property = property, Value = value };
				visualState.Setters.Add(setter);
			}
		}

		private static string GetVsmStateName(PseudoClass pseudoClass)
		{
			return pseudoClass switch
			{
				PseudoClass.Hover => VisualStateManager.CommonStates.PointerOver,
				PseudoClass.Focused => VisualStateManager.CommonStates.Focused,
				PseudoClass.Disabled => VisualStateManager.CommonStates.Disabled,
				_ => VisualStateManager.CommonStates.Normal
			};
		}
	}

	/// <summary>
	/// Enum for pseudo-class types.
	/// </summary>
	internal enum PseudoClass
	{
		None,
		Hover,
		Focused,
		Disabled
	}
}
