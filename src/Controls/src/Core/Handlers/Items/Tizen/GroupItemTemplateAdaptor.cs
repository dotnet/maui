using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{

	public class GroupItemTemplateAdaptor : ItemTemplateAdaptor
	{
		GroupItemSource _groupItemSource;
		DataTemplate _groupHeaderTemplate;
		DataTemplate _groupFooterTemplate;
		DataTemplate _itemTemplate;

		public GroupItemTemplateAdaptor(GroupableItemsView itemsView, GroupItemSource source) : base(itemsView, source, itemsView.ItemTemplate ?? new DefaultItemTemplate())
		{
			_groupHeaderTemplate = itemsView.GroupHeaderTemplate ?? new DefaultItemTemplate();
			_groupFooterTemplate = itemsView.GroupFooterTemplate ?? new DefaultItemTemplate();
			_itemTemplate = ItemTemplate;
			_groupItemSource = source;
		}

		/// <summary>
		/// Get absolute index in groups
		/// </summary>
		/// <param name="group">Index of group</param>
		/// <param name="inGroup">Index of item in group</param>
		/// <returns>Index that converted to absolute position</returns>
		public int GetAbsoluteIndex(int group, int inGroup)
		{
			return _groupItemSource.GetAbsoluteIndex(group, inGroup);
		}

		public override NView CreateNativeView(int index)
		{
			var (_, inGroup) = _groupItemSource.GetGroupAndIndex(index);
			if (inGroup == -1)
			{
				ItemTemplate = _groupHeaderTemplate;
			}
			else if (inGroup == -2)
			{
				ItemTemplate = _groupFooterTemplate;
			}
			var nativeView = base.CreateNativeView(index);

			ItemTemplate = _itemTemplate;
			return nativeView;
		}

		public override object GetViewCategory(int index)
		{
			var (_, inGroup) = _groupItemSource.GetGroupAndIndex(index);
			if (inGroup == -1)
			{
				return _groupHeaderTemplate;
			}
			else if (inGroup == -2)
			{
				return _groupFooterTemplate;
			}

			return base.GetViewCategory(index);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_groupItemSource?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
