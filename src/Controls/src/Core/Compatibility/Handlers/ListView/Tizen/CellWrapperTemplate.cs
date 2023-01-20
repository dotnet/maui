#nullable disable
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class CellWrapperTemplateSelector : DataTemplateSelector
	{
		Dictionary<DataTemplate, DataTemplate> _cache = new Dictionary<DataTemplate, DataTemplate>();
		DataTemplateSelector _selector;
		public CellWrapperTemplateSelector(DataTemplateSelector selector)
		{
			_selector = selector;
		}
		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var template = _selector.SelectTemplate(item, container);
			if (_cache.ContainsKey(template))
			{
				return _cache[template];
			}
			var wrapper = new CellWrapperTemplate(template, container);
			_cache[template] = wrapper;
			return wrapper;
		}
	}

	public class CellWrapperTemplate : DataTemplate
	{
		BindableObject _container;
		DataTemplate _sourceTemplate;

		public CellWrapperTemplate(DataTemplate source, BindableObject container = null)
		{
			_container = container;
			_sourceTemplate = source;

			LoadTemplate = () => CellContentFactory.CreateContent(_sourceTemplate.CreateContent(), _container);
		}
	}
}
