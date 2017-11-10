using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK.Controls;
using Xamarin.Forms.Platform.GTK.Extensions;
using Xamarin.Forms.Platform.GTK.Renderers;

namespace Xamarin.Forms.Platform.GTK
{
    public class GtkToolbarTracker
    {
        private readonly string _defaultBackButtonTitle = "Back";

        private readonly ToolbarTracker _toolbarTracker;
        private HBox _toolbar;
        private HBox _toolbarNavigationSection;
        private Alignment _toolbarTitleSectionWrapper;
        private HBox _toolbarTitleSection;
        private HBox _toolbarSection;
        private Gtk.Label _toolbarTitle;
        private ImageControl _toolbarIcon;
        private NavigationPage _navigation;
        private string _backButton;

        private MasterDetailPage _parentMasterDetailPage;

        public GtkToolbarTracker()
        {
            _toolbarTracker = new ToolbarTracker();
            _toolbarTracker.CollectionChanged += ToolbarTrackerOnCollectionChanged;
        }

        public HBox Toolbar
        {
            get { return _toolbar; }
        }

        public NavigationPage Navigation
        {
            get { return _navigation; }
            set
            {
                if (_navigation == value)
                    return;

                if (_navigation != null)
                    _navigation.PropertyChanged -= NavigationPagePropertyChanged;

                _navigation = value;

                if (_navigation != null)
                {
                    _toolbarTracker.Target = _navigation.CurrentPage;
                    _navigation.PropertyChanged += NavigationPagePropertyChanged;
                }

                UpdateToolBar();
            }
        }

        public void TryHide(NavigationPage navPage = null)
        {
            if (navPage == null || navPage == _navigation)
            {
                Navigation = null;
            }
        }

        public void UpdateIcon()
        {
            if (_toolbar == null || _navigation == null)
                return;

            var iconPath = GetCurrentPageIconPath();

            if (!string.IsNullOrEmpty(iconPath))
            {
                _toolbarIcon.Pixbuf = new Pixbuf(iconPath);
                _toolbarIcon.SetSizeRequest(GtkToolbarConstants.ToolbarIconWidth, GtkToolbarConstants.ToolbarIconHeight);
            }
            else
            {
                _toolbarIcon.WidthRequest = 1;
            }
        }

        public void UpdateTitle()
        {
            if (_toolbar == null || _navigation == null)
                return;

            var title = GetCurrentPageTitle();

            if (_toolbarTitle != null)
            {
                var span = new Span()
                {
                    FontSize = 12.0d,
                    Text = title ?? string.Empty
                };

                _toolbarTitle.SetTextFromSpan(span);
            }
        }

        protected virtual HBox ConfigureToolbar()
        {
            var toolbar = new HBox();
            toolbar.HeightRequest = GtkToolbarConstants.ToolbarHeight;

            _toolbarNavigationSection = new HBox();
            toolbar.PackStart(_toolbarNavigationSection, false, true, 0);

            _toolbarTitleSectionWrapper = new Alignment(0f, 0.5f, 0, 0);
            _toolbarTitleSection = new HBox();
            _toolbarTitleSectionWrapper.Add(_toolbarTitleSection);
            toolbar.PackStart(_toolbarTitleSectionWrapper, true, true, 0);

            _toolbarSection = new HBox();
            toolbar.PackStart(_toolbarSection, false, true, 0);

            return toolbar;
        }

        private void ToolbarTrackerOnCollectionChanged(object sender, EventArgs eventArgs)
        {
            UpdateToolbarItems();
        }

        private void UpdateToolbarItems()
        {
            if (_toolbar == null || _navigation == null)
                return;

            var currentPage = _navigation.Peek(0);
            UpdateItems(currentPage.ToolbarItems);
        }

