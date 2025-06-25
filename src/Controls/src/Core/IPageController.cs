#nullable disable
using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public interface IPageController : IVisualElementController
	{
		ObservableCollection<Element> InternalChildren { get; }

		void SendAppearing();

		void SendDisappearing();
	}
}