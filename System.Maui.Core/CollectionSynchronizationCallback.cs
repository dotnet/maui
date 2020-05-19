using System;
using System.Collections;

namespace System.Maui
{
	public delegate void CollectionSynchronizationCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess);
}