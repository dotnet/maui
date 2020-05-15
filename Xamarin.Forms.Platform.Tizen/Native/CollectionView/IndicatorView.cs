using System;
using System.Collections.Generic;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class IndicatorView : Index
	{
		const int OddMiddleItem = 10;
		const int EvenMiddleItem = 11;
		List<IndexItem> _list = new List<IndexItem>();

		public IndicatorView(EvasObject parent) : base(parent)
		{
			AutoHide = false;
			IsHorizontal = true;
			Style = "pagecontrol";
			if (Device.Idiom == TargetIdiom.Watch)
				Style = "circle";
		}

		public event EventHandler<SelectedPositionChangedEventArgs> SelectedPosition;

		public void UpdateSelectedIndex(int index)
		{
			if (index > -1 && index < _list.Count)
			{
				_list[index].Select(true);
			}
		}

		public void AppendIndex(int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				var item = Append(null);
				item.Selected += OnSelected;
				_list.Add(item);
			}
			if (Device.Idiom == TargetIdiom.Watch)
				ApplyStyle();
		}

		public void ClearIndex()
		{
			foreach (var item in _list)
			{
				item.Selected -= OnSelected;
			}
			_list.Clear();
			Clear();
		}

		void ApplyStyle()
		{
			foreach (var item in _list)
			{
				if (_list.Count % 2 == 0)
				{
					int position = EvenMiddleItem - (_list.Count / 2) + _list.IndexOf(item);
					string itemStyle = "item/even_" + position;
					item.Style = itemStyle;
				}
				else
				{
					int position = OddMiddleItem - (_list.Count / 2) + _list.IndexOf(item);
					string itemStyle = "item/odd_" + position;
					item.Style = itemStyle;
				}
			}
		}

		void OnSelected(object sender, EventArgs e)
		{
			var index = _list.IndexOf((IndexItem)sender);
			SelectedPosition?.Invoke(this, new SelectedPositionChangedEventArgs(index));
			UpdateSelectedIndex(index);
		}
	}
}
