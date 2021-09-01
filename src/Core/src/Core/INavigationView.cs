using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	internal interface INavigationView : IView
	{
		IReadOnlyList<IView> ModalStack { get; }

		IReadOnlyList<IView> NavigationStack { get; }

		void InsertPageBefore(IView page, IView before);
		Task<IView> PopAsync();
		Task<IView> PopAsync(bool animated);
		Task<IView> PopModalAsync();
		Task<IView> PopModalAsync(bool animated);
		Task PopToRootAsync();
		Task PopToRootAsync(bool animated);

		Task PushAsync(IView page);

		Task PushAsync(IView page, bool animated);
		Task PushModalAsync(IView page);
		Task PushModalAsync(IView page, bool animated);

		void RemovePage(IView page);
	}
}
