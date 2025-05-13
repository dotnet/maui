using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/CollectionView.xml" path="Type[@FullName='Microsoft.Maui.Controls.CollectionView']/Docs/*" />
#if IOS || MACCATALYST
	[ElementHandler<Handlers.Items2.CollectionViewHandler2>]
#else
	[ElementHandler<Handlers.Items.CollectionViewHandler>]
#endif
	public class CollectionView : ReorderableItemsView
	{
	}
}
