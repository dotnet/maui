#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.Entry;

	/// <summary>Controls input method editor (IME) options for entry fields on the Android platform.</summary>
	public static class Entry
	{
		/// <summary>Bindable property for <see cref="ImeOptions"/>.</summary>
		public static readonly BindableProperty ImeOptionsProperty = BindableProperty.Create(nameof(ImeOptions), typeof(ImeFlags), typeof(Entry), ImeFlags.Default);

		/// <summary>Returns flags that specify input method editor options, such as the kind of action that is sent by the editor.</summary>
		/// <param name="element">The Android entry for which to get the input method editor options.</param>
		/// <returns>The flags that specify input method editor options, such as the kind of action that is sent by the editor.</returns>
		public static ImeFlags GetImeOptions(BindableObject element)
		{
			return (ImeFlags)element.GetValue(ImeOptionsProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Entry.xml" path="//Member[@MemberName='SetImeOptions'][1]/Docs/*" />
		public static void SetImeOptions(BindableObject element, ImeFlags value)
		{
			element.SetValue(ImeOptionsProperty, value);
		}

		/// <summary>Returns flags that specify input method editor options, such as the kind of action that is sent by the editor.</summary>
		/// <param name="config">The platform configuration for the Android entry for which to get the input method editor options.</param>
		/// <returns>The flags that specify input method editor options, such as the kind of action that is sent by the editor.</returns>
		public static ImeFlags ImeOptions(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetImeOptions(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Entry.xml" path="//Member[@MemberName='SetImeOptions'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetImeOptions(this IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, FormsElement> config, ImeFlags value)
		{
			SetImeOptions(config.Element, value);
			return config;
		}
	}

	/// <summary>Enumerates input method editor (IME) options for entry fields on the Android platform.</summary>
	public enum ImeFlags
	{
		/// <summary>The null IME option, which indicates no options.</summary>
		Default = 0,
		/// <summary>Indicates no action will be made available.</summary>
		None = 1,
		/// <summary>Indicates that the action key will send a <c>go</c> action.</summary>
		Go = 2,
		/// <summary>Indicates that the action key will send a <c>search</c> action.</summary>
		Search = 3,
		/// <summary>Indicates that the action key will send a <c>send</c> action.</summary>
		Send = 4,
		/// <summary>Indicates that the action key will send a <c>next</c> action.</summary>
		Next = 5,
		/// <summary>Indicates that the action key will send a <c>done</c> action.</summary>
		Done = 6,
		/// <summary>Indicates that the action key will send a <c>previous</c> action.</summary>
		Previous = 7,
		/// <summary>The mask to select action options.</summary>
		ImeMaskAction = 255,
		/// <summary>Indicates that the spellchecker will neither learn from the user, nor suggest corrections based on what the user has previously typed.</summary>
		NoPersonalizedLearning = 16777216,
		/// <summary>Indicates that the editor UI should not go fullscreen.</summary>
		NoFullscreen = 33554432,
		/// <summary>Indicates that no UI will be shown for extracted text.</summary>
		NoExtractUi = 268435456,
		/// <summary>Indicates that no UI will be displayed for custom actions.</summary>
		NoAccessoryAction = 536870912,
	}
}
