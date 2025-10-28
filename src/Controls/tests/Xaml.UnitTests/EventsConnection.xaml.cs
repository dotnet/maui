using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class BaseForEvents : ContentView
	{
		protected int baseClicked;
		protected void HandleClickedOnBase(object sender, EventArgs e)
		{
			baseClicked++;
		}
	}

	public class ElementWithEvent : ContentView
	{
		public event EventHandler Clicked;

		public void SendClicked()
		{
			var eh = Clicked;
			if (eh != null)
				eh(this, EventArgs.Empty);
		}
	}

	public class ElementWithGenericEvent : ContentView
	{
		public event EventHandler<ItemTappedEventArgs> Clicked;

		public void SendClicked()
		{
			var eh = Clicked;
			if (eh != null)
				eh(this, new ItemTappedEventArgs("foo", "bar", -1));
		}
	}

	public partial class EventsConnection : BaseForEvents
	{
		public EventsConnection()
		{
			InitializeComponent();
		}

		public EventsConnection(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		int clicked;
		public void HandleClicked(object sender, EventArgs e)
		{
			clicked++;
		}

		int genericClicked;
		public void HandleGenericClicked(object sender, ItemTappedEventArgs e)
		{
			genericClicked++;
		}

		int asyncPrivateClicked;
#pragma warning disable 1998 // considered for removal
		async void HandleClickedPrivateAsync(object sender, EventArgs e)
#pragma warning restore 1998
		{
			asyncPrivateClicked++;
		}

		int baseForVirtualClicked;
		protected virtual void HandleVirtualClicked(object sender, EventArgs e)
		{
			baseForVirtualClicked++;
		}

		protected static int staticClicked;

		// This is necessary because the interpreter searches the class
		// specified by x:Class for a static method.
		// See: https://github.com/xamarin/Microsoft.Maui.Controls/issues/5100
		static void HandleStaticClicked(object sender, EventArgs e)
		{
			staticClicked++;
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void TestClicked(bool useCompiledXaml)
			{
				var layout = new EventsConnection(useCompiledXaml);
				Assert.Equal(0, layout.clicked);
				layout.elementWithEvent.SendClicked();
				Assert.Equal(1, layout.clicked);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void TestGenericClicked(bool useCompiledXaml)
			{
				var layout = new EventsConnection(useCompiledXaml);
				Assert.Equal(0, layout.genericClicked);
				layout.elementWithGenericEvent.SendClicked();
				Assert.Equal(1, layout.genericClicked);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void TestHandlerOnBase(bool useCompiledXaml)
			{
				var layout = new EventsConnection(useCompiledXaml);
				Assert.Equal(0, layout.baseClicked);
				layout.eventHandlerOnBase.SendClicked();
				Assert.Equal(1, layout.baseClicked);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void TestAsyncPrivateHandler(bool useCompiledXaml)
			{
				var layout = new EventsConnection(useCompiledXaml);
				Assert.Equal(0, layout.asyncPrivateClicked);
				layout.elementwithAsyncprivateHandler.SendClicked();
				Assert.Equal(1, layout.asyncPrivateClicked);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void TestVirtualHandler(bool useCompiledXaml)
			{
				var layout = new SubForEvents(useCompiledXaml);
				Assert.Equal(0, layout.baseForVirtualClicked);
				Assert.Equal(0, layout.overrideClicked);
				layout.elementWithVirtualHandler.SendClicked();
				Assert.Equal(0, layout.baseForVirtualClicked);
				Assert.Equal(1, layout.overrideClicked);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void TestStaticHandler(bool useCompiledXaml)
			{
				try
				{
					var layout = new SubForEvents(useCompiledXaml);
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

	public class SubForEvents : EventsConnection
	{
		public SubForEvents(bool useCompiledXaml) : base(useCompiledXaml)
		{
		}

		public int overrideClicked;
		protected override void HandleVirtualClicked(object sender, EventArgs e)
		{
			overrideClicked++;
		}

#pragma warning disable 1998 // considered for removal
		async void HandleClickedPrivateAsync(object sender, EventArgs e)
#pragma warning restore 1998
		{
		}

		// This is necessary because the interpreter searches the subclass
		// for a static method.
		// See: https://github.com/xamarin/Microsoft.Maui.Controls/issues/5100
		static void HandleStaticClicked(object sender, EventArgs e)
		{
			staticClicked++;
		}
	}
}