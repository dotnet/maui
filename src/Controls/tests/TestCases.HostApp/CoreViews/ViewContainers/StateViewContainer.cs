using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	internal class StateViewContainer<T> : ViewContainer<T>
	where T : View
	{
		public Button StateChangeButton { get; private set; }
		public Label ViewInteractionLabel { get; private set; }

		public StateViewContainer(Enum formsMember, T view, Func<object, string> converterFunc = null) : base(formsMember, view)
		{
			var name = formsMember.ToString();

			var propertyName = name;
			if (name.IndexOf('_', StringComparison.OrdinalIgnoreCase) is int idx && idx > 0)
				propertyName = name[0..idx];

			var stateTitleLabel = new Label
			{
				Text = propertyName + "? "
			};

			ViewInteractionLabel = new Label
			{
				Text = "Interacted? False"
			};

			var stateValueLabel = new Label
			{
				BindingContext = view,
				AutomationId = name + "StateLabel"
			};

			var converter = new GenericValueConverter(o =>
			{
				if (converterFunc is not null)
					return converterFunc(o);

				try
				{
					var attr = o.GetType().GetCustomAttribute<TypeConverterAttribute>();
					if (attr is not null)
					{
						var name = attr.ConverterTypeName;
						var converter = Type.GetType(name);
						var instance = (TypeConverter)Activator.CreateInstance(converter);
						return instance.ConvertTo(o, typeof(string));
					}
				}
				catch (Exception)
				{
					// no-op: fall back to ToString
				}

				return o.ToString();
			});

			if (name == "Focus" || name == "Unfocused" || name == "Focused")
				stateValueLabel.SetBinding(Label.TextProperty, "IsFocused", converter: converter);
			else
				stateValueLabel.SetBinding(Label.TextProperty, propertyName, converter: converter);

			StateChangeButton = new Button
			{
				Text = "Change State: " + propertyName,
				AutomationId = name + "StateButton"
			};

			var labelLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children = {
					stateTitleLabel,
					stateValueLabel
				}
			};

			ContainerLayout.Children.Add(ViewInteractionLabel);
			ContainerLayout.Children.Add(labelLayout);
			ContainerLayout.Children.Add(StateChangeButton);
		}
	}
}
