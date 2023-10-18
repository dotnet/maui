using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.DatePicker)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11472, "DateTime Localization Issue",
		PlatformAffected.iOS | PlatformAffected.Android)]
	public class DateTimeLocalizationTests : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.DateTimePickerGalleries.DateTimePickerGallery());
#endif
		}

#if UITEST

		public string TimeString(String format, String time)
		{
			RunningApp.ClearText("timeFormatString");
			RunningApp.EnterText("timeFormatString", format);
			RunningApp.PressEnter();
			RunningApp.ClearText("settingTime");
			RunningApp.EnterText("settingTime", time);
			RunningApp.PressEnter();
			var text = RunningApp.ReadTimePicker("timeClockOptions");
			return text;
		}

		public string DateString(String format, String date)
		{
			RunningApp.ClearText("dateFormatString");
			RunningApp.EnterText("dateFormatString", format);
			RunningApp.PressEnter();
			RunningApp.ClearText("settingDate");
			RunningApp.EnterText("settingDate", date);
			RunningApp.PressEnter();
			var text = RunningApp.ReadDatePicker("dateCalendarOptions");
			return text;
		}

		[Test]
		// Tests runs locally without issues but doesn't run successfully in a hosted agent yet
		[Category(UITestCategories.UwpIgnore)]
		[FailsOnMauiAndroid]
		[FailsOnMauiIOS]
		public void TimePicker24H()
		{
			RunningApp.Tap(x => x.Marked("TimePicker"));
#if !WINDOWS
			Assert.AreEqual("0.0.0 A", TimeString("H.m.s t", "0, 0"));
			Assert.AreEqual("13:05 PM", TimeString("HH:mm tt", "13, 5"));
			Assert.AreEqual("12 PM", TimeString("HH tt", "12, 0"));
			Assert.AreEqual("5.", TimeString("H.", "5, 1"));
#else
			Assert.AreEqual("23:00", TimeString("HH", "23, 0"));
			Assert.AreEqual("11:00:PM", TimeString("hh", "23, 0"));
#endif
		}

#if !WINDOWS
		[Test]
		[Compatibility.UITests.FailsOnMauiAndroid]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TimePicker12H()
		{
			RunningApp.Tap(x => x.Marked("TimePicker"));
			Assert.AreEqual("12 0/AM", TimeString("hh m/tt", "0, 0"));
			Assert.AreEqual("12", TimeString("hh", "12, 37"));
			Assert.AreEqual("11-00", TimeString("h-ss", "23, 59"));
			Assert.AreEqual("07:59", TimeString("hh:mm", "7, 59"));

		}

		[Test]
		[FailsOnMauiAndroid]
		public void TimePickerOther()
		{
			RunningApp.Tap(x => x.Marked("TimePicker"));
			Assert.AreEqual("59, P", TimeString("mm, t", "13, 59"));
			Assert.AreEqual("ABCDEGIJLNOP", TimeString("ABCDEGIJLNOP", "23, 59"));
			Assert.AreEqual("QRSTUVWXYZ", TimeString("QRSTUVWXYZ", "23, 59"));
			Assert.AreEqual("abceijklnopqruvwx", TimeString("abceijklnopqruvwx", "23, 59"));
		}
#endif

		[Test]
		// Tests runs locally without issues but doesn't run successfully in a hosted agent yet
		[Category(UITestCategories.UwpIgnore)]
		[FailsOnMauiAndroid]
		[FailsOnMauiIOS]
		public void DatePickerDMY()
		{
			RunningApp.Tap(x => x.Marked("DatePicker"));
#if !WINDOWS
			Assert.AreEqual("31/1/99", DateString("d/M/y", "1999, 1, 31"));
			Assert.AreEqual("02-29-00", DateString("MM-dd-yy", "2000, 2, 29"));
			Assert.AreEqual("2010, Apr, Thu", DateString("yyy, MMM, ddd", "2010, 4, 15"));
			Assert.AreEqual("August.Saturday.2015", DateString("MMMM.dddd.yyyy", "2015, 8, 1"));
#else
			Assert.AreEqual("31,1,99", DateString("d/M/y", "1999, 1, 31"));
			Assert.AreEqual("29,2,00", DateString("MM-dd-yy", "2000, 2, 29"));
			Assert.AreEqual("Thu 15,Apr,2010", DateString("yyy, MMM, ddd", "2010, 4, 15"));
			Assert.AreEqual("Saturday,August,2015", DateString("MMMM.dddd.yyyy", "2015, 8, 1"));
#endif
		}

		[Test]
		// Tests runs locally without issues but doesn't run successfully in a hosted agent yet
		[Category(UITestCategories.UwpIgnore)]
		[FailsOnMauiAndroid]
		[FailsOnMauiIOS]
		public void DatePickerMissing()
		{
			RunningApp.Tap(x => x.Marked("DatePicker"));
#if !WINDOWS
			Assert.AreEqual("October 97", DateString("MMMM yy", "1997, 10, 30"));
			Assert.AreEqual("Monday", DateString("dddd", "2020, 7, 20"));
			Assert.AreEqual("2002: Dec", DateString("yyyy: MMM", "2002, 12, 31"));
#else
			Assert.AreEqual("October,97", DateString("MMMM yy", "1997, 10, 30"));
			Assert.AreEqual("Monday", DateString("dddd", "2020, 7, 20"));
			Assert.AreEqual("Dec,2002", DateString("yyyy: MMM", "2002, 12, 31"));
#endif
		}

#if !WINDOWS
		[Test]
		[FailsOnMauiAndroid]
		[FailsOnMauiIOS]
		public void DatePickerLetters()
		{
			RunningApp.Tap(x => x.Marked("DatePicker"));
			Assert.AreEqual("ABCDEGIJLNOP", DateString("ABCDEGIJLNOP", "2002, 12, 31"));
			Assert.AreEqual("QRSTUVWXYZ", DateString("QRSTUVWXYZ", "2002, 12, 31"));
			Assert.AreEqual("abceijklnopqruvwx", DateString("abceijklnopqruvwx", "2002, 12, 31"));
		}
#endif

#endif
	}
}