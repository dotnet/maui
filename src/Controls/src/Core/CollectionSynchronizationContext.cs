#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	internal sealed class CollectionSynchronizationContext
	{
		internal CollectionSynchronizationContext(object context, CollectionSynchronizationCallback callback)
		{
			ContextReference = new WeakReference(context);
			Callback = callback;
		}

		internal CollectionSynchronizationCallback Callback { get; private set; }

		internal object Context
		{
			get { return ContextReference?.Target; }
		}

		internal WeakReference ContextReference { get; }
	}
}