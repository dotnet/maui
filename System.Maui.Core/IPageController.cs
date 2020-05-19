using System.Collections.ObjectModel;

namespace System.Maui
{
	public interface IPageController : IVisualElementController
	{
		Rectangle ContainerArea { get; set; }

		bool IgnoresContainerArea { get; set; }

		ObservableCollection<Element> InternalChildren { get; }

		void SendAppearing();

		void SendDisappearing();
	}
}