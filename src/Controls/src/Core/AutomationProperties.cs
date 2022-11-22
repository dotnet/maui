namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="Type[@FullName='Microsoft.Maui.Controls.AutomationProperties']/Docs/*" />
	public class AutomationProperties
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='HelpTextProperty']/Docs/*" />
		public static readonly BindableProperty HelpTextProperty = BindableProperty.Create("HelpText", typeof(string), typeof(AutomationProperties), default(string));

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='IsInAccessibleTreeProperty']/Docs/*" />
		public static readonly BindableProperty IsInAccessibleTreeProperty = BindableProperty.Create("IsInAccessibleTree", typeof(bool?), typeof(AutomationProperties), null);

		public static readonly BindableProperty ExcludedWithChildrenProperty = BindableProperty.Create("ExcludedWithChildren", typeof(bool?), typeof(AutomationProperties), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='LabeledByProperty']/Docs/*" />
		public static readonly BindableProperty LabeledByProperty = BindableProperty.Create("LabeledBy", typeof(VisualElement), typeof(AutomationProperties), default(VisualElement));

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='NameProperty']/Docs/*" />
		public static readonly BindableProperty NameProperty = BindableProperty.Create("Name", typeof(string), typeof(AutomationProperties), default(string));

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='GetHelpText']/Docs/*" />
		public static string GetHelpText(BindableObject bindable)
		{
			return (string)bindable.GetValue(HelpTextProperty);
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
			return (VisualElement)bindable.GetValue(LabeledByProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='GetName']/Docs/*" />
		public static string GetName(BindableObject bindable)
		{
			return (string)bindable.GetValue(NameProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='SetHelpText']/Docs/*" />
		public static void SetHelpText(BindableObject bindable, string value)
		{
			bindable.SetValue(HelpTextProperty, value);
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
			bindable.SetValue(LabeledByProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AutomationProperties.xml" path="//Member[@MemberName='SetName']/Docs/*" />
		public static void SetName(BindableObject bindable, string value)
		{
			bindable.SetValue(NameProperty, value);
		}
	}
}
