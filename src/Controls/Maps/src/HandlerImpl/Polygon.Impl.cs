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
		/// <inheritdoc cref="IFilledMapElement.Fill"/>
		public Paint? Fill => FillColor?.AsPaint();

		/// <summary>
		/// Gets or sets the location object at the specified index.
		/// </summary>
		/// <param name="index">The index of the location object to return or set.</param>
		/// <returns>The location object at the specified index.</returns>
		public Location this[int index]
		{
			get { return Geopath[index]; }
			set
			{
				RemoveAt(index);
				Insert(index, value);
			}
		}

		/// <summary>
		/// Gets the location object count in this polygon.
		/// </summary>
		public int Count => Geopath.Count;

		/// <summary>
		/// Gets whether this polygon is read-only.
		/// </summary>
		/// <remarks>Always returns <see langword="false"/>.</remarks>
		public bool IsReadOnly => false;

		/// <summary>
		/// Adds a location to this polygon.
		/// </summary>
		/// <param name="item">The location to add.</param>
		public void Add(Location item)
		{
			var index = Geopath.Count;
			Geopath.Add(item);
			NotifyHandler(nameof(Add), index, item);
		}

		/// <summary>
		/// Clears all locations that make up this polygon.
		/// </summary>
		public void Clear()
		{
			for (int i = Geopath.Count - 1; i >= 0; i--)
			{
				RemoveAt(i);
			}
		}

		/// <summary>
		/// Returns whether the specified location is contained in this polygon.
		/// </summary>
		/// <param name="item">The location object of which to determine if it is contained in the location collection of this polygon.</param>
		/// <returns><see langword="true"/> if <paramref name="item"/> is contained in the location collection of this polygon, otherwise <see langword="false"/>.</returns>
		public bool Contains(Location item)
		{
			return Geopath.Contains(item);
		}

		/// <summary>
		/// Copies the child locations to the specified array.
		/// </summary>
		/// <param name="array">The target array to copy the child views to.</param>
		/// <param name="arrayIndex">The index at which the copying needs to start.</param>
		public void CopyTo(Location[] array, int arrayIndex)
		{
			Geopath.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Returns an enumerator that lists all of the location object in this polygon.
		/// </summary>
		/// <returns>A <see cref="IEnumerator{T}"/> of type <see cref="Location"/> with all the locations in this polygon.</returns>
		public IEnumerator<Location> GetEnumerator()
		{
			return Geopath.GetEnumerator();
		}

		/// <summary>
		/// Gets the index of a specified location object.
		/// </summary>
		/// <param name="item">The location object of which to determine the index.</param>
		/// <returns>The index of the specified location, if the location was not found this will return <c>-1</c>.</returns>
		public int IndexOf(Location item)
		{
			return Geopath.IndexOf(item);
		}

		/// <summary>
		/// Inserts a location object at the specified index.
		/// </summary>
		/// <param name="index">The index at which to specify the location object.</param>
		/// <param name="item">The location object to insert.</param>
		public void Insert(int index, Location item)
		{
			Geopath.Insert(index, item);
			NotifyHandler(nameof(Insert), index, item);
		}

		/// <summary>
		/// Removes a location object from this polygon.
		/// </summary>
		/// <param name="item">The location object to remove.</param>
		/// <returns><see langword="true"/> if the location was removed successfully, otherwise <see langword="false"/>.</returns>
		public bool Remove(Location item)
		{
			var index = Geopath.IndexOf(item);
			var result = Geopath.Remove(item);
			NotifyHandler(nameof(Remove), index, item);
			return result;
		}

		/// <summary>
		/// Removes a location object at the specified index.
		/// </summary>
		/// <param name="index">The index at which to remove the location.</param>
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
