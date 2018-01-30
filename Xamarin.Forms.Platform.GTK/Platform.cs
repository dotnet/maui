using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK.Helpers;
using Xamarin.Forms.Platform.GTK.Renderers;

namespace Xamarin.Forms.Platform.GTK
{
    public class Platform : BindableObject, IPlatform, INavigation, IDisposable
    {
        private bool _disposed;
        readonly List<Page> _modals;
        private readonly PlatformRenderer _renderer;

        internal static readonly BindableProperty RendererProperty =
            BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer),
                typeof(Platform), default(IVisualElementRenderer),
            propertyChanged: (bindable, oldvalue, newvalue) =>
            {
                var view = bindable as VisualElement;
                if (view != null)
                    view.IsPlatformEnabled = newvalue != null;
            });

        internal PlatformRenderer PlatformRenderer => _renderer;

        Page Page { get; set; }

        IReadOnlyList<Page> INavigation.ModalStack
        {
            get { return _modals; }
        }

        IReadOnlyList<Page> INavigation.NavigationStack
        {
            get { return new List<Page>(); }
        }

        internal Platform()
        {
            _renderer = new PlatformRenderer(this);
            _modals = new List<Page>();
            Application.Current.NavigationProxy.Inner = this;

            MessagingCenter.Subscribe(this, Page.AlertSignalName, (Page sender, AlertArguments arguments) => DialogHelper.ShowAlert(PlatformRenderer, arguments));
            MessagingCenter.Subscribe(this, Page.ActionSheetSignalName, (Page sender, ActionSheetArguments arguments) => DialogHelper.ShowActionSheet(PlatformRenderer, arguments));
        }

        internal static void DisposeModelAndChildrenRenderers(Element view)
        {
            IVisualElementRenderer renderer;

            foreach (VisualElement child in view.Descendants())
                DisposeModelAndChildrenRenderers(child);

            renderer = GetRenderer((VisualElement)view);

            renderer?.Dispose();

            view.ClearValue(RendererProperty);
        }

        SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
        {
            var renderView = GetRenderer(view);

            if (renderView == null || renderView.Container == null)
                return new SizeRequest(Size.Zero);

            return renderView.GetDesiredSize(widthConstraint, heightConstraint);
        }

        public static IVisualElementRenderer GetRenderer(VisualElement element)
        {
            return (IVisualElementRenderer)element.GetValue(RendererProperty);
        }

        public static void SetRenderer(VisualElement element, IVisualElementRenderer value)
        {
            if (element != null)
            {
                element.SetValue(RendererProperty, value);
                element.IsPlatformEnabled = value != null;
            }
        }

        public static IVisualElementRenderer CreateRenderer(VisualElement element)
        {
            var renderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element) ?? new DefaultRenderer();
            renderer.SetElement(element);

            return renderer;
        }

        void IDisposable.Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
    
            MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName);
            MessagingCenter.Unsubscribe<Page, AlertArguments>(this, Page.AlertSignalName);
            MessagingCenter.Unsubscribe<Page, bool>(this, Page.BusySetSignalName);

            DisposeModelAndChildrenRenderers(Page);
            foreach (var modal in _modals)
                DisposeModelAndChildrenRenderers(modal);

            PlatformRenderer.Dispose();
        }

        internal void SetPage(Page newRoot)
        {
            if (newRoot == null)
                return;

            if (Page != null)
                throw new NotImplementedException();

            Page = newRoot;
            Page.Platform = this;

            AddChild(Page);

            Application.Current.NavigationProxy.Inner = this;
        }

        private void AddChild(Page mainPage)
        {
            var viewRenderer = GetRenderer(mainPage);

            if (viewRenderer == null)
            {
                viewRenderer = CreateRenderer(mainPage);
                SetRenderer(mainPage, viewRenderer);

                PlatformRenderer.Add(viewRenderer.Container);
                PlatformRenderer.ShowAll();
            }
        }

        private void HandleChildRemoved(object sender, ElementEventArgs e)
        {
            var view = e.Element;
            DisposeModelAndChildrenRenderers(view);
        }

        void INavigation.InsertPageBefore(Page page, Page before)
        {
            throw new InvalidOperationException("InsertPageBefore is not supported globally on GTK, please use a NavigationPage.");
        }

        Task<Page> INavigation.PopAsync()
        {
            return ((INavigation)this).PopAsync(true);
        }

        Task<Page> INavigation.PopAsync(bool animated)
        {
            throw new InvalidOperationException("PopAsync is not supported globally on GTK, please use a NavigationPage.");
        }

        Task<Page> INavigation.PopModalAsync()
        {
            return ((INavigation)this).PopModalAsync(true);
        }

        Task<Page> INavigation.PopModalAsync(bool animated)
        {
            var modal = _modals.Last();
            _modals.Remove(modal);
            modal.DescendantRemoved -= HandleChildRemoved;

            var modalPage = GetRenderer(modal) as Container;

            var pageControl = PlatformRenderer.Child as IPageControl;

            Device.BeginInvokeOnMainThread(() =>
            {
                if (pageControl != null)
                {
                    var page = pageControl.Control;

                    if (page != null)
                    {
                        if (page.Children.Length > 0)
                        {
                            page.Remove(modalPage);
                        }

                        if (page.Children != null)
                        {
                            foreach (var child in page.Children)
                            {
                                child.ShowAll();
                            }

                            page.ShowAll();
                        }
                    }
                }

                DisposeModelAndChildrenRenderers(modal);
            });

            return Task.FromResult<Page>(modal);
        }

        Task INavigation.PopToRootAsync()
        {
            return ((INavigation)this).PopToRootAsync(true);
        }

        Task INavigation.PopToRootAsync(bool animated)
        {
            throw new InvalidOperationException("PopToRootAsync is not supported globally on GTK, please use a NavigationPage.");
        }

        Task INavigation.PushAsync(Page root)
        {
            return ((INavigation)this).PushAsync(root, true);
        }

        Task INavigation.PushAsync(Page root, bool animated)
        {
            throw new InvalidOperationException("PushAsync is not supported globally on GTK, please use a NavigationPage.");
        }

        Task INavigation.PushModalAsync(Page modal)
        {
            return ((INavigation)this).PushModalAsync(modal, true);
        }

        Task INavigation.PushModalAsync(Page modal, bool animated)
        {
            _modals.Add(modal);
            modal.Platform = this;
            modal.DescendantRemoved += HandleChildRemoved;

            var modalRenderer = GetRenderer(modal);
            if (modalRenderer == null)
            {
                modalRenderer = CreateRenderer(modal);
                SetRenderer(modal, modalRenderer);
            }

            var pageControl = PlatformRenderer.Child as IPageControl;

            Device.BeginInvokeOnMainThread(() =>
            {
                if (pageControl != null)
                {
                    var page = pageControl.Control;

                    if (page != null)
                    {
                        page.Attach(modalRenderer.Container, 0, 1, 0, 1);

                        if (page.Children != null)
                        {
                            foreach (var child in page.Children)
                            {
                                child.ShowAll();
                            }

                            page.ShowAll();
                        }
                    }
                }
            });

            return Task.FromResult<object>(null);
        }

        void INavigation.RemovePage(Page page)
        {
            throw new InvalidOperationException("RemovePage is not supported globally on GTK, please use a NavigationPage.");
        }

        internal class DefaultRenderer : VisualElementRenderer<VisualElement, Widget>
        {

        }
    }
}