#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.Entry;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Entry.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Entry']/Docs/*" />
	public static class Entry
	{
		/// <summary>Bindable property for <see cref="ImeOptions"/>.</summary>
		public static readonly BindableProperty ImeOptionsProperty = BindableProperty.Create(nameof(ImeOptions), typeof(ImeFlags), typeof(Entry), ImeFlags.Default);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Entry.xml" path="//Member[@MemberName='GetImeOptions']/Docs/*" />
		public static ImeFlags GetImeOptions(BindableObject element)
		{
			return (ImeFlags)element.GetValue(ImeOptionsProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Entry.xml" path="//Member[@MemberName='SetImeOptions'][1]/Docs/*" />
		public static void SetImeOptions(BindableObject element, ImeFlags value)
		{
			element.SetValue(ImeOptionsProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Entry.xml" path="//Member[@MemberName='ImeOptions']/Docs/*" />
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

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ImeFlags']/Docs/*" />
	public enum ImeFlags
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='Default']/Docs/*" />
		Default = 0,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='None']/Docs/*" />
		None = 1,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='Go']/Docs/*" />
		Go = 2,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='Search']/Docs/*" />
		Search = 3,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='Send']/Docs/*" />
		Send = 4,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='Next']/Docs/*" />
		Next = 5,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='Done']/Docs/*" />
		Done = 6,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='Previous']/Docs/*" />
		Previous = 7,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='ImeMaskAction']/Docs/*" />
		ImeMaskAction = 255,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='NoPersonalizedLearning']/Docs/*" />
		NoPersonalizedLearning = 16777216,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='NoFullscreen']/Docs/*" />
		NoFullscreen = 33554432,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='NoExtractUi']/Docs/*" />
		NoExtractUi = 268435456,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ImeFlags.xml" path="//Member[@MemberName='NoAccessoryAction']/Docs/*" />
		NoAccessoryAction = 536870912,
	}
}
