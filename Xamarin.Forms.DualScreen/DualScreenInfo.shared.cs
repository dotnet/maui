using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.DualScreen
{
    public class DualScreenInfo : INotifyPropertyChanged
    {
        static Lazy<DualScreenInfo> _dualScreenInfo { get; } = new Lazy<DualScreenInfo>(OnCreate);
        public event PropertyChangedEventHandler PropertyChanged;
        Rectangle[] _spanningBounds;
        Rectangle _hingeBounds;
        bool _isLandscape;
        TwoPaneViewMode _spanMode;

        public static DualScreenInfo Current => _dualScreenInfo.Value;
        public Rectangle[] SpanningBounds
        {
            get => GetSpanningBounds();
            set
            {
                SetProperty(ref _spanningBounds, value);
            }
        }

        public Rectangle HingeBounds
        {
            get => GetHingeBounds();
            set
            {
                SetProperty(ref _hingeBounds, value);
            }
        }

        public bool IsLandscape
        {
            get => GetIsLandscape();
            set
            {
                SetProperty(ref _isLandscape, value);
            }
        }

        public TwoPaneViewMode SpanMode
        {
            get => GetSpanMode();
            set
            {
                SetProperty(ref _spanMode, value);
            }
        }


        Rectangle[] GetSpanningBounds()
        {
            var guide = TwoPaneViewLayoutGuide.Instance;
            var hinge = guide.Hinge;
            guide.UpdateLayouts();

            if (hinge == Rectangle.Zero)
                return new Rectangle[0];

            if (guide.Pane2 == Rectangle.Zero)
                return new Rectangle[0];

            return new[] { guide.Pane1, guide.Pane2 };
        }

        Rectangle GetHingeBounds()
        {
            var guide = TwoPaneViewLayoutGuide.Instance;
            guide.UpdateLayouts();
            return guide.Hinge;
        }

        bool GetIsLandscape() => TwoPaneViewLayoutGuide.Instance.IsLandscape;

        TwoPaneViewMode GetSpanMode() => TwoPaneViewLayoutGuide.Instance.Mode;

        static DualScreenInfo OnCreate()
        {
            DualScreenInfo dualScreenInfo = new DualScreenInfo();
            TwoPaneViewLayoutGuide.Instance.PropertyChanged += dualScreenInfo.OnTwoPaneViewLayoutGuideChanged;
            return dualScreenInfo;
        }

        void OnTwoPaneViewLayoutGuideChanged(object sender, PropertyChangedEventArgs e)
        {
            SpanningBounds = GetSpanningBounds();
            IsLandscape = GetIsLandscape();
            HingeBounds = GetHingeBounds();
            SpanMode = GetSpanMode();
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