        private void NavigationPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(NavigationPage.BarTextColorProperty.PropertyName) ||
                e.PropertyName.Equals(NavigationPage.BarBackgroundColorProperty.PropertyName) ||
                e.PropertyName.Equals(Page.TitleProperty.PropertyName) ||
                e.PropertyName.Equals(Page.IconProperty.PropertyName))
                UpdateToolBar();
        }
        
        private string GetCurrentPageTitle()
        {
            if (_navigation == null)
                return string.Empty;
            return _navigation.Peek(0).Title ?? string.Empty;
        }

        private string GetCurrentPageIconPath()
        {
            if (_navigation == null)
                return string.Empty;
            return _navigation.Peek(0).Icon ?? string.Empty;
        }

        private void UpdateBarBackgroundColor(Controls.Page page)
        {
            if (Navigation != null)
            {
                if (Navigation.BarBackgroundColor.IsDefaultOrTransparent())
                {
                    page?.SetToolbarColor(null);
                }
                else
                {
                    var backgroundColor = Navigation.BarBackgroundColor.ToGtkColor();
                    page?.SetToolbarColor(backgroundColor);
                }
            }
        }

        private void UpdateBarTextColor()
        {
            if (Navigation != null && Navigation.BarTextColor != Color.Default)
            {
                var textColor = Navigation.BarTextColor.ToGtkColor();

                if (_toolbar != null && _toolbarTitle != null)
                {
                    _toolbarTitle.ModifyFg(StateType.Normal, textColor);
                }
            }
        }

        private void UpdateBarBackgroundColor()
        {
            var currentPage = _navigation.Peek(0);
            var pageRenderer = Platform.GetRenderer(currentPage);

            if (pageRenderer != null && pageRenderer.Disposed)
            {
                return;
            }

            var page = pageRenderer as IPageControl;

            if (page?.Control != null)
            {
                page.Control.Toolbar = _toolbar;
                UpdateBarBackgroundColor(page.Control);
            }
        }

        private void UpdateItems(IList<ToolbarItem> toolBarItems)
        {
            foreach (var child in _toolbarSection.Children)
            {
                _toolbarSection.Remove(child);
            }

            foreach (var toolBarItem in toolBarItems.Where(t => t.Order != ToolbarItemOrder.Secondary))
            {
                ToolButton newToolButton = ToolButtonHelper.CreateToolButton(toolBarItem);
                _toolbarSection.PackStart(newToolButton, false, false, GtkToolbarConstants.ToolbarItemSpacing);
                newToolButton.Clicked += (sender, args) => { toolBarItem.Activate(); };

                toolBarItem.PropertyChanged -= OnToolbarItemPropertyChanged;
                toolBarItem.PropertyChanged += OnToolbarItemPropertyChanged;
            }

            var secondaryToolBarItems = toolBarItems.Where(t => t.Order == ToolbarItemOrder.Secondary);

            if (secondaryToolBarItems.Any())
            {
                ToolButton secondaryButton = ToolButtonHelper.CreateToolButton(Stock.Add);
                _toolbarSection.PackStart(secondaryButton, false, false, 0);

				Gtk.Menu menu = new Gtk.Menu();
                foreach (var secondaryToolBarItem in secondaryToolBarItems)
                {
					Gtk.MenuItem menuItem = new Gtk.MenuItem(secondaryToolBarItem.Text)
					{
						Sensitive = secondaryToolBarItem.IsEnabled
					};
					menu.Add(menuItem);

                    menuItem.ButtonPressEvent += (sender, args) =>
                    {
                        secondaryToolBarItem.Activate();
                    };

                    secondaryToolBarItem.PropertyChanged -= OnToolbarItemPropertyChanged;
                    secondaryToolBarItem.PropertyChanged += OnToolbarItemPropertyChanged;
                }

                secondaryButton.Clicked += (sender, args) =>
                {
                    menu.ShowAll();
                    menu.Popup();
                };
            }

            _toolbarSection.ShowAll();
        }

        private bool ShowBackButton()
        {
            if (_navigation == null)
                return false;

            return NavigationPage.GetHasBackButton(_navigation.CurrentPage)
                && !IsRootPage();
        }

        private bool IsRootPage()
        {
            if (_navigation == null)
                return true;

            return _navigation.StackDepth <= 1;
        }

        private void UpdateNavigationItems()
        {
            if (_toolbar == null || _navigation == null)
                return;

            if (ShowBackButton())
            {
                ToolButton navigationButton;

                if (string.IsNullOrEmpty(_backButton))
                {
                    navigationButton = ToolButtonHelper.CreateToolButton(Stock.GoBack);
                }
                else
                {
                    navigationButton = ToolButtonHelper.CreateToolButton(_backButton, string.Empty);
                }

                navigationButton.TooltipText = GetPreviousPageTitle() ?? string.Empty;
                navigationButton.WidthRequest = GtkToolbarConstants.BackButtonItemWidth;
                _toolbarNavigationSection.PackStart(navigationButton, false, false, GtkToolbarConstants.ToolbarItemSpacing);

                navigationButton.Clicked += async (sender, args) =>
                {
                    await NavigateBackFromBackButton();
                };
            }
            else if (_parentMasterDetailPage != null && _parentMasterDetailPage.ShouldShowToolbarButton())
            {
                ToolButton hamburguerButton = new ToolButton(null, string.Empty);

                var hamburgerPixBuf = Controls.MasterDetailPage.HamburgerPixBuf;
                if (hamburgerPixBuf != null)
                {
                    var image = new Gtk.Image(hamburgerPixBuf);
                    hamburguerButton = new ToolButton(image, string.Empty);
                }

                hamburguerButton.HeightRequest = GtkToolbarConstants.ToolbarItemHeight;
                hamburguerButton.WidthRequest = GtkToolbarConstants.BackButtonItemWidth;
                _toolbarNavigationSection.PackStart(hamburguerButton, false, false, GtkToolbarConstants.ToolbarItemSpacing);

                hamburguerButton.Clicked += (sender, args) =>
                {
                    _parentMasterDetailPage.IsPresented = !_parentMasterDetailPage.IsPresented;
                };
            }
        }

        private string GetPreviousPageTitle()
        {
            if (_navigation == null || _navigation.StackDepth <= 1)
                return string.Empty;

            return _navigation.Peek(1).Title ?? _defaultBackButtonTitle;
        }

        private async Task NavigateBackFromBackButton()
        {
            var popAsyncInner = _navigation?.PopAsyncInner(true, true);

            if (popAsyncInner != null)
                await popAsyncInner;
        }

        private void FindParentMasterDetail()
        {
            var masterDetailPage = _navigation.GetParentsPath()
                                          .OfType<MasterDetailPage>()
                                          .Where(md => md.Detail == _navigation)
                                          .FirstOrDefault();

            _parentMasterDetailPage = masterDetailPage;
        }

        internal void UpdateBackButton(string backButton)
        {
            _backButton = backButton;
        }

        internal void UpdateToolBar()
        {
            if (_navigation == null)
            {
                if (_toolbar != null)
                    _toolbar.Visible = false;

                _toolbar = null;

                return;
            }

            var currentPage = _navigation.Peek(0);

            if (NavigationPage.GetHasNavigationBar(currentPage))
            {
                _toolbar = ConfigureToolbar();

                _toolbarIcon = new ImageControl
                {
                    WidthRequest = 1,
                    Aspect = ImageAspect.AspectFit
                };
                _toolbarTitleSection.PackStart(_toolbarIcon, false, false, 8);

                _toolbarTitle = new Gtk.Label();
                _toolbarTitleSection.PackEnd(_toolbarTitle, true, true, 0);

                FindParentMasterDetail();

                UpdateNavigationItems();
                UpdateTitle();
                UpdateIcon();
                UpdateToolbarItems();
                UpdateBarTextColor();
                UpdateBarBackgroundColor();
            }
            else
            {
                if (_toolbar != null)
                {
                    _toolbar.Visible = false;
                }
            }
        }

        private void OnToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName)
                UpdateToolbarItems();
            else if (e.PropertyName == MenuItem.TextProperty.PropertyName)
                UpdateToolbarItems();
            else if (e.PropertyName == MenuItem.IconProperty.PropertyName)
                UpdateToolbarItems();
        }

        static class ToolButtonHelper
        {
            public static ToolButton CreateToolButton(string stockId)
            {
                ToolButton button = new ToolButton(stockId);
                ApplyDefaultDimensions(button);

                return button;
            }

            public static ToolButton CreateToolButton(string iconFileName, string title)
            {
                var pixBuf = new Pixbuf(iconFileName);
                var image = new Gtk.Image(pixBuf);

                ToolButton button = new ToolButton(image, title);
                ApplyDefaultDimensions(button);

                return button;
            }

            public static ToolButton CreateToolButton(ToolbarItem item)
            {
                var pixBuf = item.Icon.ToPixbuf();
                Gtk.Image icon = pixBuf != null ? new Gtk.Image(pixBuf) : null;
                ToolButton button = new ToolButton(icon, item.Text);
                ApplyDefaultDimensions(button);
                button.TooltipText = item.Text ?? string.Empty;
                button.Sensitive = item.IsEnabled;

                return button;
            }

            private static void ApplyDefaultDimensions(ToolButton button)
            {
                button.HeightRequest = GtkToolbarConstants.ToolbarItemHeight;
                button.WidthRequest = GtkToolbarConstants.ToolbarItemWidth;
            }
        }
    }
}