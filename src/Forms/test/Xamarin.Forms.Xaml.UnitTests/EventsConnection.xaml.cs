using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
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
		// See: https://github.com/xamarin/Xamarin.Forms/issues/5100
		static void HandleStaticClicked(object sender, EventArgs e)
		{
			staticClicked++;
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void TestClicked(bool useCompiledXaml)
			{
				var layout = new EventsConnection(useCompiledXaml);
				Assert.AreEqual(0, layout.clicked);
				layout.elementWithEvent.SendClicked();
				Assert.AreEqual(1, layout.clicked);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestGenericClicked(bool useCompiledXaml)
			{
				var layout = new EventsConnection(useCompiledXaml);
				Assert.AreEqual(0, layout.genericClicked);
				layout.elementWithGenericEvent.SendClicked();
				Assert.AreEqual(1, layout.genericClicked);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestHandlerOnBase(bool useCompiledXaml)
			{
				var layout = new EventsConnection(useCompiledXaml);
				Assert.AreEqual(0, layout.baseClicked);
				layout.eventHandlerOnBase.SendClicked();
				Assert.AreEqual(1, layout.baseClicked);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestAsyncPrivateHandler(bool useCompiledXaml)
			{
				var layout = new EventsConnection(useCompiledXaml);
				Assert.AreEqual(0, layout.asyncPrivateClicked);
				layout.elementwithAsyncprivateHandler.SendClicked();
				Assert.AreEqual(1, layout.asyncPrivateClicked);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestVirtualHandler(bool useCompiledXaml)
			{
				var layout = new SubForEvents(useCompiledXaml);
				Assert.AreEqual(0, layout.baseForVirtualClicked);
				Assert.AreEqual(0, layout.overrideClicked);
				layout.elementWithVirtualHandler.SendClicked();
				Assert.AreEqual(0, layout.baseForVirtualClicked);
				Assert.AreEqual(1, layout.overrideClicked);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestStaticHandler(bool useCompiledXaml)
			{
				try
				{
					var layout = new SubForEvents(useCompiledXaml);
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
		// See: https://github.com/xamarin/Xamarin.Forms/issues/5100
		static void HandleStaticClicked(object sender, EventArgs e)
		{
			staticClicked++;
		}
	}
}