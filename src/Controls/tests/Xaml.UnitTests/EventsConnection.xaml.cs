using System;
using Xunit;

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

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void TestClicked(XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.Equal(0, layout.clicked);
			layout.elementWithEvent.SendClicked();
			Assert.Equal(1, layout.clicked);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestGenericClicked(XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.Equal(0, layout.genericClicked);
			layout.elementWithGenericEvent.SendClicked();
			Assert.Equal(1, layout.genericClicked);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestHandlerOnBase(XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.Equal(0, layout.baseClicked);
			layout.eventHandlerOnBase.SendClicked();
			Assert.Equal(1, layout.baseClicked);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestAsyncPrivateHandler(XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.Equal(0, layout.asyncPrivateClicked);
			layout.elementwithAsyncprivateHandler.SendClicked();
			Assert.Equal(1, layout.asyncPrivateClicked);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestVirtualHandler(XamlInflator inflator)
		{
			var layout = new SubForEvents(inflator);
			Assert.Equal(0, layout.baseForVirtualClicked);
			Assert.Equal(0, layout.overrideClicked);
			layout.elementWithVirtualHandler.SendClicked();
			Assert.Equal(0, layout.baseForVirtualClicked);
			Assert.Equal(1, layout.overrideClicked);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestStaticHandler(XamlInflator inflator)
		{
			try
			{
				var layout = new SubForEvents(inflator);
				Assert.Equal(0, staticClicked);
				layout.elementWithStaticHandler.SendClicked();
				Assert.Equal(1, staticClicked);
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