﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Collections.ObjectModel;

﻿using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{	
#if APP
	public class BaseView : ContentPage
	{
		public BaseView()
		{
		}
	}

	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1549, "XAML converters not invoked", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1549 : BaseView
	{	
		public Issue1549 ()
		{
			InitializeComponent ();

			Items = new ObservableCollection<Issue1549Item> ();
			Items.Add (new Issue1549Item () { IsLocked = true });
			Items.Add (new Issue1549Item () { IsLocked = true });
			Items.Add (new Issue1549Item () { IsLocked = true });
			Items.Add (new Issue1549Item () { IsLocked = true });
			lst.BindingContext = this;
		}

		public new ObservableCollection<Issue1549Item> Items {
			get;
			set;
		}
	}

	public class InvertBoolenConverter : IValueConverter
	{

		#region IValueConverter implementation

		public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool)  {

				return !(bool)value;
			}
			return value;
		}

		public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}


	public class Issue1549Item
	{

		public bool IsLocked {
			get;
			set;
		}

	}
#endif
}

