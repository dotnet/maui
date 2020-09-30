using System;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	public abstract class DataTemplateSelector : DataTemplate
	{
		Dictionary<Type, DataTemplate> _dataTemplates = new Dictionary<Type, DataTemplate>();

		public DataTemplate SelectTemplate(object item, BindableObject container)
		{
			var listView = container as ListView;

			var recycle = listView == null ? false :
				(listView.CachingStrategy & ListViewCachingStrategy.RecycleElementAndDataTemplate) ==
					ListViewCachingStrategy.RecycleElementAndDataTemplate;

			DataTemplate dataTemplate = null;
			if (recycle && _dataTemplates.TryGetValue(item.GetType(), out dataTemplate))
				return dataTemplate;

			dataTemplate = OnSelectTemplate(item, container);
			if (dataTemplate is DataTemplateSelector)
				throw new NotSupportedException(
					"DataTemplateSelector.OnSelectTemplate must not return another DataTemplateSelector");

			if (recycle)
			{
				if (!dataTemplate.CanRecycle)
					throw new NotSupportedException(
						"RecycleElementAndDataTemplate requires DataTemplate activated with ctor taking a type.");

				_dataTemplates[item.GetType()] = dataTemplate;
			}

			return dataTemplate;
		}

		protected abstract DataTemplate OnSelectTemplate(object item, BindableObject container);
	}
}