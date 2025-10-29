#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplateSelector.xml" path="Type[@FullName='Microsoft.Maui.Controls.DataTemplateSelector']/Docs/*" />
	public abstract class DataTemplateSelector : DataTemplate
	{
		Dictionary<Type, DataTemplate> _dataTemplates = new Dictionary<Type, DataTemplate>();

		/// <include file="../../docs/Microsoft.Maui.Controls/DataTemplateSelector.xml" path="//Member[@MemberName='SelectTemplate']/Docs/*" />
		public DataTemplate SelectTemplate(object item, BindableObject container)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var listView = container as ListView;
#pragma warning restore CS0618 // Type or member is obsolete

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