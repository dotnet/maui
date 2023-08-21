// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	[System.ComponentModel.TypeConverter(typeof(Shapes.PointCollectionConverter))]
	public sealed class PointCollection : ObservableCollection<Point>
	{
		public PointCollection() : base()
		{
		}

		public PointCollection(Point[] p)
			: base(p)
		{
		}

		public static implicit operator PointCollection(Point[] d)
			=> d == null ? new() : new(d);
	}
}