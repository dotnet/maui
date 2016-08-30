using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	public class FormsCommandBar : CommandBar
	{
		// TODO Once 10.0.14393.0 is available, enable dynamic overflow: https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.controls.commandbar.isdynamicoverflowenabled.aspx 

		public FormsCommandBar()
		{
			PrimaryCommands.VectorChanged += OnCommandsChanged;
			SecondaryCommands.VectorChanged += OnCommandsChanged;
			UpdateVisibility();
		}

		void OnCommandsChanged(IObservableVector<ICommandBarElement> sender, IVectorChangedEventArgs args)
		{
			UpdateVisibility();
		}

		void UpdateVisibility()
		{
			Visibility = PrimaryCommands.Count + SecondaryCommands.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}