namespace Microsoft.Maui.ManualTests.Controls
{
	public class SearchTermDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate DefaultTemplate { get; set; }
		public DataTemplate OtherTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			string query = (string)item;
			return string.Equals(query, "xamarin", StringComparison.OrdinalIgnoreCase) ? OtherTemplate : DefaultTemplate;
		}
	}
}
