#nullable disable
using System;
using Foundation;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal static class CellExtensions
	{
#pragma warning disable CS0618 // Type or member is obsolete
		internal static NSIndexPath GetIndexPath(this Cell self)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			NSIndexPath path;

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete

			return path;
		}
	}
}