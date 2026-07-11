#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A "proxy" class for subscribing <see cref="TableRoot.SectionCollectionChanged"/> via WeakReference.
	/// General usage is to store this in a member variable and call Subscribe()/Unsubscribe() appropriately.
	/// The owning class should have a finalizer that calls Unsubscribe() to prevent these objects from leaking.
	/// </summary>
	class WeakSectionCollectionChangedProxy : WeakEventProxy<TableRoot, EventHandler<ChildCollectionChangedEventArgs>>
	{
		public WeakSectionCollectionChangedProxy() { }

		void OnSectionCollectionChanged(object? sender, ChildCollectionChangedEventArgs e)
		{
			if (TryGetHandler(out var handler))
			{
				handler(sender, e);
			}
			else
			{
				Unsubscribe();
			}
		}

		public override void Subscribe(TableRoot source, EventHandler<ChildCollectionChangedEventArgs> handler)
		{
			if (TryGetSource(out var s))
			{
				s.SectionCollectionChanged -= OnSectionCollectionChanged;
			}

			source.SectionCollectionChanged += OnSectionCollectionChanged;
			base.Subscribe(source, handler);
		}

		public override void Unsubscribe()
		{
			if (TryGetSource(out var s))
			{
				s.SectionCollectionChanged -= OnSectionCollectionChanged;
			}

			base.Unsubscribe();
		}
	}
}
