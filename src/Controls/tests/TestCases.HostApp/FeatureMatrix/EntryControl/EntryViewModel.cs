using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace Maui.Controls.Sample
{
    public class EntryViewModel : INotifyPropertyChanged
    {
        private string _text = "Test Entry";
        private Color _textColor = Colors.Black;
        private string _placeholder = "Enter text here";
        private Color _placeholderColor = Colors.Gray;
        private double _fontSize = 14;
        private TextAlignment _horizontalTextAlignment = TextAlignment.Start;
        private TextAlignment _verticalTextAlignment = TextAlignment.Center;
        private bool _isPassword = false;
        private double _characterSpacing = 0;
        private ReturnType _returnType = ReturnType.Default;
        private int _maxLength = -1;
        private bool _isCursorVisible = false;
        private int _cursorPosition = 0;
        private int _selectionLength = 0;
        private bool _isReadOnly = false;
        private bool _isTextPredictionEnabled = false;
        private bool _isSpellCheckEnabled = false;
        private Keyboard _keyboard = Keyboard.Default;
        private string _fontFamily = null;
        private bool isVisible = true;
        private bool _isEnabled = true;
        private ClearButtonVisibility _clearButtonVisibility = ClearButtonVisibility.WhileEditing;
        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ReturnCommand { get; set; }
        public EntryViewModel()
        {
            ReturnCommand = new Command<string>(
                execute: (entryText) =>
                {
                    if (entryText == "Test")
                    {
                        Text = "Command Executed with Parameter";
                    }
                }
            );
        }
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

        public string Placeholder
        {
            get => _placeholder;
            set { _placeholder = value; OnPropertyChanged(); }
        }
        public Color PlaceholderColor
        {
            get => _placeholderColor;
            set { _placeholderColor = value; OnPropertyChanged(); }
        }

        public ClearButtonVisibility ClearButtonVisibility
        {
            get => _clearButtonVisibility;
            set { _clearButtonVisibility = value; OnPropertyChanged(); }
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

        public bool IsCursorVisible
        {
            get => _isCursorVisible;
            set { _isCursorVisible = value; OnPropertyChanged(); }
        }
        public int CursorPosition
        {
            get => _cursorPosition;
            set { _cursorPosition = value; OnPropertyChanged(); }
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

        public bool IsVisible
        {
            get => isVisible;
            set { isVisible = value; OnPropertyChanged(); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        public Keyboard Keyboard
        {
            get => _keyboard;
            set { _keyboard = value; OnPropertyChanged(); }
        }

        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set { _flowDirection = value; OnPropertyChanged(); }
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