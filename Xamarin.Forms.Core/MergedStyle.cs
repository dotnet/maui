using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms
{
	public partial class VisualElement
	{
		sealed class MergedStyle : IStyle
		{
			////If the base type is one of these, stop registering dynamic resources further
			////The last one (typeof(Element)) is a safety guard as we might be creating VisualElement directly in internal code
			static readonly IList<Type> s_stopAtTypes = new List<Type> { typeof(View), typeof(Layout<>), typeof(VisualElement), typeof(Element) };

			readonly BindableProperty _classStyleProperty = BindableProperty.Create("ClassStyle", typeof(IList<Style>), typeof(VisualElement), default(IList<Style>),
				propertyChanged: (bindable, oldvalue, newvalue) => ((VisualElement)bindable)._mergedStyle.OnClassStyleChanged());

			readonly List<BindableProperty> _implicitStyles = new List<BindableProperty>();

			IStyle _classStyle;

			IStyle _implicitStyle;

			IStyle _style;

			string _styleClass;

			public MergedStyle(Type targetType, BindableObject target)
			{
				Target = target;
				TargetType = targetType;
				RegisterImplicitStyles();
				Apply(Target);
			}

			public IStyle Style
			{
				get { return _style; }
				set { SetStyle(ImplicitStyle, ClassStyle, value); }
			}

			public string StyleClass
			{
				get { return _styleClass; }
				set
				{
					string val = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
					if (_styleClass == val)
						return;

					if (_styleClass != null)
						Target.RemoveDynamicResource(_classStyleProperty);

					_styleClass = val;

					if (_styleClass != null)
						Target.SetDynamicResource(_classStyleProperty, Forms.Style.StyleClassPrefix + _styleClass);
				}
			}

			public BindableObject Target { get; }

			IStyle ClassStyle
			{
				get { return _classStyle; }
				set { SetStyle(ImplicitStyle, value, Style); }
			}

			IStyle ImplicitStyle
			{
				get { return _implicitStyle; }
				set { SetStyle(value, ClassStyle, Style); }
			}

			public void Apply(BindableObject bindable)
			{
				ImplicitStyle?.Apply(bindable);
				ClassStyle?.Apply(bindable);
				Style?.Apply(bindable);
			}

			public Type TargetType { get; }

			public void UnApply(BindableObject bindable)
			{
				Style?.UnApply(bindable);
				ClassStyle?.UnApply(bindable);
				ImplicitStyle?.UnApply(bindable);
			}

			void OnClassStyleChanged()
			{
				var classStyles = Target.GetValue(_classStyleProperty) as IList<Style>;
				if (classStyles == null)
					ClassStyle = null;
				else
				{
					ClassStyle = classStyles.FirstOrDefault(s => s.CanBeAppliedTo(TargetType));
				}
			}

			void OnImplicitStyleChanged()
			{
				var first = true;
				foreach (BindableProperty implicitStyleProperty in _implicitStyles)
				{
					var implicitStyle = (Style)Target.GetValue(implicitStyleProperty);
					if (implicitStyle != null)
					{
						if (first || implicitStyle.ApplyToDerivedTypes)
						{
							ImplicitStyle = implicitStyle;
							return;
						}
					}
					first = false;
				}
			}

			void RegisterImplicitStyles()
			{
				Type type = TargetType;
				while (true)
				{
					BindableProperty implicitStyleProperty = BindableProperty.Create("ImplicitStyle", typeof(Style), typeof(VisualElement), default(Style),
						propertyChanged: (bindable, oldvalue, newvalue) => ((VisualElement)bindable)._mergedStyle.OnImplicitStyleChanged());
					Target.SetDynamicResource(implicitStyleProperty, type.FullName);
					_implicitStyles.Add(implicitStyleProperty);
					type = type.GetTypeInfo().BaseType;
					if (s_stopAtTypes.Contains(type))
						return;
				}
			}

			void SetStyle(IStyle implicitStyle, IStyle classStyle, IStyle style)
			{
				bool shouldReApplyStyle = implicitStyle != ImplicitStyle || classStyle != ClassStyle || Style != style;
				bool shouldReApplyClassStyle = implicitStyle != ImplicitStyle || classStyle != ClassStyle;
				bool shouldReApplyImplicitStyle = implicitStyle != ImplicitStyle && (Style as Style == null || ((Style)Style).CanCascade);

				if (shouldReApplyStyle)
					Style?.UnApply(Target);
				if (shouldReApplyClassStyle)
					ClassStyle?.UnApply(Target);
				if (shouldReApplyImplicitStyle)
					ImplicitStyle?.UnApply(Target);

				_implicitStyle = implicitStyle;
				_classStyle = classStyle;
				_style = style;

				if (shouldReApplyImplicitStyle)
					ImplicitStyle?.Apply(Target);
				if (shouldReApplyClassStyle)
					ClassStyle?.Apply(Target);
				if (shouldReApplyStyle)
					Style?.Apply(Target);
			}
		}
	}
}