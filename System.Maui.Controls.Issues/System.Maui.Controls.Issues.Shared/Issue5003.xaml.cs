using System;
using System.Globalization;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5003, "iOS StrikeThrough applied to null string throws error", PlatformAffected.iOS)]
	public partial class Issue5003 : ContentPage
	{

		public Issue5003()
		{
#if APP
			InitializeComponent();
#endif
			BindingContext = new VM();
		}

		[Preserve(AllMembers = true)]
		class VM
		{
			public string MyNullString { get; set; }
			public bool SomeBoolean { get; set; }
		}
	}

	public class StrikeThroughIfTrueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b)
			{
				if (b)
					return TextDecorations.Strikethrough;
			}

			return TextDecorations.None;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}