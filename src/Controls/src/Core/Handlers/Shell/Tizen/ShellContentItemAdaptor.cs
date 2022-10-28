#nullable enable

using System.Collections;
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellContentItemAdaptor : ItemTemplateAdaptor
	{
		public ShellContentItemAdaptor(Element element, IEnumerable items) : base(element, items, GetTemplate()) { }

		protected override bool IsSelectable => true;

		static DataTemplate GetTemplate()
		{
			return new DataTemplate(() =>
			{
				return new ShellContentItemView();
			});
		}
	}
}
