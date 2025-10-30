using Microsoft.Maui.ManualTests.Models;

namespace Microsoft.Maui.ManualTests.Controls
{
	public class MonkeyDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate AmericanMonkey { get; set; }
		public DataTemplate OtherMonkey { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			return ((Monkey)item).Location.Contains("America", StringComparison.Ordinal) ? AmericanMonkey : OtherMonkey;
		}
	}
}
