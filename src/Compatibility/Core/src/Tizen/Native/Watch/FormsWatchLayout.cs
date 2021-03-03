using ElmSharp;
using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native.Watch
{
	public class FormsWatchLayout : FormsLayout
	{
		public FormsWatchLayout(EvasObject parent) : base(parent)
		{
			if (Device.Idiom != TargetIdiom.Watch)
				Log.Error($"{0} is only supported on TargetIdiom.Watch : {1}", this, Device.Idiom);
		}
	}

	public class DateTimeLayout : FormsWatchLayout
	{
		public class Styles
		{
			public const string Default = "datetime";
		}

		public class Parts
		{
			public const string ButtomButton = "elm.swallow.btn";
		}

		public DateTimeLayout(EvasObject parent, string style = Styles.Default) : base(parent)
		{
			SetTheme("layout", "circle", style);
		}

		public bool SetBottomButtonPart(EvasObject content, bool preserveOldContent = false)
		{
			return SetPartContent(Parts.ButtomButton, content, preserveOldContent);
		}
	}

	public class PopupLayout : FormsWatchLayout
	{
		public class Parts
		{
			public const string Button1 = "button1";
			public const string Button2 = "button2";
			public const string Title = "elm.text.title";
			public const string Content = "elm.swallow.content";

			public class Colors
			{
				public const string Title = "text_title";
			}
		}

		public class Styles
		{
			public const string Circle = "content/circle";
			public const string Buttons2 = "content/circle/buttons2";
		}


		public PopupLayout(EvasObject parent, string style) : base(parent)
		{
			SetTheme("layout", "popup", style);
		}

		public bool SetTitleText(string title)
		{
			return SetPartText(Parts.Title, title);
		}

		public void SetTitleColor(EColor color)
		{
			SetPartColor(Parts.Colors.Title, color);
		}
	}

	public class PopupClassBaseGroupLayout : FormsWatchLayout
	{
		public class Styles
		{
			public const string Circle = "circle";
		}

		public class Parts
		{
			public const string ActionArea = "elm.swallow.action_area";
		}

		public PopupClassBaseGroupLayout(EvasObject parent) : base(parent)
		{
			SetTheme("popup", "base", Styles.Circle);
		}
	}

	public class PopupClass2ButtonGroupLayout : FormsWatchLayout
	{
		public class Styles
		{
			public const string Circle = "popup/circle";
		}

		public class Parts
		{
			public const string Content = "elm.swallow.content";
		}

		public PopupClass2ButtonGroupLayout(EvasObject parent) : base(parent)
		{
			SetTheme("popup", "buttons2", Styles.Circle);
		}
	}

}
