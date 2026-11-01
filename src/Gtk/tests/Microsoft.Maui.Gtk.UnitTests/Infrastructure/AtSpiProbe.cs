using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>
	/// A minimal AT-SPI client (<c>libatspi</c>) that reads back what GTK publishes on the accessibility bus: names, descriptions,
	/// roles, states, relations and values of the widgets of this process. The calls block on D-Bus replies, so run them on a
	/// worker thread while the GTK thread keeps pumping (see <see cref="Query"/>).
	/// </summary>
	static class AtSpiProbe
	{
		const string Lib = "libatspi.so.0";
		const string GLibLib = "libglib-2.0.so.0";
		const string GObjectLib = "libgobject-2.0.so.0";

		const int StateVisible = 30;
		const int StateShowing = 25;
		const int StateSensitive = 24;
		const int StateChecked = 4;
		const int StateSelected = 23;
		const int StateEditable = 7;
		const int RelationLabelledBy = 2;

		[DllImport(Lib)] static extern int atspi_init();
		[DllImport(Lib)] static extern IntPtr atspi_get_desktop(int i);
		[DllImport(Lib)] static extern int atspi_accessible_get_child_count(IntPtr obj, out IntPtr error);
		[DllImport(Lib)] static extern IntPtr atspi_accessible_get_child_at_index(IntPtr obj, int index, out IntPtr error);
		[DllImport(Lib)] static extern IntPtr atspi_accessible_get_name(IntPtr obj, out IntPtr error);
		[DllImport(Lib)] static extern IntPtr atspi_accessible_get_description(IntPtr obj, out IntPtr error);
		[DllImport(Lib)] static extern IntPtr atspi_accessible_get_role_name(IntPtr obj, out IntPtr error);
		[DllImport(Lib)] static extern IntPtr atspi_accessible_get_state_set(IntPtr obj);
		[DllImport(Lib)] static extern int atspi_state_set_contains(IntPtr set, int state);
		[DllImport(Lib)] static extern IntPtr atspi_accessible_get_relation_set(IntPtr obj, out IntPtr error);
		[DllImport(Lib)] static extern int atspi_relation_get_relation_type(IntPtr relation);
		[DllImport(Lib)] static extern int atspi_relation_get_n_targets(IntPtr relation);
		[DllImport(Lib)] static extern IntPtr atspi_relation_get_target(IntPtr relation, int index);
		[DllImport(Lib)] static extern IntPtr atspi_accessible_get_value_iface(IntPtr obj);
		[DllImport(Lib)] static extern double atspi_value_get_current_value(IntPtr value, out IntPtr error);
		[DllImport(Lib)] static extern double atspi_value_get_minimum_value(IntPtr value, out IntPtr error);
		[DllImport(Lib)] static extern double atspi_value_get_maximum_value(IntPtr value, out IntPtr error);
		[DllImport(Lib)] static extern IntPtr atspi_accessible_get_attributes_as_array(IntPtr obj, out IntPtr error);
		[DllImport(GLibLib)] static extern void g_free(IntPtr mem);
		[DllImport(GLibLib)] static extern void g_array_free(IntPtr array, int freeSegment);
		[DllImport(GObjectLib)] static extern void g_object_unref(IntPtr obj);

		/// <summary>What AT-SPI published for one node of the tree.</summary>
		public sealed class Node
		{
			public string Name = string.Empty;
			public string Description = string.Empty;
			public string Role = string.Empty;
			public bool Showing;
			public bool Sensitive;
			public bool Checked;
			public bool Selected;
			public bool Editable;
			public List<string> LabelledBy = new();
			public double? Value;
			public double? Minimum;
			public double? Maximum;
			public List<string> Attributes = new();
		}

		/// <summary>Whether <c>libatspi</c> loads and the accessibility bus answers.</summary>
		public static bool IsAvailable()
		{
			if (!OperatingSystem.IsLinux())
				return false;

			try
			{
				return atspi_init() >= 0 && atspi_get_desktop(0) != IntPtr.Zero;
			}
			catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException)
			{
				return false;
			}
		}

		/// <summary>
		/// Runs <paramref name="query"/> on a worker thread while the GTK thread pumps (GTK answers the bus requests from its main loop).
		/// </summary>
		public static T? Query<T>(Func<T> query, global::Gtk.Window window, int timeoutMs = 15000)
		{
			var task = Task.Run(query);
			GtkTestHost.PumpUntil(() => task.IsCompleted, window, timeoutMs);
			return task.IsCompleted ? task.GetAwaiter().GetResult() : default;
		}

		/// <summary>Finds the first node of the tree (all applications) whose name is <paramref name="name"/>.</summary>
		public static Node? FindByName(string name, string? role = null)
		{
			var desktop = atspi_get_desktop(0);
			return desktop == IntPtr.Zero ? null : Search(desktop, name, role, 0);
		}

		/// <summary>The names of every node below the desktop that are not empty (diagnostics).</summary>
		public static List<string> Dump()
		{
			var all = new List<string>();
			var desktop = atspi_get_desktop(0);
			if (desktop != IntPtr.Zero)
				Collect(desktop, 0, all);
			return all;
		}

		static void Collect(IntPtr node, int depth, List<string> into)
		{
			if (depth > 24 || into.Count > 400)
				return;

			into.Add(new string(' ', depth) + Text(atspi_accessible_get_role_name(node, out _)) + " '" + Text(atspi_accessible_get_name(node, out _)) + "'");
			int count = atspi_accessible_get_child_count(node, out _);
			for (int i = 0; i < count; i++)
			{
				var child = atspi_accessible_get_child_at_index(node, i, out _);
				if (child != IntPtr.Zero)
					Collect(child, depth + 1, into);
			}
		}

		static Node? Search(IntPtr node, string name, string? role, int depth)
		{
			if (depth > 24)
				return null;

			int count = atspi_accessible_get_child_count(node, out _);
			for (int i = 0; i < count; i++)
			{
				var child = atspi_accessible_get_child_at_index(node, i, out _);
				if (child == IntPtr.Zero)
					continue;

				if (Text(atspi_accessible_get_name(child, out _)) == name && (role is null || Text(atspi_accessible_get_role_name(child, out _)) == role))
					return Read(child);

				var found = Search(child, name, role, depth + 1);
				if (found is not null)
					return found;
			}

			return null;
		}

		static Node Read(IntPtr obj)
		{
			var node = new Node
			{
				Name = Text(atspi_accessible_get_name(obj, out _)),
				Description = Text(atspi_accessible_get_description(obj, out _)),
				Role = Text(atspi_accessible_get_role_name(obj, out _)),
			};

			var states = atspi_accessible_get_state_set(obj);
			if (states != IntPtr.Zero)
			{
				node.Showing = atspi_state_set_contains(states, StateShowing) != 0;
				node.Sensitive = atspi_state_set_contains(states, StateSensitive) != 0;
				node.Checked = atspi_state_set_contains(states, StateChecked) != 0;
				node.Selected = atspi_state_set_contains(states, StateSelected) != 0;
				node.Editable = atspi_state_set_contains(states, StateEditable) != 0;
				g_object_unref(states);
			}

			var relations = atspi_accessible_get_relation_set(obj, out _);
			if (relations != IntPtr.Zero)
			{
				// GArray { gchar* data; guint len } of AtspiRelation*.
				var data = Marshal.ReadIntPtr(relations);
				int length = Marshal.ReadInt32(relations, IntPtr.Size);
				for (int i = 0; i < length; i++)
				{
					var relation = Marshal.ReadIntPtr(data, i * IntPtr.Size);
					node.Attributes.Add("relation:" + atspi_relation_get_relation_type(relation) + ":" + atspi_relation_get_n_targets(relation));
					if (atspi_relation_get_relation_type(relation) != RelationLabelledBy)
						continue;

					for (int t = 0; t < atspi_relation_get_n_targets(relation); t++)
						node.LabelledBy.Add(Text(atspi_accessible_get_name(atspi_relation_get_target(relation, t), out _)));
				}
			}

			var value = atspi_accessible_get_value_iface(obj);
			if (value != IntPtr.Zero)
			{
				node.Value = atspi_value_get_current_value(value, out _);
				node.Minimum = atspi_value_get_minimum_value(value, out _);
				node.Maximum = atspi_value_get_maximum_value(value, out _);
			}

			var attributes = atspi_accessible_get_attributes_as_array(obj, out _);
			if (attributes != IntPtr.Zero)
			{
				var data = Marshal.ReadIntPtr(attributes);
				int length = Marshal.ReadInt32(attributes, IntPtr.Size);
				for (int i = 0; i < length; i++)
					node.Attributes.Add(Text(Marshal.ReadIntPtr(data, i * IntPtr.Size)));
			}

			return node;
		}

		static string Text(IntPtr utf8)
		{
			if (utf8 == IntPtr.Zero)
				return string.Empty;

			return Marshal.PtrToStringUTF8(utf8) ?? string.Empty;
		}
	}
}
