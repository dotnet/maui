#nullable disable
using System;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Controls.Internals;
using AListView = Android.Widget.ListView;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public static class CellFactory
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public static AView GetCell(Cell item, AView convertView, ViewGroup parent, Context context, View view)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			// If the convert view coming in is null that means this cell is going to need a new view generated for it
			// This should probably be copied over to ListView once all sets of these TableView changes are propagated There
#pragma warning disable CS0618 // Type or member is obsolete
			if (item.Handler is IElementHandler handler && convertView is null && view is TableView)
			{
				handler.DisconnectHandler();
			}
#pragma warning restore CS0618 // Type or member is obsolete

			CellRenderer renderer = CellRenderer.GetRenderer(item);
			if (renderer == null)
			{
				var mauiContext = view.FindMauiContext() ?? item.FindMauiContext();
				item.ConvertView = convertView;

				convertView = item.ToPlatform(mauiContext);
				item.ConvertView = null;

				renderer = CellRenderer.GetRenderer(item);
			}

			AView result = renderer.GetCell(item, convertView, parent, context);

#pragma warning disable CS0618 // Type or member is obsolete
			if (view is TableView)
				UpdateMinimumHeightFromParent(context, result, (TableView)view);
			else if (view is ListView)
				UpdateMinimumHeightFromParent(context, result, (ListView)view);
#pragma warning restore CS0618 // Type or member is obsolete

			return result;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		static void UpdateMinimumHeightFromParent(Context context, AView view, TableView table)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			if (!table.HasUnevenRows && table.RowHeight > 0)
				view.SetMinimumHeight((int)context.ToPixels(table.RowHeight));
		}

#pragma warning disable CS0618 // Type or member is obsolete
		static void UpdateMinimumHeightFromParent(Context context, AView view, ListView listView)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			if (!listView.HasUnevenRows && listView.RowHeight > 0)
				view.SetMinimumHeight((int)context.ToPixels(listView.RowHeight));
		}
	}
}