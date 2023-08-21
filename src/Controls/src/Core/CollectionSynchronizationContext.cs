// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
			get { return ContextReference != null ? ContextReference.Target : null; }
		}

		internal WeakReference ContextReference { get; }
	}
}