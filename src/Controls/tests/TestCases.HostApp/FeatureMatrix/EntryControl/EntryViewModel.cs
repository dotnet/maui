using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public class EntryViewModel : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        private Color _textColor = Colors.Black;
        private double _fontSize = 14;
        private TextAlignment _horizontalTextAlignment = TextAlignment.Start;
        private TextAlignment _verticalTextAlignment = TextAlignment.Start;
        private bool _isPassword = false;
        private double _characterSpacing = 0;
        private ReturnType _returnType = ReturnType.Default;
        private int _maxLength = -1;
        private bool _cursorVisible = false;
        private int _cursorPosition = 0;
        private int _selectionLength = 0;
        private bool _isReadOnly = false;
        private bool _isTextPredictionEnabled = false;
        private bool _isSpellCheckEnabled = false;
        private Keyboard _keyboard = Keyboard.Default;
        private bool _fontAutoScalingEnabled = false;
        private string _fontFamily = null;
        public event PropertyChangedEventHandler PropertyChanged;


        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; OnPropertyChanged(); }
        }

        public double FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(); }
        }

        public TextAlignment HorizontalTextAlignment
        {
            get => _horizontalTextAlignment;
            set { _horizontalTextAlignment = value; OnPropertyChanged(); }
        }

        public TextAlignment VerticalTextAlignment
        {
            get => _verticalTextAlignment;
            set { _verticalTextAlignment = value; OnPropertyChanged(); }
        }

        public bool IsPassword
        {
            get => _isPassword;
            set { _isPassword = value; OnPropertyChanged(); }
        }

        public double CharacterSpacing
        {
            get => _characterSpacing;
            set { _characterSpacing = value; OnPropertyChanged(); }
        }

        public ReturnType ReturnType
        {
            get => _returnType;
            set { _returnType = value; OnPropertyChanged(); }
        }

        public int MaxLength
        {
            get => _maxLength;
            set { _maxLength = value; OnPropertyChanged(); }
        }
        public int CursorPosition
        {
            get => _cursorPosition;
            set { _cursorPosition = value; OnPropertyChanged(); }
        }
        public bool IsCursorVisible
        {
            get => _cursorVisible;
            set { _cursorVisible = value; OnPropertyChanged(); }
        }
        public int SelectionLength
        {
            get => _selectionLength;
            set { _selectionLength = value; OnPropertyChanged(); }
        }
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(); }
        }
        public bool IsTextPredictionEnabled
        {
            get => _isTextPredictionEnabled;
            set { _isTextPredictionEnabled = value; OnPropertyChanged(); }
        }
        public bool IsSpellCheckEnabled
        {
            get => _isSpellCheckEnabled;
            set { _isSpellCheckEnabled = value; OnPropertyChanged(); }
        }
        public Keyboard Keyboard
        {
            get => _keyboard;
            set { _keyboard = value; OnPropertyChanged(); }
        }
        public bool FontAutoScalingEnabled
        {
            get => _fontAutoScalingEnabled;
            set { _fontAutoScalingEnabled = value; OnPropertyChanged(); }
        }

        public string FontFamily
        {
            get => _fontFamily;
            set { _fontFamily = value; OnPropertyChanged(); }
        }

       protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}