using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 22306_3, "Verify many different button examples", PlatformAffected.iOS)]
public partial class Issue22306_3 : ContentPage
{
	public Issue22306_3()
	{
		InitializeComponent();
	}

	void Button1_Pressed (object sender, System.EventArgs e)
	{
		ClearAll();
		Page1_1.IsVisible = true;
		Page1_2Button.IsVisible = true;
		Page1_3.IsVisible = true;
		Page1_4.IsVisible = true;
	}

	void Button2_Pressed (object sender, System.EventArgs e)
	{
		ClearAll();
		Page2_1.IsVisible = true;
		Page2_2.IsVisible = true;
		Page2_3.IsVisible = true;
	}

	void Button3_Pressed (object sender, System.EventArgs e)
	{
		ClearAll();
		Page3_1.IsVisible = true;
		Page3_2.IsVisible = true;
		Page3_3.IsVisible = true;
		Page3_4.IsVisible = true;
		Page3_5.IsVisible = true;
		Page3_6.IsVisible = true;
		Page3_7.IsVisible = true;
		Page3_8.IsVisible = true;
	}

	void Button4_Pressed (object sender, System.EventArgs e)
	{
		ClearAll();
		Page4_1.IsVisible = true;
		Page4_2.IsVisible = true;
		Page4_3.IsVisible = true;
		Page4_4.IsVisible = true;
		Page4_5.IsVisible = true;
	}

	void Button5_Pressed (object sender, System.EventArgs e)
	{
		ClearAll();
		Page5_1.IsVisible = true;
		Page5_2.IsVisible = true;
		Page5_3.IsVisible = true;
	}

	void Button6_Pressed (object sender, System.EventArgs e)
	{
		ClearAll();
		Page6_1.IsVisible = true;
		Page6_2.IsVisible = true;
		Page6_3.IsVisible = true;
	}

	void Button7_Pressed (object sender, System.EventArgs e)
	{
		ClearAll();
		Page7_1.IsVisible = true;
		Page7_2.IsVisible = true;
		Page7_3.IsVisible = true;
	}

	void Button8_Pressed (object sender, System.EventArgs e)
	{
		ClearAll();
		Page8_1.IsVisible = true;
		Page8_2.IsVisible = true;
		Page8_3.IsVisible = true;
		Page8_4.IsVisible = true;
		Page8_5.IsVisible = true;
		Page8_6Button.IsVisible = true;
		Page8_7Button.IsVisible = true;
	}

	void ClearAll ()
	{
		Page1_1.IsVisible = false;
		Page1_2Button.IsVisible = false;
		Page1_3.IsVisible = false;
		Page1_4.IsVisible = false;
		Page2_1.IsVisible = false;
		Page2_2.IsVisible = false;
		Page2_3.IsVisible = false;
		Page3_1.IsVisible = false;
		Page3_2.IsVisible = false;
		Page3_3.IsVisible = false;
		Page3_4.IsVisible = false;
		Page3_5.IsVisible = false;
		Page3_6.IsVisible = false;
		Page3_7.IsVisible = false;
		Page3_8.IsVisible = false;
		Page4_1.IsVisible = false;
		Page4_2.IsVisible = false;
		Page4_3.IsVisible = false;
		Page4_4.IsVisible = false;
		Page4_5.IsVisible = false;
		Page5_1.IsVisible = false;
		Page5_2.IsVisible = false;
		Page5_3.IsVisible = false;
		Page6_1.IsVisible = false;
		Page6_2.IsVisible = false;
		Page6_3.IsVisible = false;
		Page7_1.IsVisible = false;
		Page7_2.IsVisible = false;
		Page7_3.IsVisible = false;
		Page8_1.IsVisible = false;
		Page8_2.IsVisible = false;
		Page8_3.IsVisible = false;
		Page8_4.IsVisible = false;
		Page8_5.IsVisible = false;
		Page8_6Button.IsVisible = false;
		Page8_7Button.IsVisible = false;
	}

	void Page1_2Button_Pressed (object sender, System.EventArgs e)
	{
		Page1_1.BorderWidth = 75;
	}

	async void ScrollToMiddlePressed (object sender, System.EventArgs e)
	{
		var scroll1 = Page8_1 as ScrollView;
		var scroll2 = Page8_3 as ScrollView;
		var scroll3 = Page8_5 as ScrollView;

		if (scroll1 != null)
		{
			var contentWidth1 = scroll1.ContentSize.Width;
			var scrollViewWidth1 = scroll1.Width;
			var scrollPosition1 = (contentWidth1 - scrollViewWidth1) / 2;
			await scroll1.ScrollToAsync(scrollPosition1, 0, false);
		}

		if (scroll2 != null)
		{
			var contentWidth2 = scroll2.ContentSize.Width;
			var scrollViewWidth2 = scroll2.Width;
			var scrollPosition2 = (contentWidth2 - scrollViewWidth2) / 2;
			await scroll2.ScrollToAsync(scrollPosition2, 0, false);
		}

		if (scroll3 != null)
		{
			var contentWidth3 = scroll3.ContentSize.Width;
			var scrollViewWidth3 = scroll3.Width;
			var scrollPosition3 = (contentWidth3 - scrollViewWidth3) / 2;
			await scroll3.ScrollToAsync(scrollPosition3, 0, false);
		}
	}

	async void ScrollToEndPressed(object sender, System.EventArgs e)
	{
		var scroll1 = Page8_1 as ScrollView;
		var scroll2 = Page8_3 as ScrollView;
		var scroll3 = Page8_5 as ScrollView;

		if (scroll1 != null)
		{
			var contentWidth1 = scroll1.ContentSize.Width;
			await scroll1.ScrollToAsync(contentWidth1, 0, false);
		}

		if (scroll2 != null)
		{
			var contentWidth2 = scroll2.ContentSize.Width;
			await scroll2.ScrollToAsync(contentWidth2, 0, false);
		}

		if (scroll3 != null)
		{
			var contentWidth3 = scroll3.ContentSize.Width;
			await scroll3.ScrollToAsync(contentWidth3, 0, false);
		}
	}
}
