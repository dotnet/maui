using System;

namespace Xamarin.Forms
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
			get { return ContextReference != null ? ContextReference.Target : null; }
		}

		internal WeakReference ContextReference { get; }
	}
}