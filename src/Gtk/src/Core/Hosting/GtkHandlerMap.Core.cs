using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Hosting
{
	public static partial class GtkHandlerMap
	{
		// Core handlers. Other areas add their pairs from their own files/assemblies with GtkHandlerMap.Add.
		static void AddCoreEntries()
		{
			Add<ApplicationHandler, GtkApplicationHandler>();
			Add<WindowHandler, GtkWindowHandler>();
			Add<PageHandler, GtkPageHandler>();
			Add<ContentViewHandler, GtkContentViewHandler>();
			Add<LayoutHandler, GtkLayoutHandler>();
			Add<LabelHandler, GtkLabelHandler>();
			Add<ButtonHandler, GtkButtonHandler>();
			Add<ImageHandler, GtkImageHandler>();
			Add<EntryHandler, GtkEntryHandler>();
			Add<EditorHandler, GtkEditorHandler>();
			Add<ScrollViewHandler, GtkScrollViewHandler>();
			Add<BorderHandler, GtkBorderHandler>();
			Add<SwitchHandler, GtkSwitchHandler>();
			Add<CheckBoxHandler, GtkCheckBoxHandler>();

			AddDefault(typeof(IApplication), typeof(GtkApplicationHandler));
			AddDefault(typeof(IWindow), typeof(GtkWindowHandler));
			AddDefault(typeof(IContentView), typeof(GtkContentViewHandler));
			AddDefault(typeof(ILayout), typeof(GtkLayoutHandler));
			AddDefault(typeof(ILabel), typeof(GtkLabelHandler));
			AddDefault(typeof(IButton), typeof(GtkButtonHandler));
			AddDefault(typeof(IImage), typeof(GtkImageHandler));
			AddDefault(typeof(IEntry), typeof(GtkEntryHandler));
			AddDefault(typeof(IEditor), typeof(GtkEditorHandler));
			AddDefault(typeof(IScrollView), typeof(GtkScrollViewHandler));
			AddDefault(typeof(IBorderView), typeof(GtkBorderHandler));
			AddDefault(typeof(ISwitch), typeof(GtkSwitchHandler));
			AddDefault(typeof(ICheckBox), typeof(GtkCheckBoxHandler));
		}
	}
}
