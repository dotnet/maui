using ElmSharp;

using ELayout = ElmSharp.Layout;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	public class FormsLayout : ELayout
	{
		public string ThemeClass { get; private set; }
		public string ThemeGroup { get; private set; }
		public string ThemeStyle { get; private set; }

		public FormsLayout(EvasObject parent) : base(parent)
		{
		}

		public new void SetTheme(string klass, string group, string style)
		{
			base.SetTheme(klass, group, style);
			ThemeClass = klass;
			ThemeGroup = group;
			ThemeStyle = style;
		}
	}

	public class ApplicationLayout : FormsLayout
	{
		public class Styles
		{
			public const string Default = "default";
		}

		public class Parts
		{
			public const string Content = "elm.swallow.content";
			public const string Background = "elm.swallow.bg";
		}

		public ApplicationLayout(EvasObject parent, string style = Styles.Default) : base(parent)
		{
			SetTheme("layout", "application", style);
		}

		public bool SetContentPart(EvasObject content, bool preserveOldContent = false)
		{
			return SetPartContent(Parts.Content, content, preserveOldContent);
		}

		public bool SetBackgroundPart(EvasObject content, bool preserveOldContent = false)
		{
			return SetPartContent(Parts.Background, content, preserveOldContent);
		}
	}

	public class WidgetLayout : FormsLayout
	{
		public class Styles
		{
			public const string Default = "default";
		}

		public WidgetLayout(EvasObject parent, string style = Styles.Default) : base(parent)
		{
			SetTheme("layout", "elm_widget", style);
		}
	}

	public class EntryLayout : FormsLayout
	{
		public class Styles
		{
			public const string Default = "default";
		}

		public class Parts
		{
			public const string Content = "elm.swallow.content";
		}

		public EntryLayout(EvasObject parent, string style = Styles.Default) : base(parent)
		{
			SetTheme("layout", "entry", style);
		}
	}

	public class EditFieldEntryLayout : FormsLayout
	{
		public class Styles
		{
			public const string SingleLine = "singleline";
			public const string MulitLine = "multiline";
		}

		public class Parts
		{
			public const string Button = "elm.swallow.button";
		}

		public class Signals
		{
			public const string FocusedState = "elm,state,focused";
			public const string UnFocusedState = "elm,state,unfocused";
			public const string ShowButtonAction = "elm,action,show,button";
			public const string HideButtonAction = "elm,action,hide,button";
		}

		public EditFieldEntryLayout(EvasObject parent, string style) : base(parent)
		{
			SetTheme("layout", "editfield", style);
		}

		public bool SetButtonPart(EvasObject content, bool preserveOldContent = false)
		{
			return SetPartContent(Parts.Button, content, preserveOldContent);
		}

		public void SendButtonActionSignal(bool isVisible)
		{
			SignalEmit(isVisible ? Signals.ShowButtonAction : Signals.HideButtonAction, "");
		}

		public void SendFocusStateSignal(bool isFocus)
		{
			SignalEmit(isFocus ? Signals.FocusedState : Signals.UnFocusedState, "");
		}

	}
}
