using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Page.xml" path="Type[@FullName='Microsoft.Maui.Controls.Page']/Docs" />
	public partial class Page : IView, ITitledElement, IToolbarElement
	{
		Toolbar _toolbar;
		IToolbar IToolbarElement.Toolbar
		{
			get => _toolbar;
		}

		internal Toolbar Toolbar
		{
			get => _toolbar;
			set
			{
				_toolbar = value;
				if (this is NavigationPage np && value is Toolbar ct)
					ct.ApplyNavigationPage(np, HasAppeared);

				Handler?.UpdateValue(nameof(IToolbarElement.Toolbar));
			}
		}

		internal void SendNavigatedTo(NavigatedToEventArgs args)
		{
			NavigatedTo?.Invoke(this, args);
			OnNavigatedTo(args);
		}

		internal void SendNavigatingFrom(NavigatingFromEventArgs args)
		{
			NavigatingFrom?.Invoke(this, args);
			OnNavigatingFrom(args);
		}

		internal void SendNavigatedFrom(NavigatedFromEventArgs args)
		{
			NavigatedFrom?.Invoke(this, args);
			OnNavigatedFrom(args);
		}

		public event EventHandler<NavigatedToEventArgs> NavigatedTo;
		public event EventHandler<NavigatingFromEventArgs> NavigatingFrom;
		public event EventHandler<NavigatedFromEventArgs> NavigatedFrom;

		protected virtual void OnNavigatedTo(NavigatedToEventArgs args) { }
		protected virtual void OnNavigatingFrom(NavigatingFromEventArgs args) { }
		protected virtual void OnNavigatedFrom(NavigatedFromEventArgs args) { }

		/// <include file="../../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='GetParentWindow']/Docs" />
		public virtual Window GetParentWindow()
			=> this.FindParentOfType<Window>();
	}

	public sealed class NavigatingFromEventArgs : EventArgs
	{

	}

	public sealed class NavigatedToEventArgs : EventArgs
	{
		internal NavigatedToEventArgs(Page previousPage)
		{
			PreviousPage = previousPage;
		}

		internal Page PreviousPage { get; }
	}

	public sealed class NavigatedFromEventArgs : EventArgs
	{
		internal NavigatedFromEventArgs(Page destinationPage)
		{
			DestinationPage = destinationPage;
		}

		internal Page DestinationPage { get; }
	}
}
