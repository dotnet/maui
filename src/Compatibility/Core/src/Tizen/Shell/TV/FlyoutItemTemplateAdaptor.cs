using System.Collections;
using Xamarin.Forms.Platform.Tizen.Native;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.TV
{
	public class FlyoutItemTemplateAdaptor : ItemTemplateAdaptor
	{
		public FlyoutItemTemplateAdaptor(Element itemsView, IEnumerable items, DataTemplate template, bool hasHeader)
			: base(itemsView, items, template)
		{
			HasHeader = hasHeader;
		}

		public bool HasHeader { get; set; }

		protected override bool IsSelectable => true;

		protected override View CreateHeaderView()
		{
			if (!HasHeader)
				return null;

			View header = null;
			if (Element is IShellController shell)
			{
				header = shell.FlyoutHeader;
			}
			return header;
		}
	}
}
