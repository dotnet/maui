using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Maui.Controls
{
<<<<<<< HEAD
	/// <include file="../../../docs/Microsoft.Maui.Controls/CollectionView.xml" path="Type[@FullName='Microsoft.Maui.Controls.CollectionView']/Docs/*" />
#if IOS || MACCATALYST
	[ElementHandler<Handlers.Items2.CollectionViewHandler2>]
#else
	[ElementHandler<Handlers.Items.CollectionViewHandler>]
#endif
||||||| 3f26a592b2
	/// <include file="../../../docs/Microsoft.Maui.Controls/CollectionView.xml" path="Type[@FullName='Microsoft.Maui.Controls.CollectionView']/Docs/*" />
=======
	/// <summary>A <see cref="Microsoft.Maui.Controls.SelectableItemsView"/> that presents a collection of items.</summary>
>>>>>>> 485b400ee4a317af11647f3e64085d7d8d4d5f17
	public class CollectionView : ReorderableItemsView
	{
	}
}
