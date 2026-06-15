using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.SelectableItemsView"/> that presents a collection of items.</summary>
#if IOS || MACCATALYST
	[ElementHandler(typeof(Handlers.Items2.CollectionViewHandler2))]
#else
	[ElementHandler(typeof(Handlers.Items.CollectionViewHandler))]
#endif
	public class CollectionView : ReorderableItemsView
	{
	}
}
