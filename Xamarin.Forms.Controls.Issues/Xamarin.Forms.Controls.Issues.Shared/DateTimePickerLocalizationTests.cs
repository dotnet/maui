using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
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
			var text = RunningApp.WaitForElement("timeClockOptions")[0].ReadText();
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
			var text = RunningApp.WaitForElement("dateCalendarOptions")[0].ReadText();
			return text;
		}

		[Test]
		public void TimePicker24H()
		{
			RunningApp.Tap(x => x.Marked("TimePicker"));
			Assert.AreEqual("0.0.0 A", TimeString("H.m.s t", "0, 0"));
			Assert.AreEqual("13:05 PM", TimeString("HH:mm tt", "13, 5"));
			Assert.AreEqual("12 PM", TimeString("HH tt", "12, 0"));
			Assert.AreEqual("5.", TimeString("H.", "5, 1"));
		}

		[Test]
		public void TimePicker12H()
		{
			RunningApp.Tap(x => x.Marked("TimePicker"));
			Assert.AreEqual("12 0/AM", TimeString("hh m/tt", "0, 0"));
			Assert.AreEqual("12", TimeString("hh", "12, 37"));
			Assert.AreEqual("11-00", TimeString("h-ss", "23, 59"));
			Assert.AreEqual("07:59", TimeString("hh:mm", "7, 59"));

		}

		[Test]
		public void TimePickerOther()
		{
			RunningApp.Tap(x => x.Marked("TimePicker"));
			Assert.AreEqual("59, P", TimeString("mm, t", "13, 59"));
			Assert.AreEqual("ABCDEGIJLNOP", TimeString("ABCDEGIJLNOP", "23, 59"));
			Assert.AreEqual("QRSTUVWXYZ", TimeString("QRSTUVWXYZ", "23, 59"));
			Assert.AreEqual("abceijklnopqruvwx", TimeString("abceijklnopqruvwx", "23, 59"));
		}

		[Test]
		public void DatePickerDMY()
		{
			RunningApp.Tap(x => x.Marked("DatePicker"));
			Assert.AreEqual("31/1/99", DateString("d/M/y", "1999, 1, 31"));
			Assert.AreEqual("02-29-00", DateString("MM-dd-yy", "2000, 2, 29"));
			Assert.AreEqual("2010, Apr, Thu", DateString("yyy, MMM, ddd", "2010, 4, 15"));
			Assert.AreEqual("August.Saturday.2015", DateString("MMMM.dddd.yyyy", "2015, 8, 1"));
		}

		[Test]
		public void DatePickerMissing()
		{
			RunningApp.Tap(x => x.Marked("DatePicker"));
			Assert.AreEqual("October 97", DateString("MMMM yy", "1997, 10, 30"));
			Assert.AreEqual("Monday", DateString("dddd", "2020, 7, 20"));
			Assert.AreEqual("2002: Dec", DateString("yyyy: MMM", "2002, 12, 31"));
		}

		[Test]
		public void DatePickerLetters()
		{
			RunningApp.Tap(x => x.Marked("DatePicker"));
			Assert.AreEqual("ABCDEGIJLNOP", DateString("ABCDEGIJLNOP", "2002, 12, 31"));
			Assert.AreEqual("QRSTUVWXYZ", DateString("QRSTUVWXYZ", "2002, 12, 31"));
			Assert.AreEqual("abceijklnopqruvwx", DateString("abceijklnopqruvwx", "2002, 12, 31"));
		}

#endif
	}
}