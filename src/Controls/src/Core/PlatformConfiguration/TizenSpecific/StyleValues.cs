#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	/// <summary>Enumerates button styles</summary>
	public static class ButtonStyle
	{
		/// <summary>Indicates the default button style.</summary>
		public const string Default = "default";
		/// <summary>Indicates the circle button style.</summary>
		public const string Circle = "circle";
		/// <summary>Indicates the bottom button style.</summary>
		public const string Bottom = "bottom";
		/// <summary>Indicates a text button.</summary>
		public const string Text = "textbutton";
		/// <summary>Indicates a selection button.</summary>
		public const string SelectMode = "select_mode";
	}

	/// <summary>Enumerates visual styles for switches.</summary>
	public static class SwitchStyle
	{
		/// <summary>Indicates a checkbox UI.</summary>
		public const string CheckBox = "default";
		/// <summary>Indicates a toggle UI.</summary>
		public const string Toggle = "toggle";
		/// <summary>Indicates a favorite, or star, UI.</summary>
		public const string Favorite = "favorite";
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/SwitchStyle.xml" path="//Member[@MemberName='OnOff']/Docs/*" />
		public const string OnOff = "on&off";
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/SwitchStyle.xml" path="//Member[@MemberName='Small']/Docs/*" />
		public const string Small = "small";
	}

	/// <summary>Enumerates visual styles for progress bars.</summary>
	public static class ProgressBarStyle
	{
		/// <summary>Indicates the default progress bar style.</summary>
		public const string Default = "default";
		/// <summary>Indicates the pending style, to communicate that a progress estimate has not yet been made.</summary>
		public const string Pending = "pending";
	}

	/// <summary>Enumerates tab bar styles for a tabbed page.</summary>
	public static class TabbedPageStyle
	{
		/// <summary>Indicates the default tab bar style.</summary>
		public const string Default = "default";
		/// <summary>Indicates a tab bar with no title.</summary>
		public const string Tabbar = "tabbar";
		/// <summary>Indicates a tab bar with a title.</summary>
		public const string TabbarWithTitle = "tabbar_with_title";
	}
}
