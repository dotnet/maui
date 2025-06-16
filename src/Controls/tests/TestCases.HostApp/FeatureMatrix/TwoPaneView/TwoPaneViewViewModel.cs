using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Foldable;

namespace Maui.Controls.Sample
{
    public class TwoPaneViewViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private GridLength _pane1Length = new GridLength(1, GridUnitType.Star);
        private GridLength _pane2Length = new GridLength(1, GridUnitType.Star);
        private double _minTallModeHeight = 500;
        private double _minWideModeWidth = 700;
        private bool _isShadowEnabled = true;
        private bool _isVisible = true;
        private FlowDirection _flowDirection = FlowDirection.LeftToRight;

        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set
            {
                if (_flowDirection != value)
                {
                    _flowDirection = value;
                    OnPropertyChanged();
                }
            }
        }

        public GridLength Pane1Length
        {
            get => _pane1Length;
            set
            {
                _pane1Length = value;
                OnPropertyChanged();
            }
        }
        public GridLength Pane2Length
        {
            get => _pane2Length;
            set
            {
                _pane2Length = value;
                OnPropertyChanged();
            }
        }
        public double MinTallModeHeight
        {
            get => _minTallModeHeight;
            set
            {
                _minTallModeHeight = value;
                OnPropertyChanged();
            }
        }
        public double MinWideModeWidth
        {
            get => _minWideModeWidth;
            set
            {
                _minWideModeWidth = value;
                OnPropertyChanged();
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}