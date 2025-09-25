#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Contains both abbreviated and detailed UI information that is supplied to accessibility services.</summary>
	public class AutomationProperties
	{
		/// <summary>Bindable property for <c>HelpText</c>.</summary>
		[Obsolete("Use SemanticProperties.Hint instead. See the conceptual docs about accessibility for more information.")]
		public static readonly BindableProperty HelpTextProperty = BindableProperty.Create("HelpText", typeof(string), typeof(AutomationProperties), default(string));

		/// <summary>Bindable property for <c>IsInAccessibleTree</c>.</summary>
		public static readonly BindableProperty IsInAccessibleTreeProperty = BindableProperty.Create("IsInAccessibleTree", typeof(bool?), typeof(AutomationProperties), null);

		/// <summary>Bindable property for <c>ExcludedWithChildren</c>.</summary>
		public static readonly BindableProperty ExcludedWithChildrenProperty = BindableProperty.Create("ExcludedWithChildren", typeof(bool?), typeof(AutomationProperties), null);

		/// <summary>Bindable property for <c>LabeledBy</c>.</summary>
		[Obsolete("Use a SemanticProperties.Description binding instead. See the conceptual docs about accessibility for more information.")]
		public static readonly BindableProperty LabeledByProperty = BindableProperty.Create("LabeledBy", typeof(VisualElement), typeof(AutomationProperties), default(VisualElement));

		/// <summary>Bindable property for <c>Name</c>.</summary>
		[Obsolete("Use SemanticProperties.Description instead. See the conceptual docs about accessibility for more information.")]
		public static readonly BindableProperty NameProperty = BindableProperty.Create("Name", typeof(string), typeof(AutomationProperties), default(string));

		/// <summary>Returns the help text, if any, for the bindable object.</summary>
		/// <param name="bindable">The bindable object whose help text to get.</param>
		public static string GetHelpText(BindableObject bindable)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return (string)bindable.GetValue(HelpTextProperty);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <summary>Gets a nullable Boolean value that tells whether the bindable object is available to the accessibility system.</summary>
		/// <param name="bindable">The bindable object whose status to check.</param>
		/// <returns><see langword="true"/> if <paramref name="bindable"/> is available to the accessibility system. <see langword="false"/> or <see langword="null"/> if it is not.</returns>
		public static bool? GetIsInAccessibleTree(BindableObject bindable)
		{
			return (bool?)bindable.GetValue(IsInAccessibleTreeProperty);
		}

		public static bool? GetExcludedWithChildren(BindableObject bindable)
		{
			return (bool?)bindable.GetValue(ExcludedWithChildrenProperty);
		}

		/// <summary>Returns the element that labels <paramref name="bindable"/>, if <paramref name="bindable"/> does not label itself and if another element describes it in the UI.</summary>
		/// <param name="bindable">The object whose label to find.</param>
		/// <returns>The element that labels <paramref name="bindable"/>, if present.</returns>
		[System.ComponentModel.TypeConverter(typeof(ReferenceTypeConverter))]
		public static VisualElement GetLabeledBy(BindableObject bindable)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return (VisualElement)bindable.GetValue(LabeledByProperty);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <summary>Returns the short, developer-specified, introductory name of the element, such as "Progress Indicator" or "Button".</summary>
		/// <param name="bindable">The object whose name to get.</param>
		/// <returns>The short, introdctory name of the element.</returns>
		public static string GetName(BindableObject bindable)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return (string)bindable.GetValue(NameProperty);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <summary>Sets the help text for <paramref name="bindable"/>.</summary>
		/// <param name="bindable">The object whose help text to set.</param>
		/// <param name="value">The new help text value.</param>
		public static void SetHelpText(BindableObject bindable, string value)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			bindable.SetValue(HelpTextProperty, value);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <summary>Sets a Boolean value that tells whether the bindable object is available to the accessibility system.</summary>
		/// <param name="bindable">The object ot add or remove from the accessibility system.</param>
		/// <param name="value"><see langword="true"/> to make <paramref name="bindable"/> visible to the accessibility system. <see langword="false"/> to remove it from the system.</param>
		public static void SetIsInAccessibleTree(BindableObject bindable, bool? value)
		{
			bindable.SetValue(IsInAccessibleTreeProperty, value);
		}

		public static void SetExcludedWithChildren(BindableObject bindable, bool? value)
		{
			bindable.SetValue(ExcludedWithChildrenProperty, value);
		}

		/// <summary>Sets another element, such as a <see cref="Microsoft.Maui.Controls.Label"/> as the label for <paramref name="bindable"/>.</summary>
		/// <param name="bindable">The object whose label to set.</param>
		/// <param name="value">The visual element that will name <paramref name="bindable"/>, or <see langword="null"/> to make <paramref name="bindable"/> its own label.</param>
		public static void SetLabeledBy(BindableObject bindable, VisualElement value)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			bindable.SetValue(LabeledByProperty, value);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <summary>Sets the short, developer-specified, introductory name of the element, such as "Progress Indicator" or "Button".</summary>
		/// <param name="bindable">The object whose name to set.</param>
		/// <param name="value">The new name.</param>
		public static void SetName(BindableObject bindable, string value)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			bindable.SetValue(NameProperty, value);
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}
