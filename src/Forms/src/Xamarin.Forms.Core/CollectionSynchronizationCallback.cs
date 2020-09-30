using System;
using System.Collections;

namespace Xamarin.Forms
{
	public delegate void CollectionSynchronizationCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess);
}