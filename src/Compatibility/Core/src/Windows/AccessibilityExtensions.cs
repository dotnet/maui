using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public static class AccessibilityExtensions
	{
		// TODO: This is not having any effect on anything I've tested yet. See if we need it  
		// after we test the FP and NP w/ back button explicitly enabled. 
		public static void SetBackButtonTitle(this PageControl Control, Element Element)
		{
			if (Element == null)
				return;

			var elemValue = ConcatenateNameAndHint(Element);

			Control.BackButtonTitle = elemValue;
		}

		static string ConcatenateNameAndHint(Element Element)
		{
			string separator;

#pragma warning disable CS0618 // Type or member is obsolete
			var name = (string)Element.GetValue(AutomationProperties.NameProperty);
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
			var hint = (string)Element.GetValue(AutomationProperties.HelpTextProperty);
#pragma warning restore CS0618 // Type or member is obsolete


			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(hint))
				separator = "";

			else
				separator = ". ";


			return string.Join(separator, name, hint);

		}
	}
}
