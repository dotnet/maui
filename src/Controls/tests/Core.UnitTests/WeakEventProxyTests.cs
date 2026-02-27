using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class WeakEventProxyTests
	{
		[Fact]
		public async Task DoesNotLeak()
		{
			WeakReference reference;
			var list = new ObservableCollection<string>();
			var proxy = new WeakNotifyCollectionChangedProxy();

			{
				var subscriber = new Subscriber();
				proxy.Subscribe(list, subscriber.OnCollectionChanged);
				reference = new WeakReference(subscriber);
			}

			Assert.False(await reference.WaitForCollect(), "Subscriber should not be alive!");
		}

		[Fact]
		public async Task EventFires()
		{
			var list = new ObservableCollection<string>();
			var proxy = new WeakNotifyCollectionChangedProxy();

			bool fired = false;
			// NOTE: this test wouldn't pass if we didn't save this and GC.KeepAlive() it
			NotifyCollectionChangedEventHandler handler = (s, e) => fired = true;
			proxy.Subscribe(list, handler);

			await TestHelpers.Collect();
			GC.KeepAlive(handler);

			list.Add("a");

			Assert.True(fired);
		}

		class Subscriber
		{
			public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }
		}
	}
}
