using System.Collections.ObjectModel;

namespace System.Maui
{
	internal class ElementCollection<T> : ObservableWrapper<Element, T> where T : Element
	{
		public ElementCollection(ObservableCollection<Element> list) : base(list)
		{
		}
	}
}