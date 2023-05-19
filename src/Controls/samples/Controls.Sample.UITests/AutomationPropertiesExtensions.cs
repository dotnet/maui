using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public static class AutomationPropertiesExtensions
	{
		public static void SetAutomationPropertiesName(this Element element, string name)
		{
			element.SetValue(SemanticProperties.DescriptionProperty, name);
		}

		public static string GetAutomationPropertiesName(this Element element)
		{
			return (string)element.GetValue(SemanticProperties.DescriptionProperty);
		}

		public static void SetAutomationPropertiesHelpText(this Element element, string HelpText)
		{
			element.SetValue(SemanticProperties.HintProperty, HelpText);
		}

		public static string GetAutomationPropertiesHelpText(this Element element)
		{
			return (string)element.GetValue(SemanticProperties.HintProperty);
		}

		public static void SetAutomationPropertiesIsInAccessibleTree(this Element element, bool value)
		{
			element.SetValue(AutomationProperties.IsInAccessibleTreeProperty, value);
		}

		public static bool GetAutomationPropertiesIsInAccessibleTree(this Element element)
		{
			return (bool)element.GetValue(AutomationProperties.IsInAccessibleTreeProperty);
		}

		public static void SetAutomationPropertiesLabeledBy(this Element element, Element value)
		{
			element.SetValue(SemanticProperties.DescriptionProperty, value);
		}

		public static Element GetAutomationPropertiesLabeledBy(this Element element)
		{
			return (Element)element.GetValue(SemanticProperties.DescriptionProperty);
		}
	}
}