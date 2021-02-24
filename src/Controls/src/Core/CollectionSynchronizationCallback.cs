using System;
using System.Collections;

namespace Microsoft.Maui.Controls
{
	public delegate void CollectionSynchronizationCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess);
}