using System;

namespace Xamarin.Forms
{
	public abstract class DataTemplateSelector : DataTemplate
	{
		public DataTemplate SelectTemplate(object item, BindableObject container)
		{
			DataTemplate result = OnSelectTemplate(item, container);
			if (result is DataTemplateSelector)
				throw new NotSupportedException("DataTemplateSelector.OnSelectTemplate must not return another DataTemplateSelector");
			return result;
		}

		protected abstract DataTemplate OnSelectTemplate(object item, BindableObject container);
	}
}