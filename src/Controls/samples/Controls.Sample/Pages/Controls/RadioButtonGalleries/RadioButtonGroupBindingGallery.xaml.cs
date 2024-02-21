using System.ComponentModel;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.RadioButtonGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RadioButtonGroupBindingGallery
	{
		RadioButtonGroupBindingModel _viewModel;

		public RadioButtonGroupBindingGallery()
		{
			InitializeComponent();
			_viewModel = new RadioButtonGroupBindingModel() { GroupName = "group1" };
			BindingContext = _viewModel;
		}

		private void Button_Clicked(object sender, System.EventArgs e)
		{
			_viewModel.Selection = "B";
		}
	}

	public class RadioButtonGroupBindingModel : INotifyPropertyChanged
	{
		private string? _groupName;
		private object? _selection;

		public event PropertyChangedEventHandler? PropertyChanged;

		void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string? GroupName
		{
			get => _groupName;
			set
			{
				_groupName = value;
				OnPropertyChanged(nameof(GroupName));
			}
		}

		public object? Selection
		{
			get => _selection;
			set
			{
				_selection = value;
				OnPropertyChanged(nameof(Selection));
			}
		}
	}
}