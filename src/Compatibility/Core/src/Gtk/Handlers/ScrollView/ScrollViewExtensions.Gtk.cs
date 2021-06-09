using System;
using Gtk;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.ScrollView
{

	public static class ScrollViewExtensions
	{

		public static PolicyType ToNative(this ScrollBarVisibility it) => it switch
		{
			ScrollBarVisibility.Default => PolicyType.Automatic,
			ScrollBarVisibility.Always => PolicyType.Always,
			ScrollBarVisibility.Never => PolicyType.Never,
			_ => throw new ArgumentOutOfRangeException(nameof(it), it, null)
		};

		public static ScrollOrientation ToScrollOrientation(this (PolicyType horizontal, PolicyType vertical) it) =>
			it.horizontal switch
			{
				PolicyType.Always when it.vertical == PolicyType.Never => ScrollOrientation.Horizontal,
				PolicyType.Automatic when it.vertical == PolicyType.Never => ScrollOrientation.Horizontal,
				PolicyType.Never when it.vertical == PolicyType.Always => ScrollOrientation.Vertical,
				PolicyType.Never when it.vertical == PolicyType.Automatic => ScrollOrientation.Vertical,
				PolicyType.Always when it.vertical == PolicyType.Always => ScrollOrientation.Both,
				PolicyType.Automatic when it.vertical == PolicyType.Automatic => ScrollOrientation.Both,

				_ => ScrollOrientation.Neither
			};

		public static ScrollOrientation ToScrollOrientation(this Gtk.ScrolledWindow it)
		{
			it.GetPolicy(out var hscrollbarPolicy, out var vscrollbarPolicy);

			return ToScrollOrientation((hscrollbarPolicy, vscrollbarPolicy));
		}

	}

}