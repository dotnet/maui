using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	public class FormsCommandBar : CommandBar
	{
		Windows.UI.Xaml.Controls.Button _moreButton;

		public FormsCommandBar()
		{
			PrimaryCommands.VectorChanged += OnCommandsChanged;
			SecondaryCommands.VectorChanged += OnCommandsChanged;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_moreButton = GetTemplateChild("MoreButton") as Windows.UI.Xaml.Controls.Button;
			UpdateMore();
		}

		void OnCommandsChanged(IObservableVector<ICommandBarElement> sender, IVectorChangedEventArgs args)
		{
			UpdateMore();
		}

		void UpdateMore()
		{
			if (_moreButton == null)
				return;

			_moreButton.Visibility = PrimaryCommands.Count > 0 || SecondaryCommands.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}