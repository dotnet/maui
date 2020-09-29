using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;


namespace Xamarin.Forms
{
	public interface ITemplatedItemsList<TItem> : IReadOnlyList<TItem>, INotifyCollectionChanged where TItem : BindableObject
	{
		event NotifyCollectionChangedEventHandler GroupedCollectionChanged;
		event PropertyChangedEventHandler PropertyChanged;

		object BindingContext { get; }
		string Name { get; set; }
		TItem HeaderContent { get; }
		IEnumerable ItemsSource { get; }
		IReadOnlyList<string> ShortNames { get; }

		IListProxy ListProxy { get; }

		DataTemplate SelectDataTemplate(object item);
		int GetGlobalIndexForGroup(ITemplatedItemsList<TItem> group);
		int GetGlobalIndexOfItem(object item);
		ITemplatedItemsList<TItem> GetGroup(int index);
		Tuple<int, int> GetGroupAndIndexOfItem(object item);
		Tuple<int, int> GetGroupAndIndexOfItem(object group, object item);
		int GetGroupIndexFromGlobal(int globalIndex, out int leftOver);
		int IndexOf(TItem item);
		TItem ActivateContent(int index, object item = null);
		TItem UpdateContent(TItem content, int index);
		TItem UpdateHeader(TItem content, int groupIndex);
	}
}