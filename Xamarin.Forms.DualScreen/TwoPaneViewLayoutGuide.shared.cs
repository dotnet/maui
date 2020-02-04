using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.DualScreen
{
    internal class TwoPaneViewLayoutGuide : INotifyPropertyChanged
    {
        public static TwoPaneViewLayoutGuide Instance => _twoPaneViewLayoutGuide.Value;
        static Lazy<TwoPaneViewLayoutGuide> _twoPaneViewLayoutGuide = new Lazy<TwoPaneViewLayoutGuide>(() => new TwoPaneViewLayoutGuide());

        IDualScreenService DualScreenService =>
            DependencyService.Get<IDualScreenService>() ?? NoDualScreenServiceImpl.Instance;

        Rectangle _hinge;
        Rectangle _leftPage;
        Rectangle _rightPane;
        TwoPaneViewMode _mode;
        Layout _layout;
        bool _isLandscape;
        public event PropertyChangedEventHandler PropertyChanged;
        List<string> _pendingPropertyChanges = new List<string>();

        TwoPaneViewLayoutGuide()
        {
        }

        public TwoPaneViewLayoutGuide(Layout layout)
        {
            _layout = layout;
        }

        public void WatchForChanges()
        {
            StopWatchingForChanges();
            DualScreenService.OnScreenChanged += OnScreenChanged;

            if (_layout != null)
            {
                _layout.SizeChanged += OnLayoutChanged;
            }
            if (Device.Info is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += OnDeviceInfoChanged;
            }
        }

        public void StopWatchingForChanges()
        {
            DualScreenService.OnScreenChanged -= OnScreenChanged;

            if (_layout != null)
            {
                _layout.SizeChanged -= OnLayoutChanged;
            }
            if (Device.Info is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged -= OnDeviceInfoChanged;
            }
        }

        void OnLayoutChanged(object sender, EventArgs e)
        {
            UpdateLayouts();
        }

        void OnScreenChanged(object sender, EventArgs e)
        {
            UpdateLayouts();
        }

        void OnDeviceInfoChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Device.Info.CurrentOrientation))
            {
                UpdateLayouts();
            }
        }

        public bool IsLandscape
        {
            get => DualScreenService.IsLandscape;
            set => SetProperty(ref _isLandscape, value);
        }

        public TwoPaneViewMode Mode
        {
            get
            {
                return GetTwoPaneViewMode();
            }
            private set
            {
                SetProperty(ref _mode, value);
            }
        }

        public Rectangle Pane1
        {
            get
            {
                return _leftPage;
            }
            private set
            {
                SetProperty(ref _leftPage, value);
            }
        }

        public Rectangle Pane2
        {
            get
            {
                return _rightPane;
            }
            private set
            {
                SetProperty(ref _rightPane, value);
            }
        }

        public Rectangle Hinge
        {
            get
            {
                return DualScreenService.GetHinge();
            }
            private set
            {
                SetProperty(ref _hinge, value);
            }
        }

        internal void UpdateLayouts()
        {
            Rectangle containerArea;
            if (_layout == null)
            {
                containerArea = new Rectangle(Point.Zero, Device.info.ScaledScreenSize);
            }
            else
            {
                containerArea = _layout.Bounds;
            }

            if (containerArea.Width <= 0)
            {
                return;
            }

            Rectangle _newPane1 = Pane1;
            Rectangle _newPane2 = Pane2;

            if (!DualScreenService.IsLandscape)
            {
                if (DualScreenService.IsSpanned)
                {
                    var paneWidth = (containerArea.Width - Hinge.Width) / 2;
                    _newPane1 = new Rectangle(0, 0, paneWidth, containerArea.Height);
                    _newPane2 = new Rectangle(paneWidth + Hinge.Width, 0, paneWidth, Pane1.Height);
                }
                else
                {
                    _newPane1 = new Rectangle(0, 0, containerArea.Width, containerArea.Height);
                    _newPane2 = Rectangle.Zero;
                }
            }
            else
            {
                if (DualScreenService.IsSpanned)
				{
					Point displayedScreenAbsCoordinates = Point.Zero;

					if (_layout != null)
						displayedScreenAbsCoordinates = DualScreenService.GetLocationOnScreen(_layout) ?? Point.Zero;

					var screenSize = Device.info.ScaledScreenSize;
                    var topStuffHeight = displayedScreenAbsCoordinates.Y;
                    var bottomStuffHeight = screenSize.Height - topStuffHeight - containerArea.Height;
                    var paneWidth = containerArea.Width;
                    var leftPaneHeight = Hinge.Y - topStuffHeight;
                    var rightPaneHeight = screenSize.Height - topStuffHeight - leftPaneHeight - bottomStuffHeight - Hinge.Height;

                    _newPane1 = new Rectangle(0, 0, paneWidth, leftPaneHeight);
                    _newPane2 = new Rectangle(0, Hinge.Y + Hinge.Height - topStuffHeight, paneWidth, rightPaneHeight);
                }
                else
                {
                    _newPane1 = new Rectangle(0, 0, containerArea.Width, containerArea.Height);
                    _newPane2 = Rectangle.Zero;
                }
            }

            if (_newPane2.Height < 0 || _newPane2.Width < 0)
                _newPane2 = Rectangle.Zero;

            if (_newPane1.Height < 0 || _newPane1.Width < 0)
                _newPane1 = Rectangle.Zero;

            Pane1 = _newPane1;
            Pane2 = _newPane2;
            Mode = GetTwoPaneViewMode();
            Hinge = DualScreenService.GetHinge();
            IsLandscape = DualScreenService.IsLandscape;

            var properties = _pendingPropertyChanges.ToList();
            _pendingPropertyChanges.Clear();

            foreach(var property in properties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        TwoPaneViewMode GetTwoPaneViewMode()
        {
            if (!DualScreenService.IsSpanned)
                return TwoPaneViewMode.SinglePane;

            if (DualScreenService.IsLandscape)
                return TwoPaneViewMode.Tall;

            return TwoPaneViewMode.Wide;
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            _pendingPropertyChanges.Add(propertyName);
            return true;
        }
    }
}