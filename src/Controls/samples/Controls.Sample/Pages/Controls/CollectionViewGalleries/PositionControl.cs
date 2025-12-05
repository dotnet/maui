using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{
    internal class PositionControl : ContentView
    {
        private readonly Slider _slider;

        [RequiresUnreferencedCode("Uses reflection-based APIs for binding")]
        public PositionControl(CarouselView carousel, int itemsCount)
        {
            var animateLabel = new Label { Text = "Animate: ", VerticalTextAlignment = TextAlignment.Center };
            var animateSwitch = new Switch { BindingContext = carousel, AutomationId = "AnimateSwitch" };
            
            // String-based binding should be replaced with expression-based binding in trim-compatible code
            animateSwitch.SetBinding(Switch.IsToggledProperty, nameof(carousel.IsScrollAnimated), BindingMode.TwoWay);

            var swipeLabel = new Label { Text = "Swipe: ", VerticalTextAlignment = TextAlignment.Center };
            var swipeSwitch = new Switch { BindingContext = carousel, AutomationId = "SwipeSwitch" };
            swipeSwitch.SetBinding(Switch.IsToggledProperty, nameof(carousel.IsSwipeEnabled), BindingMode.TwoWay);

            var bounceLabel = new Label { Text = "Bounce: ", VerticalTextAlignment = TextAlignment.Center };
            var bounceSwitch = new Switch { BindingContext = carousel, AutomationId = "BounceSwitch" };
            bounceSwitch.SetBinding(Switch.IsToggledProperty, nameof(carousel.IsBounceEnabled), BindingMode.TwoWay);

            _slider = new Slider
            {
                BackgroundColor = Colors.Pink,
                ThumbColor = Colors.Red,
                WidthRequest = 100,
                BindingContext = carousel
            };
            
            // Using string-based binding for sample simplicity
            _slider.SetBinding(Slider.ValueProperty, nameof(carousel.Position));
            UpdatePositionCount(itemsCount);

            var indexLabel = new Label { Text = "Go To Position: ", VerticalTextAlignment = TextAlignment.Center };
            var label = new Label { WidthRequest = 20, BackgroundColor = Colors.LightCyan, AutomationId = "CurrentPositionLabel" };
            label.SetBinding(Label.TextProperty, nameof(carousel.Position), stringFormat: "pos:{0}");
            label.BindingContext = carousel;
            var indexButton = new Button { Text = "Go" };

            var layout = new Grid
            {
                RowDefinitions = new RowDefinitionCollection {
                    new RowDefinition { Height = 20 },
                    new RowDefinition { Height = 20 },
                    new RowDefinition { Height = 20 },
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                }
            };

            layout.Children.Add(indexLabel);

            layout.Children.Add(_slider);
            Grid.SetColumn(_slider, 1);

            layout.Children.Add(label);
            Grid.SetColumn(label, 2);

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { animateLabel, animateSwitch, swipeLabel, swipeSwitch, bounceLabel, bounceSwitch }
            };

            layout.Children.Add(stackLayout);
            Grid.SetRow(stackLayout, 2);
            Grid.SetColumnSpan(stackLayout, 3);

            Content = layout;
        }

        public void UpdatePositionCount(int itemsCount)
        {
            if (itemsCount > 0)
                _slider.Maximum = itemsCount - 1;
        }

        public void UpdatePosition(int position)
        {
            _slider.Value = position;
        }
    }
}