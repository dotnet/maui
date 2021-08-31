using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage : INavigationView
	{
		Thickness IView.Margin => Thickness.Zero;

		partial void Init()
		{
			PushRequested += (_, args) =>
			{
				var request = new MauiNavigationRequestedEventArgs(args.Page, args.BeforePage, args.Animated);
				Handler?.Invoke(nameof(INavigationView.PushAsync), request);

			};

			PopRequested += (_, args) =>
			{
				var request = new MauiNavigationRequestedEventArgs(args.Page, args.BeforePage, args.Animated);
				Handler?.Invoke(nameof(INavigationView.PopAsync), request);
			};
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			if (Content is IView view)
			{
				view.Measure(widthConstraint, heightConstraint);
			}

			return new Size(widthConstraint, heightConstraint);
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);

			if (Content is IView view)
			{
				_ = view.Arrange(Frame);
			}

			return Frame.Size;
		}

		IElementHandler _previousHandler;
		protected override void OnHandlerChanged()
		{
			// Because the navigation handler is shimmed we disconnect from it so it disposes
			if (_previousHandler?.VirtualView == this)
				_previousHandler?.DisconnectHandler();

			_previousHandler = null;
			base.OnHandlerChanged();
		}

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			_previousHandler = args.OldHandler;
		}

		void INavigationView.InsertPageBefore(IView page, IView before)
		{
			throw new NotImplementedException();
		}

		Task<IView> INavigationView.PopAsync() =>
			(this as INavigationView).PopAsync(true);

		async Task<IView> INavigationView.PopAsync(bool animated) =>
			await this.PopAsync(animated);

		Task<IView> INavigationView.PopModalAsync()
		{
			throw new NotImplementedException();
		}

		Task<IView> INavigationView.PopModalAsync(bool animated)
		{
			throw new NotImplementedException();
		}

		Task INavigationView.PushAsync(IView page) =>
			(this as INavigationView).PushAsync(page, true);

		Task INavigationView.PushAsync(IView page, bool animated)
		{
			return this.PushAsync((Page)page, animated);
		}

		Task INavigationView.PushModalAsync(IView page)
		{
			throw new NotImplementedException();
		}

		Task INavigationView.PushModalAsync(IView page, bool animated)
		{
			throw new NotImplementedException();
		}

		void INavigationView.RemovePage(IView page)
		{
			throw new NotImplementedException();
		}

		IView Content => this.CurrentPage;

		IReadOnlyList<IView> INavigationView.ModalStack => throw new NotImplementedException();

		IReadOnlyList<IView> INavigationView.NavigationStack =>
			this.Navigation.NavigationStack;
	}

}
