using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	public class SemanticProperties
	{
		public static readonly BindableProperty DescriptionProperty = BindableProperty.CreateAttached("Description", typeof(string), typeof(SemanticProperties), default(string));

		public static readonly BindableProperty HintProperty = BindableProperty.CreateAttached("Hint", typeof(string), typeof(SemanticProperties), default(string));

		public static readonly BindableProperty HeadingLevelProperty = BindableProperty.CreateAttached("HeadingLevel", typeof(SemanticHeadingLevel), typeof(SemanticProperties), SemanticHeadingLevel.None);

		public static string GetDescription(BindableObject bindable)
		{
			return (string)bindable.GetValue(DescriptionProperty);
		}

		public static void SetDescription(BindableObject bindable, string value)
		{
			bindable.SetValue(DescriptionProperty, value);
		}

		public static string GetHint(BindableObject bindable)
		{
			return (string)bindable.GetValue(HintProperty);
		}

		public static void SetHint(BindableObject bindable, string value)
		{
			bindable.SetValue(HintProperty, value);
		}

		public static SemanticHeadingLevel GetHeadingLevel(BindableObject bindable)
		{
			return (SemanticHeadingLevel)bindable.GetValue(HeadingLevelProperty);
		}

		public static void SetHeadingLevel(BindableObject bindable, SemanticHeadingLevel value)
		{
			bindable.SetValue(HeadingLevelProperty, value);
		}

		static BindableProperty[] _semanticPropertiesToWatch = new[]
		{
			SemanticProperties.DescriptionProperty,
			SemanticProperties.HintProperty,
			SemanticProperties.HeadingLevelProperty,
			AutomationProperties.NameProperty,
			AutomationProperties.LabeledByProperty,
			AutomationProperties.HelpTextProperty,
			AutomationProperties.IsInAccessibleTreeProperty,
		};

		// https://github.com/dotnet/maui/issues/9156
		// this is currently required because you can't bind to an attached property
		internal static ActionDisposable FakeBindSemanticProperties(BindableObject source, BindableObject dest)
		{
			foreach (var bp in _semanticPropertiesToWatch)
			{
				CopyProperty(bp, source, dest);
			}

			source.PropertyChanged += PropagateSemanticProperties;

			// The BindingContext on the template might get changed if the
			// platform is recycling the views so we need to detach the grid
			// from watching the FlyoutItem that it's associated with
			return new ActionDisposable(() => source.PropertyChanged -= PropagateSemanticProperties);

			void PropagateSemanticProperties(Object sender, PropertyChangedEventArgs args)
			{
				foreach (var bp in _semanticPropertiesToWatch)
				{
					if (args.Is(bp))
						CopyProperty(bp, source, dest);
				}
			}

			void CopyProperty(BindableProperty bp, BindableObject source, BindableObject dest)
			{
				if (source.IsSet(bp))
					dest.SetValue(bp, source.GetValue(bp));
			}
		}
	}
}
