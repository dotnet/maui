using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public class PageOne : ContentPage
    {
        public PageOne()
        {
            AutomationId = "PageOne";
            Title = "Page One";
            Content = new Label
            {
                AutomationId = "PageOneLabel",
                Text = "This is Page One.",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }
    }

    public class PageTwo : ContentPage
    {
        public PageTwo()
        {
            AutomationId = "PageTwo";
            Title = "Page Two";
            Content = new Label
            {
                AutomationId = "PageTwoLabel",
                Text = "Welcome to Page Two!",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }
    }

    public class PageThree : ContentPage
    {
        public PageThree()
        {
            AutomationId = "PageThree";
            Title = "Page Three";
            Content = new Label
            {
                AutomationId = "PageThreeLabel",
                Text = "You are viewing Page Three.",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }
    }

    public class PageFour : ContentPage
    {
        public PageFour()
        {
            AutomationId = "PageFour";
            Title = "Page Four";
            Content = new Label
            {
                AutomationId = "PageFourLabel",
                Text = "This is Page Four.",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }
    }

    public class PageFive : ContentPage
    {
        public PageFive()
        {
            AutomationId = "PageFive";
            Title = "Page Five";
            Content = new Label
            {
                AutomationId = "PageFiveLabel",
                Text = "Welcome to Page Five!",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }
    }

    public class PageSix : ContentPage
    {
        public PageSix()
        {
            AutomationId = "PageSix";
            Title = "Page Six";
            Content = new Label
            {
                AutomationId = "PageSixLabel",
                Text = "You are viewing Page Six.",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }
    }

    public class PageSeven : ContentPage
    {
        public PageSeven()
        {
            AutomationId = "PageSeven";
            Title = "Page Seven";
            Content = new Label
            {
                AutomationId = "PageSevenLabel",
                Text = "This is Page Seven.",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }
    }
}
