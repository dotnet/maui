using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>
	/// A minimal UI Automation client (raw COM, no WPF) that reads the tree GTK publishes through its AccessKit backend on Windows.
	/// The calls cross into the window's provider, so run them on a worker thread while the GTK thread keeps pumping (see <see cref="Query"/>).
	/// </summary>
	static class UiaProbe
	{
		const int NamePropertyId = 30005;
		const int ControlTypePropertyId = 30003;
		const int IsEnabledPropertyId = 30010;
		const int HelpTextPropertyId = 30013;
		const int FullDescriptionPropertyId = 30159;
		const int ScopeSubtree = 7;

		[ComImport, Guid("30cbe57d-d9d0-452a-ab13-7ac5ac4825ee"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IUIAutomation
		{
			[PreserveSig] int CompareElements();
			[PreserveSig] int CompareRuntimeIds();
			[PreserveSig] int GetRootElement();
			[PreserveSig] int ElementFromHandle(IntPtr hwnd, out IUIAutomationElement element);
			[PreserveSig] int ElementFromPoint();
			[PreserveSig] int GetFocusedElement();
			[PreserveSig] int GetRootElementBuildCache();
			[PreserveSig] int ElementFromHandleBuildCache();
			[PreserveSig] int ElementFromPointBuildCache();
			[PreserveSig] int GetFocusedElementBuildCache();
			[PreserveSig] int CreateTreeWalker();
			[PreserveSig] int ControlViewWalker();
			[PreserveSig] int ContentViewWalker();
			[PreserveSig] int RawViewWalker();
			[PreserveSig] int RawViewCondition();
			[PreserveSig] int ControlViewCondition();
			[PreserveSig] int ContentViewCondition();
			[PreserveSig] int CreateCacheRequest();
			[PreserveSig] int CreateTrueCondition(out IUIAutomationCondition condition);
		}

		[ComImport, Guid("352ffba8-0973-437c-a61f-f64cafd81df9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IUIAutomationCondition
		{
		}

		[ComImport, Guid("d22108aa-8ac5-49a5-837b-37bbb3d7591e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IUIAutomationElement
		{
			[PreserveSig] int SetFocus();
			[PreserveSig] int GetRuntimeId();
			[PreserveSig] int FindFirst();
			[PreserveSig] int FindAll(int scope, IUIAutomationCondition condition, out IUIAutomationElementArray elements);
			[PreserveSig] int FindFirstBuildCache();
			[PreserveSig] int FindAllBuildCache();
			[PreserveSig] int BuildUpdatedCache();
			[PreserveSig] int GetCurrentPropertyValue(int propertyId, [MarshalAs(UnmanagedType.Struct)] out object value);
		}

		[ComImport, Guid("14314595-b4bc-4055-95f2-58f2e42c9855"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IUIAutomationElementArray
		{
			[PreserveSig] int GetLength(out int length);
			[PreserveSig] int GetElement(int index, out IUIAutomationElement element);
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr FindWindowW(string? className, string title);

		/// <summary>What UI Automation reports for one element.</summary>
		public sealed class Node
		{
			public string Name = string.Empty;
			public string HelpText = string.Empty;
			public string FullDescription = string.Empty;
			public int ControlType;
			public bool IsEnabled;
		}

		/// <summary>Runs <paramref name="query"/> on a worker thread while the GTK thread pumps.</summary>
		public static T? Query<T>(Func<T> query, global::Gtk.Window window, int timeoutMs = 15000)
		{
			var task = Task.Run(query);
			GtkTestHost.PumpUntil(() => task.IsCompleted, window, timeoutMs);
			return task.IsCompleted ? task.GetAwaiter().GetResult() : default;
		}

		/// <summary>The elements below the top-level window titled <paramref name="windowTitle"/>, or <see langword="null"/> when UI Automation cannot see it.</summary>
		public static List<Node>? Elements(string windowTitle)
		{
			if (!OperatingSystem.IsWindows())
				return null;

			var hwnd = FindWindowW(null, windowTitle);
			if (hwnd == IntPtr.Zero)
				return null;

			var type = Type.GetTypeFromCLSID(new Guid("ff48dba4-60ef-4201-aa87-54103eef594e"));
			if (type is null || Activator.CreateInstance(type) is not IUIAutomation automation)
				return null;

			if (automation.ElementFromHandle(hwnd, out var root) != 0 || root is null)
				return null;

			if (automation.CreateTrueCondition(out var all) != 0 || root.FindAll(ScopeSubtree, all, out var array) != 0)
				return null;

			array.GetLength(out int length);
			var nodes = new List<Node>(length);
			for (int i = 0; i < length; i++)
			{
				if (array.GetElement(i, out var element) != 0)
					continue;

				nodes.Add(new Node
				{
					Name = Text(element, NamePropertyId),
					HelpText = Text(element, HelpTextPropertyId),
					FullDescription = Text(element, FullDescriptionPropertyId),
					ControlType = element.GetCurrentPropertyValue(ControlTypePropertyId, out var type2) == 0 && type2 is int t ? t : 0,
					IsEnabled = element.GetCurrentPropertyValue(IsEnabledPropertyId, out var enabled) == 0 && enabled is true,
				});
			}

			return nodes;
		}

		static string Text(IUIAutomationElement element, int propertyId) =>
			element.GetCurrentPropertyValue(propertyId, out var value) == 0 ? value as string ?? string.Empty : string.Empty;
	}
}
