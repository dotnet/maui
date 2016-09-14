using System;
using Foundation;
using Xamarin.Forms.Internals;

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
				var til = self.GetGroup<ItemsView<Cell>, Cell>();
				if (til != null)
					section = til.HeaderContent.GetIndex<ItemsView<Cell>, Cell>();

				var row = self.GetIndex<ItemsView<Cell>, Cell>();
				path = NSIndexPath.FromRowSection(row, section);
			}
			else if (self.RealParent is TableView)
			{
				var tmPath = self.GetPath();
				path = NSIndexPath.FromRowSection(tmPath.Item2, tmPath.Item1);
			}
			else
				throw new NotSupportedException("Unknown cell parent type");

			return path;
		}
	}
}