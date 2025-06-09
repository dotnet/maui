using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public class ProgressBarViewModel : INotifyPropertyChanged
    {
        private double _progress = 0.5;
        private Color _progressColor = Colors.Green;
        private Color _backgroundColor = Colors.Gray;
        private bool _isVisible = true;
        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        private float _shadowOpacity = 0f;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Progress
        {
            get => _progress;
            set { if (_progress != value) { _progress = value; OnPropertyChanged(); } }
        }
        public Color ProgressColor
        {
            get => _progressColor;
            set { if (_progressColor != value) { _progressColor = value; OnPropertyChanged(); } }
        }
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set { if (_backgroundColor != value) { _backgroundColor = value; OnPropertyChanged(); } }
        }
        public bool IsVisible
        {
            get => _isVisible;
            set { if (_isVisible != value) { _isVisible = value; OnPropertyChanged(); } }
        }
        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set { if (_flowDirection != value) { _flowDirection = value; OnPropertyChanged(); } }
        }
        public float ShadowOpacity
        {
            get => _shadowOpacity;
            set { if (_shadowOpacity != value) { _shadowOpacity = value; OnPropertyChanged(); } }
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
