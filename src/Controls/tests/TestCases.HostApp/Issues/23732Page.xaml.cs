using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{
	public partial class _23732Page : ContentPage
	{
		public ObservableCollection<_23732IconIdModel> Models { get; } = new();
		public _23732Page()
		{
			InitializeComponent();
			BindingContext = this;

			string[] glyphs = [
			_23732Constants.FontAwesomeBell,
			_23732Constants.FontAwesomeHome,
			_23732Constants.FontAwesomeEditSquare,
			_23732Constants.FontAwesomeSearch,
			_23732Constants.FontAwesomeSetting,
			_23732Constants.FontAwesomeDownChevron,
			_23732Constants.FontAwesomeLeftChevron,
			];

			int i = 0;
			foreach (string glyph in glyphs)
			{
				Models.Add(new()
				{
					Name = $"Glyph {i++}",
					Glyph = glyph
				});
			}

		}

		public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(ContentPage), null, BindingMode.OneWay);
		public string Icon
		{
			get => (string)GetValue(IconProperty);
			set
			{
				SetValue(IconProperty, value);
				OnPropertyChanged();
			}
		}

		public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(ContentPage), null, BindingMode.OneWay);
		public string Label
		{
			get => (string)GetValue(LabelProperty);
			set
			{
				SetValue(LabelProperty, value);
				OnPropertyChanged();
			}
		}

	}

	public static class _23732Constants
	{
		public const string FontAwesomeHome = "\uf015";
		public const string FontAwesomeSearch = "\uf002";
		public const string FontAwesomeSetting = "\uf013";
		public const string FontAwesomeEditSquare = "\uf044";
		public const string FontAwesomeBell = "\uf0f3";
		public const string FontAwesomeDownChevron = "\uf078";
		public const string FontAwesomeLeftChevron = "\uf053";
	}

	public class _23732IconIdModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		private string name;
		public string Name
		{
			get => name ?? string.Empty;
			set
			{
				name = value;
				OnPropertyChanged();
			}
		}

		private string glyph;
		public string Glyph
		{
			get => glyph;
			set
			{
				glyph = value;
				OnPropertyChanged();
			}
		}
	}
}