using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public sealed class UriMediaSource : MediaSource
	{
		public static readonly BindableProperty UriProperty = BindableProperty.Create(nameof(Uri), typeof(Uri), typeof(UriMediaSource), default(Uri),
			propertyChanged: (bindable, oldvalue, newvalue) => ((UriMediaSource)bindable).OnSourceChanged(), validateValue: (bindable, value) => value == null || ((Uri)value).IsAbsoluteUri);


		[TypeConverter(typeof(UriTypeConverter))]
		public Uri Uri
		{
			get { return (Uri)GetValue(UriProperty); }
			set { SetValue(UriProperty, value); }
		}
		
		public override string ToString()
		{
			return $"Uri: {Uri}";
		}
	}
}