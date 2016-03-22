using System;
#if __UNIFIED__
using Foundation;

#else
using MonoTouch.Foundation;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	internal static class CellExtensions
	{
		internal static NSIndexPath GetIndexPath(this Cell self)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			NSIndexPath path;

			if (self.RealParent is ListView)
			{
				var section = 0;
				var til = TemplatedItemsList<ItemsView<Cell>, Cell>.GetGroup(self);
				if (til != null)
					section = TemplatedItemsList<ItemsView<Cell>, Cell>.GetIndex(til.HeaderContent);

				var row = TemplatedItemsList<ItemsView<Cell>, Cell>.GetIndex(self);
				path = NSIndexPath.FromRowSection(row, section);
			}
			else if (self.RealParent is TableView)
			{
				var tmPath = TableView.TableSectionModel.GetPath(self);
				path = NSIndexPath.FromRowSection(tmPath.Item2, tmPath.Item1);
			}
			else
				throw new NotSupportedException("Unknown cell parent type");

			return path;
		}
	}
}