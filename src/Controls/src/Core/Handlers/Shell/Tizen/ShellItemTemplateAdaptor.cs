#nullable enable

using System;
using System.Collections;
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellItemTemplateAdaptor : ItemTemplateAdaptor
	{
		public ShellItemTemplateAdaptor(Element element, IEnumerable items) : base(element, items, GetTemplate())
		{
		}

		protected override bool IsSelectable => true;

		static DataTemplate GetTemplate()
		{
			return new ShellDataTemplateSelector();
		}
	}

	public class ShellDataTemplateSelector : DataTemplateSelector
	{
		DataTemplate ShellSectionItemTemplate { get; }

		DataTemplate ShellContentItemTemplate { get; }

		public ShellDataTemplateSelector()
		{
			ShellSectionItemTemplate = new DataTemplate(() =>
			{
				return new ShellSectionItemTemplatedView();
			});

			ShellContentItemTemplate = new DataTemplate(() =>
			{
				return new ShellContentItemTemplatedView();
			});
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is ShellSection)
				return ShellSectionItemTemplate;

			return ShellContentItemTemplate;
		}
	}
}
