#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CollectionViewHandler
	{
		public CollectionViewHandler() : base(Mapper)
		{

		}
		public CollectionViewHandler(PropertyMapper mapper = null) : base(mapper ?? Mapper)
		{

		}

		public static PropertyMapper<CollectionView, CollectionViewHandler> Mapper = new(ReorderableItemsViewMapper)
		{

		};
	}
}
