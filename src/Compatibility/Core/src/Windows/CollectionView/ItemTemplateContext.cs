namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class ItemTemplateContext
	{
		public DataTemplate FormsDataTemplate { get; }
		public object Item { get; }
		public BindableObject Container { get; }
		public double ItemHeight { get; }
		public double ItemWidth { get; }
		public Thickness ItemSpacing { get; }

		public ItemTemplateContext(DataTemplate formsDataTemplate, object item, BindableObject container, 
			double? height = null, double? width = null, Thickness? itemSpacing = null)
		{
			FormsDataTemplate = formsDataTemplate;
			Item = item;
			Container = container;

			if (height.HasValue)
				ItemHeight = height.Value;

			if (width.HasValue)
				ItemWidth = width.Value;

			if (itemSpacing.HasValue)
				ItemSpacing = itemSpacing.Value;
		}
	}
}