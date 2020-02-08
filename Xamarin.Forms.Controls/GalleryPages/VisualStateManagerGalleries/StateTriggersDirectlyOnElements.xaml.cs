using System.Windows.Input;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	public partial class StateTriggersDirectlyOnElements : ContentPage
	{
		public StateTriggersDirectlyOnElements()
		{
			InitializeComponent();
		}
	}

	public class StateTriggersDirectlyOnElementsViewModel : BindableObject
	{
		bool _toggleState;

		public bool ToggleState
		{
			get => _toggleState;
			set
			{
				_toggleState = value;
				OnPropertyChanged(nameof(ToggleState));
			}
		}

		public ICommand ToggleCommand => new Command(OnToggle);

		void OnToggle()
		{
			ToggleState = !ToggleState;
		}
	}
}