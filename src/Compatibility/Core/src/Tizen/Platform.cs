using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;

[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Material")]

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[Obsolete]
	public static class Platform
	{
		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var view = bindable as VisualElement;
				if (view != null)
					view.IsPlatformEnabled = newvalue != null;

				if (bindable is IView mauiView)
				{
					if (mauiView.Handler == null && newvalue is IVisualElementRenderer ver)
						mauiView.Handler = new RendererToHandlerShim(ver);
				}
			});

		public static IVisualElementRenderer GetRenderer(BindableObject bindable)
		{
			return (IVisualElementRenderer)bindable.GetValue(Platform.RendererProperty);
		}

		public static void SetRenderer(BindableObject bindable, IVisualElementRenderer value)
		{
			bindable.SetValue(Platform.RendererProperty, value);
		}

		/// <summary>
		/// Gets the renderer associated with the <c>view</c>. If it doesn't exist, creates a new one.
		/// </summary>
		/// <returns>Renderer associated with the <c>view</c>.</returns>
		/// <param name="element">VisualElement for which the renderer is going to be returned.</param>
		public static IVisualElementRenderer GetOrCreateRenderer(VisualElement element)
		{
			return GetRenderer(element) ?? CreateRenderer(element);
		}

		internal static IVisualElementRenderer CreateRenderer(VisualElement element)
		{
			IVisualElementRenderer renderer = null;

			if (renderer == null)
			{
				IViewHandler handler = null;

				//TODO: Handle this with AppBuilderHost
				try
				{
					var handlerType = Forms.MauiContext.Handlers.GetHandlerType(element.GetType());
					handler = (IViewHandler)Activator.CreateInstance(handlerType);
					handler.SetMauiContext(Forms.MauiContext);
				}
				catch
				{
					// TODO define better catch response or define if this is needed?
				}

				if (handler == null)
				{
					renderer = Forms.GetHandlerForObject<IVisualElementRenderer>(element) ?? new DefaultRenderer();
				}
				// This means the only thing registered is the RendererToHandlerShim
				// Which is only used when you are running a .NET MAUI app
				// This indicates that the user hasn't registered a specific handler for this given type
				else if (handler is RendererToHandlerShim shim)
				{
					renderer = shim.VisualElementRenderer;

					if (renderer == null)
					{
						renderer = Forms.GetHandlerForObject<IVisualElementRenderer>(element) ?? new DefaultRenderer();
					}
				}
				else if (handler is IVisualElementRenderer ver)
					renderer = ver;
				else if (handler is IPlatformViewHandler vh)
				{
					renderer = new HandlerToRendererShim(vh);
					element.Handler = handler;
				}
			}
			renderer.SetElement(element);
			return renderer;
		}

		internal static ITizenPlatform CreatePlatform()
		{
			return new DefaultPlatform();
		}

		public static SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			widthConstraint = widthConstraint <= -1 ? double.PositiveInfinity : widthConstraint;
			heightConstraint = heightConstraint <= -1 ? double.PositiveInfinity : heightConstraint;

			var renderView = GetRenderer(view);
			if (renderView == null || renderView.NativeView == null)
			{
				return (view is IView iView) ? new SizeRequest(iView.Handler.GetDesiredSize(widthConstraint, heightConstraint)) : new SizeRequest(Graphics.Size.Zero);
			}

			return renderView.GetDesiredSize(widthConstraint, heightConstraint);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ITizenPlatform : IDisposable
	{
		void SetPage(Page page);
		bool SendBackButtonPressed();
		NView GetRootNativeView();

		bool PageIsChildOfPlatform(Page page);
	}

	[Obsolete]
	public class DefaultPlatform : BindableObject, ITizenPlatform, INavigation
	{
		NavigationModel _navModel = new NavigationModel();
		bool _disposed;
		readonly NavigationStack _viewStack;
		readonly PopupManager _popupManager;

		readonly Dictionary<NView, Page> _pageMap = new Dictionary<NView, Page>();


		internal DefaultPlatform()
		{
			_viewStack = new NavigationStack
			{
				BackgroundColor = global::Tizen.NUI.Color.White,
				WidthResizePolicy = global::Tizen.NUI.ResizePolicyType.FillToParent,
				HeightResizePolicy = global::Tizen.NUI.ResizePolicyType.FillToParent,
			};

			if (Forms.UseMessagingCenter)
			{
				_popupManager = new PopupManager(this);
			}
		}

		~DefaultPlatform()
		{
			Dispose(false);
		}

		public Page Page { get; private set; }

		Page CurrentPage
		{
			get
			{
				if (_viewStack.Top != null && _pageMap.ContainsKey(_viewStack.Top))
				{
					return _pageMap[_viewStack.Top];
				}
				return null;
			}
		}

		IPageController CurrentPageController => CurrentPage;
		IReadOnlyList<Page> INavigation.ModalStack => _navModel.Modals.ToList();
		IReadOnlyList<Page> INavigation.NavigationStack => new List<Page>();

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void SetPage(Page newRoot)
		{
			if (Page != null)
			{
				foreach (var child in _viewStack.Children.ToList())
				{
					child.Dispose();
				}
				_viewStack.Clear();
				_pageMap.Clear();
			}
			_navModel = new NavigationModel();

			if (newRoot == null)
				return;

			Page = newRoot;
			_navModel.Push(newRoot, null);

			((Application)Page.RealParent).NavigationProxy.Inner = this;

			IVisualElementRenderer pageRenderer = Platform.CreateRenderer(Page);
			_pageMap[pageRenderer.NativeView] = newRoot;
			_viewStack.Push(pageRenderer.NativeView, false);

			Application.Current.Dispatcher.Dispatch(() => CurrentPageController?.SendAppearing());
		}

		public bool SendBackButtonPressed()
		{
			return _navModel.CurrentPage?.SendBackButtonPressed() ?? false;
		}

		public NView GetRootNativeView()
		{
			return _viewStack;
		}

		public bool PageIsChildOfPlatform(Page page)
		{
			var parent = page.AncestorToRoot();
			return _navModel.Modals.FirstOrDefault() == page || _navModel.Roots.Contains(parent);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				_popupManager?.Dispose();
				SetPage(null);
				_viewStack.Unparent();
				_viewStack.Dispose();
			}
			_disposed = true;
		}

		protected override void OnBindingContextChanged()
		{
			BindableObject.SetInheritedBindingContext(Page, base.BindingContext);
			base.OnBindingContextChanged();
		}

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			throw new InvalidOperationException("InsertPageBefore is not supported globally on Tizen, please use a NavigationPage.");
		}

		Task<Page> INavigation.PopAsync()
		{
			return ((INavigation)this).PopAsync(true);
		}

		Task<Page> INavigation.PopAsync(bool animated)
		{
			throw new InvalidOperationException("PopAsync is not supported globally on Tizen, please use a NavigationPage.");
		}

		Task INavigation.PopToRootAsync()
		{
			return ((INavigation)this).PopToRootAsync(true);
		}

		Task INavigation.PopToRootAsync(bool animated)
		{
			throw new InvalidOperationException("PopToRootAsync is not supported globally on Tizen, please use a NavigationPage.");
		}

		Task INavigation.PushAsync(Page root)
		{
			return ((INavigation)this).PushAsync(root, true);
		}

		Task INavigation.PushAsync(Page root, bool animated)
		{
			throw new InvalidOperationException("PushAsync is not supported globally on Tizen, please use a NavigationPage.");
		}

		void INavigation.RemovePage(Page page)
		{
			throw new InvalidOperationException("RemovePage is not supported globally on Tizen, please use a NavigationPage.");
		}

		Task INavigation.PushModalAsync(Page modal)
		{
			return ((INavigation)this).PushModalAsync(modal, true);
		}

		async Task INavigation.PushModalAsync(Page modal, bool animated)
		{
			var previousPage = CurrentPageController;
			previousPage?.SendDisappearing();

			_navModel.PushModal(modal);

			var renderer = Platform.GetOrCreateRenderer(modal);
			_pageMap[renderer.NativeView] = modal;
			await _viewStack.Push(renderer.NativeView, animated);
			CurrentPageController.SendAppearing();
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return ((INavigation)this).PopModalAsync(true);
		}

		async Task<Page> INavigation.PopModalAsync(bool animated)
		{
			Page page = _navModel.PopModal();
			(page as IPageController)?.SendDisappearing();

			IVisualElementRenderer modalRenderer = Platform.GetRenderer(page);

			await _viewStack.Pop(animated);
			_pageMap.Remove(modalRenderer.NativeView);

			modalRenderer?.Dispose();

			CurrentPageController?.SendAppearing();

			return page;
		}
	}
}
