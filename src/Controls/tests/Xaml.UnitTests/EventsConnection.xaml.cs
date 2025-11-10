using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
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


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void TestClicked(XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.Equal(0, layout.clicked);
			layout.elementWithEvent.SendClicked();
			Assert.Equal(1, layout.clicked);
		}

		[Theory]
		[Values]
		public void TestGenericClicked(XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.Equal(0, layout.genericClicked);
			layout.elementWithGenericEvent.SendClicked();
			Assert.Equal(1, layout.genericClicked);
		}

		[Theory]
		[Values]
		public void TestHandlerOnBase(XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.Equal(0, layout.baseClicked);
			layout.eventHandlerOnBase.SendClicked();
			Assert.Equal(1, layout.baseClicked);
		}

		[Theory]
		[Values]
		public void TestAsyncPrivateHandler(XamlInflator inflator)
		{
			var layout = new EventsConnection(inflator);
			Assert.Equal(0, layout.asyncPrivateClicked);
			layout.elementwithAsyncprivateHandler.SendClicked();
			Assert.Equal(1, layout.asyncPrivateClicked);
		}

		[Theory]
		[Values]
		public void TestVirtualHandler(XamlInflator inflator)
		{
			var layout = new SubForEvents(inflator);
			Assert.Equal(0, layout.baseForVirtualClicked);
			Assert.Equal(0, layout.overrideClicked);
			layout.elementWithVirtualHandler.SendClicked();
			Assert.Equal(0, layout.baseForVirtualClicked);
			Assert.Equal(1, layout.overrideClicked);
		}

		[Theory]
		[Values]
		public void TestStaticHandler(XamlInflator inflator)
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