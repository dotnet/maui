namespace Xamarin.Forms.Platform.UWP
{
	internal class ItemTemplateContext
	{
		public ItemTemplateContext(DataTemplate formsDataTemplate, object item, BindableObject container)
		{
			FormsDataTemplate = formsDataTemplate;
			Item = item;
			Container = container;
		}

		public DataTemplate FormsDataTemplate { get; }
		public object Item { get; }
		public BindableObject Container { get; }
	}
}