#nullable disable
using System.Collections;
using Microsoft.Maui.Controls.Handlers.Items;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class ListViewAdaptor : ItemTemplateAdaptor
	{
		new ListView Element { get; set; }

		public ListViewAdaptor(ListView listview, IEnumerable items, DataTemplate template) : base(listview, items, template)
		{
			Element = listview;
		}

		protected override bool IsSelectable => (Element?.SelectionMode ?? ListViewSelectionMode.None) == ListViewSelectionMode.Single;

		public override TSize MeasureItem(double widthConstraint, double heightConstraint)
		{
			if (Element.RowHeight > 0)
			{
				return new TSize(widthConstraint, Element.RowHeight);
			}
			return MeasureItem(0, widthConstraint, heightConstraint);
		}

		public override TSize MeasureItem(int index, double widthConstraint, double heightConstraint)
		{
			if (index < 0 || index >= Element.TemplatedItems.Count)
				return new TSize(0, 0);

			var cell = Element.TemplatedItems[index];
			if (cell.RenderHeight > 0)
			{
				return new TSize(widthConstraint, cell.RenderHeight);
			}
			return base.MeasureItem(index, widthConstraint, heightConstraint);
		}

		protected override View CreateHeaderView()
		{
			return Element.HeaderElement as View;
		}

		protected override View CreateFooterView()
		{
			return Element.FooterElement as View;
		}
	}
}
