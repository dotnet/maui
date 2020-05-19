using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	internal class ElementCollection<T> : ObservableWrapper<Element, T> where T : Element
	{
		public ElementCollection(ObservableCollection<Element> list) : base(list)
		{
		}
	}
}