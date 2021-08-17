using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CollectionViewHandler
	{
		public CollectionViewHandler() : base(CollectionViewMapper)
		{

		}
		public CollectionViewHandler(PropertyMapper mapper = null) : base(mapper ?? CollectionViewMapper)
		{

		}

		public static PropertyMapper<CollectionView, CollectionViewHandler> CollectionViewMapper = new PropertyMapper<CollectionView, CollectionViewHandler>(GroupableItemsViewHandler<GroupableItemsView>.GroupableItemsViewMapper)
		{

		};
	}
}
