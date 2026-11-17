namespace Microsoft.Maui.Controls.Internals;

interface ICache<TKey, TValue>
{
	TValue Get(TKey key);
}
