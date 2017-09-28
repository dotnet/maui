using Gdk;
using Gtk;
using System;
using Xamarin.Forms.Platform.GTK.Animations;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public enum MasterBehaviorType
    {
        Default = 0,
        Popover,
        Split
    }

    public class MasterDetailPage : Fixed
    {
        private const int DefaultMasterWidth = 300;
        private const int IsPresentedAnimationMilliseconds = 300;

        private Gdk.Rectangle _lastAllocation;
        private bool _isPresented;
        private MasterDetailMasterTitleContainer _titleContainer;
        private EventBox _masterContainerWrapper;
        private VBox _masterContainer;
        private Widget _master;
        private Widget _detail;
        private MasterBehaviorType _masterBehaviorType;
        private static Pixbuf _hamburgerPixBuf;
        private bool _displayTitle;
        private bool _animationsEnabled;

        public MasterDetailPage()
        {
            _animationsEnabled = false;
            _masterBehaviorType = MasterBehaviorType.Default;

            // Master Stuff
            _masterContainerWrapper = new EventBox();
            _masterContainer = new VBox(false, 0);
            _titleContainer = new MasterDetailMasterTitleContainer();
            _titleContainer.HamburguerClicked += OnHamburgerClicked;
            _titleContainer.HeightRequest = GtkToolbarConstants.ToolbarHeight;
            _masterContainer.PackStart(_titleContainer, false, true, 0);

            _master = new EventBox();
            _masterContainer.PackEnd(_master, false, true, 0);
            _masterContainerWrapper.Add(_masterContainer);

            // Detail Stuff
            _detail = new EventBox();

            Add(_detail);
            Add(_masterContainerWrapper);
        }

        public MasterBehaviorType MasterBehaviorType
        {
            get
            {
                return _masterBehaviorType;
            }

            set
            {
                if (_masterBehaviorType != value)
                {
                    _masterBehaviorType = value;
                    RefreshMasterBehavior(_masterBehaviorType);
                }
            }
        }

        public Widget Master
        {
            get
            {
                return _master;
            }

            set
            {
                RefreshMaster(value);
            }
        }

        public Widget Detail
        {
            get
            {
                return _detail;
            }

            set
            {
                RefreshDetail(value);
            }
        }

        public bool IsPresented
        {
            get
            {
                return _isPresented;
            }

            set
            {
                RefreshPresented(value);
                NotifyIsPresentedChanged();
            }
        }

        public string MasterTitle
        {
            get
            {
                return _titleContainer.Title;
            }

            set
            {
                _titleContainer.Title = value ?? string.Empty;
            }
        }

        public bool DisplayTitle
        {
            get
            {
                return _displayTitle;
            }

            set
            {
                RefreshDisplayTitle(value);
            }
        }

        public static Pixbuf HamburgerPixBuf
        {
            get
            {
                try
                {
                    if (_hamburgerPixBuf == null)
                    {
                        _hamburgerPixBuf = new Pixbuf("./Resources/hamburger.png");
                    }

                    return _hamburgerPixBuf;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                _hamburgerPixBuf = value;
            }
        }

        public void UpdateBarTextColor(Gdk.Color? barTextColor)
        {
            if (_titleContainer != null)
            {
                _titleContainer.UpdateTitleColor(barTextColor);
            }
        }

        public void UpdateBarBackgroundColor(Gdk.Color? barBackgroundColor)
        {
            if (_titleContainer != null)
            {
                _titleContainer.UpdateBackgroundColor(barBackgroundColor);
            }
        }

        public void UpdateHamburguerIcon(Pixbuf hamburguerIcon)
        {
            HamburgerPixBuf = hamburguerIcon;

            if (_titleContainer != null)
            {
                _titleContainer.HamburgerPixBuf = HamburgerPixBuf;
            }
        }

        public event EventHandler IsPresentedChanged;

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            if (_lastAllocation != allocation)
            {
                _lastAllocation = allocation;
            }

            _master.WidthRequest = DefaultMasterWidth;
            _master.HeightRequest = _detail.HeightRequest = allocation.Height;
            RefreshMasterBehavior(_masterBehaviorType);
        }

        protected override void OnShown()
        {
            base.OnShown();

            _animationsEnabled = true;
        }


        private void RefreshMasterBehavior(MasterBehaviorType masterBehaviorType)
        {
            int detailWidthRequest = 0;
            Gdk.Point point = default(Gdk.Point);

            switch (_masterBehaviorType)
            {
                case MasterBehaviorType.Split:
                    detailWidthRequest = _lastAllocation.Width - DefaultMasterWidth;
                    point = new Gdk.Point(_master.WidthRequest, 0);
                    break;
                case MasterBehaviorType.Default:
                case MasterBehaviorType.Popover:
                    detailWidthRequest = _lastAllocation.Width;
                    point = new Gdk.Point(0, 0);
                    break;
            }

            if (detailWidthRequest >= 0)
            {
                _detail.WidthRequest = detailWidthRequest;
                _detail.MoveTo(point.X, point.Y);
            }
        }

        private void RefreshMaster(Widget newMaster)
        {
            if (_master != null)
            {
                _masterContainer.RemoveFromContainer(_master);
            }

            UpdateHamburguerIcon(HamburgerPixBuf);
            _master = newMaster;
            _masterContainer.PackEnd(newMaster, false, true, 0);
            _master.ShowAll();
        }

        private void RefreshDetail(Widget newDetail)
        {
            if (_detail != null)
            {
                this.RemoveFromContainer(_detail);
            }

            _detail = newDetail;

            Add(_detail);

            Remove(_masterContainerWrapper);
            Add(_masterContainerWrapper);

            _detail.ShowAll();
            _masterContainerWrapper.GdkWindow?.Raise(); // Forcing Master to be on top
        }

        private async void RefreshPresented(bool isPresented)
        {
            _isPresented = isPresented;

            if (_masterBehaviorType == MasterBehaviorType.Split) return;

            if (_animationsEnabled)
            {
                var from = (_isPresented) ? -DefaultMasterWidth : 0;
                var to = (_isPresented) ? 0 : -DefaultMasterWidth;

                await new FloatAnimation(from, to, TimeSpan.FromMilliseconds(IsPresentedAnimationMilliseconds), true, (f) =>
                {
                    Gtk.Application.Invoke(delegate
                    {
                        _masterContainerWrapper.MoveTo(f, 0);
                    });
                }).Run();
            }
            else
            {
                _masterContainerWrapper.MoveTo(_isPresented ? 0 : -DefaultMasterWidth, 0);
            }
        }

        private void RefreshDisplayTitle(bool value = true)
        {
            _displayTitle = value;

            _masterContainer.RemoveFromContainer(_titleContainer);

            if (_displayTitle)
            {
                _masterContainer.PackStart(_titleContainer, false, true, 0);
            }
        }

        private void OnHamburgerClicked(object sender, EventArgs e)
        {
            IsPresented = !IsPresented;
        }

        private void NotifyIsPresentedChanged()
        {
            IsPresentedChanged?.Invoke(this, EventArgs.Empty);
        }

        private class MasterDetailMasterTitleContainer : EventBox
        {
            private HBox _root;
            private ToolButton _hamburguerButton;
            private Gtk.Label _titleLabel;
            private Gtk.Image _hamburguerIcon;
            private Gdk.Color _defaultTextColor;
            private Gdk.Color _defaultBackgroundColor;

            public MasterDetailMasterTitleContainer()
            {
                _defaultBackgroundColor = Style.Backgrounds[(int)StateType.Normal];

                _root = new HBox();
                _hamburguerIcon = new Gtk.Image();

                try
                {
                    _hamburguerIcon = new Gtk.Image(HamburgerPixBuf);
                }
                catch (Exception ex)
                {
                    Internals.Log.Warning("MasterDetailPage HamburguerIcon", "Could not load hamburguer icon: {0}", ex);
                }

                _hamburguerButton = new ToolButton(_hamburguerIcon, string.Empty);
                _hamburguerButton.HeightRequest = GtkToolbarConstants.ToolbarItemHeight;
                _hamburguerButton.WidthRequest = GtkToolbarConstants.ToolbarItemWidth;
                _hamburguerButton.Clicked += OnHamburguerButtonClicked;

                _titleLabel = new Gtk.Label();
                _defaultTextColor = _titleLabel.Style.Foregrounds[(int)StateType.Normal];

                _root.PackStart(_hamburguerButton, false, false, GtkToolbarConstants.ToolbarItemSpacing);
                _root.PackStart(_titleLabel, false, false, 25);

                Add(_root);
            }

            public string Title
            {
                get
                {
                    return _titleLabel.Text;
                }

                set
                {
                    _titleLabel.Text = value ?? string.Empty;
                }
            }

            public Pixbuf HamburgerPixBuf
            {
                get
                {
                    return _hamburguerIcon.Pixbuf;
                }

                set
                {
                    _hamburguerIcon.Pixbuf = value ?? null;
                }
            }

            public void UpdateTitleColor(Gdk.Color? titleColor)
            {
                if (_titleLabel != null)
                {
                    if (titleColor.HasValue)
                    {
                        _titleLabel.ModifyFg(StateType.Normal, titleColor.Value);
                    }
                    else
                    {
                        _titleLabel.ModifyFg(StateType.Normal, _defaultTextColor);
                    }
                }
            }

            public void UpdateBackgroundColor(Gdk.Color? backgroundColor)
            {
                if(_root== null)
                {
                    return;
                }

                if (backgroundColor.HasValue)
                {
                    ModifyBg(StateType.Normal, backgroundColor.Value);
                    _root.ModifyBg(StateType.Normal, backgroundColor.Value);
                }
                else
                {
                    ModifyBg(StateType.Normal, _defaultBackgroundColor);
                    _root.ModifyBg(StateType.Normal, _defaultBackgroundColor);
                }
            }

            public event EventHandler HamburguerClicked;

            private void OnHamburguerButtonClicked(object sender, EventArgs e)
            {
                HamburguerClicked?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}