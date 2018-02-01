using Gtk;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK.Controls;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class MasterDetailPageRenderer : AbstractPageRenderer<Controls.MasterDetailPage, MasterDetailPage>
    {
        public MasterDetailPageRenderer()
        {
            MessagingCenter.Subscribe(this, Forms.BarTextColor, (NavigationPage sender, Color color) =>
            {
                var barTextColor = color;

                if (barTextColor == null || barTextColor.IsDefaultOrTransparent())
                {
                    Widget.UpdateBarTextColor(null);
                }
                else
                {
                    Widget.UpdateBarTextColor(color.ToGtkColor());
                }
            });

            MessagingCenter.Subscribe(this, Forms.BarBackgroundColor, (NavigationPage sender, Color color) =>
            {
                var barBackgroundColor = color;

                if (barBackgroundColor == null || barBackgroundColor.IsDefaultOrTransparent())
                {
                    Widget.UpdateBarBackgroundColor(null);
                }
                else
                {
                    Widget.UpdateBarBackgroundColor(color.ToGtkColor());
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Widget != null)
                {
                    Widget.IsPresentedChanged -= OnIsPresentedChanged;
                }

                MessagingCenter.Unsubscribe<NavigationPage, Color>(this, Forms.BarTextColor);
                MessagingCenter.Unsubscribe<NavigationPage, Color>(this, Forms.BarBackgroundColor);

                if (Page?.Master != null)
                {
                    Page.Master.PropertyChanged -= HandleMasterPropertyChanged;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Widget == null)
                {
                    // There is nothing similar in Gtk. 
                    // Custom control has been created that simulates the expected behavior.
                    Widget = new Controls.MasterDetailPage();
                    var eventBox = new EventBox();
                    eventBox.Add(Widget);

                    Control.Content = eventBox;

                    Widget.IsPresentedChanged += OnIsPresentedChanged;

                    UpdateMasterDetail();
                    UpdateMasterBehavior();
                    UpdateIsPresented();
                    UpdateBarTextColor();
                    UpdateBarBackgroundColor();
                }
            }
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            Control?.Content?.SetSize(allocation.Width, allocation.Height);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName.Equals(nameof(MasterDetailPage.Master)) || e.PropertyName.Equals(nameof(MasterDetailPage.Detail)))
            {
                UpdateMasterDetail();
                UpdateMasterBehavior();
                UpdateIsPresented();
            }
            else if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
                UpdateIsPresented();
            else if (e.PropertyName == MasterDetailPage.MasterBehaviorProperty.PropertyName)
                UpdateMasterBehavior();
        }

        private async void HandleMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Xamarin.Forms.Page.IconProperty.PropertyName)
                await UpdateHamburguerIconAsync();
        }

        private void UpdateMasterDetail()
        {
            Gtk.Application.Invoke(async delegate
            {
                Page.Master.PropertyChanged -= HandleMasterPropertyChanged;
                await UpdateHamburguerIconAsync();

                if (Platform.GetRenderer(Page.Master) == null)
                    Platform.SetRenderer(Page.Master, Platform.CreateRenderer(Page.Master));
                if (Platform.GetRenderer(Page.Detail) == null)
                    Platform.SetRenderer(Page.Detail, Platform.CreateRenderer(Page.Detail));

                Widget.Master = Platform.GetRenderer(Page.Master).Container;
                Widget.Detail = Platform.GetRenderer(Page.Detail).Container;
                Widget.MasterTitle = Page.Master?.Title ?? string.Empty;

                UpdateBarTextColor();
                UpdateBarBackgroundColor();

                Page.Master.PropertyChanged += HandleMasterPropertyChanged;
            });
        }

        private void UpdateIsPresented()
        {
            Widget.IsPresented = Page.IsPresented;
        }

        private void UpdateMasterBehavior()
        {
            if (Page.Detail is NavigationPage)
            {
                Widget.MasterBehaviorType = GetMasterBehavior(Page.MasterBehavior);
            }
            else
            {
                // The only way to display Master page is from a toolbar. If we have not access to one,
                // we should force split mode to display menu (as no gestures are implemented).
                Widget.MasterBehaviorType = MasterBehaviorType.Split;
            }

            Widget.DisplayTitle = Widget.MasterBehaviorType != MasterBehaviorType.Split;
        }

        private void UpdateBarTextColor()
        {
            var navigationPage = Page.Detail as NavigationPage;

            if (navigationPage != null)
            {
                var barTextColor = navigationPage.BarTextColor;

                Widget.UpdateBarTextColor(barTextColor.ToGtkColor());
            }
        }

        private void UpdateBarBackgroundColor()
        {
            var navigationPage = Page.Detail as NavigationPage;

            if (navigationPage != null)
            {
                var barBackgroundColor = navigationPage.BarBackgroundColor;
                Widget.UpdateBarBackgroundColor(barBackgroundColor.ToGtkColor());
            }
        }

        private async Task UpdateHamburguerIconAsync()
        {
            var hamburguerIcon = Page.Master.Icon;

            if (hamburguerIcon != null)
            {
                IImageSourceHandler handler =
                    Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(hamburguerIcon);

                var image = await handler.LoadImageAsync(hamburguerIcon);
                Widget.UpdateHamburguerIcon(image);

                var navigationPage = Page.Detail as NavigationPage;

                if (navigationPage != null)
                {
                    var navigationRenderer = Platform.GetRenderer(navigationPage) as IToolbarTracker;
                    navigationRenderer?.NativeToolbarTracker.UpdateToolBar();
                }
            }
        }

        private MasterBehaviorType GetMasterBehavior(MasterBehavior masterBehavior)
        {
            switch (masterBehavior)
            {
                case MasterBehavior.Split:
                case MasterBehavior.SplitOnLandscape:
                case MasterBehavior.SplitOnPortrait:
                    return MasterBehaviorType.Split;
                case MasterBehavior.Popover:
                    return MasterBehaviorType.Popover;
                case MasterBehavior.Default:
                    return MasterBehaviorType.Default;
                default:
                    throw new ArgumentOutOfRangeException(nameof(masterBehavior));
            }
        }

        private void OnIsPresentedChanged(object sender, EventArgs e)
        {
            ElementController.SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, Widget.IsPresented);
        }
    }
}