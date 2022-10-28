#nullable enable

using System.Collections;
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellSectionItemAdaptor : ItemTemplateAdaptor
	{
		public ShellSectionItemAdaptor(Element element, IEnumerable items) : base(element, items, GetTemplate()) { }

		protected override bool IsSelectable => true;

		static DataTemplate GetTemplate()
		{
			return new ShellSectionDataTemplateSelector();
		}

		class ShellSectionDataTemplateSelector : DataTemplateSelector
		{
			DataTemplate ShellSectionItemTemplate { get; }
			DataTemplate MoreItemTemplate { get; }

			public ShellSectionDataTemplateSelector()
			{
				ShellSectionItemTemplate = new DataTemplate(() =>
				{
					return new ShellSectionItemView(false);
				});

				MoreItemTemplate = new DataTemplate(() =>
				{
					return new ShellSectionItemView(true);
				});
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				if (item is ShellSection)
					return ShellSectionItemTemplate;

				return MoreItemTemplate;
			}
		}
	}
}
