using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.SelectableItemsView"/> that presents a collection of items.</summary>
#if IOS || MACCATALYST
	[ElementHandler(typeof(Handlers.Items2.CollectionViewHandler2))]
#elif WINDOWS
	[CollectionViewHandler]
#else
	[ElementHandler(typeof(Handlers.Items.CollectionViewHandler))]
#endif
	public class CollectionView : ReorderableItemsView
	{
#if WINDOWS
		internal sealed class CollectionViewHandlerAttribute : ElementHandlerAttribute
		{
			[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
			public override Type GetHandlerType()
			{
				if (RuntimeFeature.IsWindowsCollectionView2HandlerEnabled)
					return typeof(Handlers.Items2.CollectionViewHandler2);

				return typeof(Handlers.Items.CollectionViewHandler);
			}
		}
#endif
	}
}
