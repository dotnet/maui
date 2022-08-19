using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Polygon : IGeoPathMapElement, IFilledMapElement
	{
		public Paint? Fill => FillColor?.AsPaint();

		public Location this[int index]
		{
			get { return Geopath[index]; }
			set
			{
				RemoveAt(index);
				Insert(index, value);
			}
		}

		public int Count => Geopath.Count;

		public bool IsReadOnly => false;

		public void Add(Location item)
		{
			var index = Geopath.Count;
			Geopath.Add(item);
			NotifyHandler(nameof(Add), index, item);
		}

		public void Clear()
		{
			for (int i = Geopath.Count - 1; i >= 0; i--)
				RemoveAt(i);
		}

		public bool Contains(Location item)
		{
			return Geopath.Contains(item);
		}

		public void CopyTo(Location[] array, int arrayIndex)
		{
			Geopath.CopyTo(array, arrayIndex);
		}

		public IEnumerator<Location> GetEnumerator()
		{
			return Geopath.GetEnumerator();
		}

		public int IndexOf(Location item)
		{
			return Geopath.IndexOf(item);
		}

		public void Insert(int index, Location item)
		{
			Geopath.Insert(index, item);
			NotifyHandler(nameof(Insert), index, item);

		}

		public bool Remove(Location item)
		{
			var index = Geopath.IndexOf(item);
			var result = Geopath.Remove(item);
			NotifyHandler(nameof(Remove), index, item);
			return result;
		}

		public void RemoveAt(int index)
		{
			var item = Geopath[index];
			Geopath.RemoveAt(index);
			NotifyHandler(nameof(Remove), index, item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Geopath.GetEnumerator();
		}

		void NotifyHandler(string action, int index, Location item)
		{
			Handler?.UpdateValue(nameof(Geopath));
		}
	}
}
