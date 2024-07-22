#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class CollectionViewHandler2
	{
		public CollectionViewHandler2() : base(CollectionViewHandler.Mapper)
		{

		}
		public CollectionViewHandler2(PropertyMapper mapper = null) : base(mapper ?? CollectionViewHandler.Mapper)
		{
		}
	}
}
