using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class BaseForEvents : ContentView
{
	protected int baseClicked;
	protected void HandleClickedOnBase(object sender, EventArgs e) => baseClicked++;
}

public class ElementWithEvent : ContentView
{
	public event EventHandler Clicked;

	public void SendClicked() => Clicked?.Invoke(this, EventArgs.Empty);
}

public class ElementWithGenericEvent : ContentView
{
	public event EventHandler<ItemTappedEventArgs> Clicked;

	public void SendClicked() => Clicked?.Invoke(this, new ItemTappedEventArgs("foo", "bar", -1));
}

public partial class EventsConnection : BaseForEvents
{
	public EventsConnection()
	{
		InitializeComponent();
	}

	int clicked;
	public void HandleClicked(object sender, EventArgs e) => clicked++;

	int genericClicked;
	public void HandleGenericClicked(object sender, ItemTappedEventArgs e) => genericClicked++;

	int asyncPrivateClicked;
#pragma warning disable 1998 // considered for removal
	async void HandleClickedPrivateAsync(object sender, EventArgs e) => asyncPrivateClicked++;
#pragma warning restore 1998

	int baseForVirtualClicked;
	protected virtual void HandleVirtualClicked(object sender, EventArgs e) => baseForVirtualClicked++;

	protected static int staticClicked;

	// This is necessary because the interpreter searches the class
	// specified by x:Class for a static method.
	// See: https://github.com/xamarin/Microsoft.Maui.Controls/issues/5100
	static void HandleStaticClicked(object sender, EventArgs e) => staticClicked++;

	[TestFixture]
	class Tests
	{
		[Test]
		public void TestClicked([Values] XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.AreEqual(0, layout.clicked);
			layout.elementWithEvent.SendClicked();
			Assert.AreEqual(1, layout.clicked);
		}

		[Test]
		public void TestGenericClicked([Values] XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.AreEqual(0, layout.genericClicked);
			layout.elementWithGenericEvent.SendClicked();
			Assert.AreEqual(1, layout.genericClicked);
		}

		[Test]
		public void TestHandlerOnBase([Values] XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.AreEqual(0, layout.baseClicked);
			layout.eventHandlerOnBase.SendClicked();
			Assert.AreEqual(1, layout.baseClicked);
		}

		[Test]
		public void TestAsyncPrivateHandler([Values] XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.AreEqual(0, layout.asyncPrivateClicked);
			layout.elementwithAsyncprivateHandler.SendClicked();
			Assert.AreEqual(1, layout.asyncPrivateClicked);
		}

		[Test]
		public void TestVirtualHandler([Values] XamlInflator inflator)
		{
			var layout = new SubForEvents(inflator);
			Assert.AreEqual(0, layout.baseForVirtualClicked);
			Assert.AreEqual(0, layout.overrideClicked);
			layout.elementWithVirtualHandler.SendClicked();
			Assert.AreEqual(0, layout.baseForVirtualClicked);
			Assert.AreEqual(1, layout.overrideClicked);
		}

		[Test]
		public void TestStaticHandler([Values] XamlInflator inflator)
		{
			try
			{
				var layout = new SubForEvents(inflator);
				Assert.AreEqual(0, staticClicked);
				layout.elementWithStaticHandler.SendClicked();
				Assert.AreEqual(1, staticClicked);
			}
			finally
			{
				staticClicked = 0;
			}
		}
	}
}

class SubForEvents : EventsConnection
{
	public SubForEvents(XamlInflator inflator) : base(inflator)
	{
	}

	public int overrideClicked;
	protected override void HandleVirtualClicked(object sender, EventArgs e) => overrideClicked++;

#pragma warning disable 1998 // considered for removal
	async void HandleClickedPrivateAsync(object sender, EventArgs e)
#pragma warning restore 1998
	{
	}

	// This is necessary because the interpreter searches the subclass
	// for a static method.
	// See: https://github.com/xamarin/Microsoft.Maui.Controls/issues/5100
	static void HandleStaticClicked(object sender, EventArgs e) => staticClicked++;
}