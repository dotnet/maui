#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="Type[@FullName='Microsoft.Maui.Controls.AutomationProperties']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='GetHelpText']/Docs/*" />
		public static string GetHelpText(BindableObject bindable)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return (string)bindable.GetValue(HelpTextProperty);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='GetIsInAccessibleTree']/Docs/*" />
		public static bool? GetIsInAccessibleTree(BindableObject bindable)
		{
			return (bool?)bindable.GetValue(IsInAccessibleTreeProperty);
		}

		public static bool? GetExcludedWithChildren(BindableObject bindable)
		{
			return (bool?)bindable.GetValue(ExcludedWithChildrenProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='GetLabeledBy']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(ReferenceTypeConverter))]
		public static VisualElement GetLabeledBy(BindableObject bindable)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return (VisualElement)bindable.GetValue(LabeledByProperty);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='GetName']/Docs/*" />
		public static string GetName(BindableObject bindable)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return (string)bindable.GetValue(NameProperty);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='SetHelpText']/Docs/*" />
		public static void SetHelpText(BindableObject bindable, string value)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			bindable.SetValue(HelpTextProperty, value);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='SetIsInAccessibleTree']/Docs/*" />
		public static void SetIsInAccessibleTree(BindableObject bindable, bool? value)
		{
			bindable.SetValue(IsInAccessibleTreeProperty, value);
		}

		public static void SetExcludedWithChildren(BindableObject bindable, bool? value)
		{
			bindable.SetValue(ExcludedWithChildrenProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='SetLabeledBy']/Docs/*" />
		public static void SetLabeledBy(BindableObject bindable, VisualElement value)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			bindable.SetValue(LabeledByProperty, value);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='SetName']/Docs/*" />
		public static void SetName(BindableObject bindable, string value)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			bindable.SetValue(NameProperty, value);
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}
