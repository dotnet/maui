#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	sealed class MergedStyle : IStyle
	{
		////If the base type is one of these, stop registering dynamic resources further
		////The last one (typeof(Element)) is a safety guard as we might be creating VisualElement directly in internal code
		static readonly IList<Type> s_stopAtTypes = new List<Type> { typeof(View), typeof(Compatibility.Layout<>), typeof(VisualElement), typeof(NavigableElement), typeof(Element) };

		IList<BindableProperty> _classStyleProperties;

		readonly List<BindableProperty> _implicitStyles = new List<BindableProperty>();

		IList<Style> _classStyles;

		IStyle _implicitStyle;

		IStyle _style;

		IList<string> _styleClass;

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
			set
			{
				if (_style == value)
					return;
				if (value != null && !value.TargetType.IsAssignableFrom(TargetType))
					Application.Current?.FindMauiContext()?.CreateLogger<Style>()?.LogWarning("Style TargetType {FullName} is not compatible with element target type {TargetType}", value.TargetType.FullName, TargetType);
				SetStyle(ImplicitStyle, ClassStyles, value);
			}
		}

		public IList<string> StyleClass
		{
			get { return _styleClass; }
			set
			{
				if (_styleClass == value)
					return;

				if (_styleClass != null && _classStyleProperties != null)
					foreach (var classStyleProperty in _classStyleProperties)
						Target.RemoveDynamicResource(classStyleProperty);

				_styleClass = value;

				if (_styleClass != null)
				{
					_classStyleProperties = new List<BindableProperty>();
					foreach (var styleClass in _styleClass)
					{
						var classStyleProperty = BindableProperty.Create("ClassStyle", typeof(IList<Style>), typeof(Element), default(IList<Style>),
							propertyChanged: (bindable, oldvalue, newvalue) => OnClassStyleChanged());
						_classStyleProperties.Add(classStyleProperty);
						Target.OnSetDynamicResource(classStyleProperty, Maui.Controls.Style.StyleClassPrefix + styleClass, SetterSpecificity.DefaultValue);
					}

					//reapply the css stylesheets
					if (Target is Element targetelement)
						targetelement.ApplyStyleSheets();
				}
			}
		}

		public BindableObject Target { get; }

		IList<Style> ClassStyles
		{
			get { return _classStyles; }
			set { SetStyle(ImplicitStyle, value, Style); }
		}

		IStyle ImplicitStyle
		{
			get { return _implicitStyle; }
			set { SetStyle(value, ClassStyles, Style); }
		}

		public void Apply(BindableObject bindable, SetterSpecificity specificity)
		{
			Apply(bindable);
		}

		void Apply(BindableObject bindable)
		{
			//NOTE specificity could be more fine grained (using distance)
			ImplicitStyle?.Apply(bindable, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
			if (ClassStyles != null)
				foreach (var classStyle in ClassStyles)
					//NOTE specificity could be more fine grained (using distance)
					((IStyle)classStyle)?.Apply(bindable, new SetterSpecificity(SetterSpecificity.StyleLocal, 0, 1, 0));
			//NOTE specificity could be more fine grained (using distance)
			Style?.Apply(bindable, new SetterSpecificity(SetterSpecificity.StyleLocal, 0, 0, 0));
		}

		public Type TargetType { get; }

		public void UnApply(BindableObject bindable)
		{
			Style?.UnApply(bindable);
			if (ClassStyles != null)
				foreach (var classStyle in ClassStyles)
					((IStyle)classStyle)?.UnApply(bindable);
			ImplicitStyle?.UnApply(bindable);
		}

		void OnClassStyleChanged()
		{
			ClassStyles = _classStyleProperties.Select(p => (Target.GetValue(p) as IList<Style>)?.FirstOrDefault(s => s.CanBeAppliedTo(TargetType))).ToList();
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

			ImplicitStyle = null;
		}

		void RegisterImplicitStyles()
		{
			Type type = TargetType;
			while (true)
			{
				BindableProperty implicitStyleProperty = BindableProperty.Create("ImplicitStyle", typeof(Style), typeof(NavigableElement), default(Style),
						propertyChanged: (bindable, oldvalue, newvalue) => OnImplicitStyleChanged());
				_implicitStyles.Add(implicitStyleProperty);
				Target.SetDynamicResource(implicitStyleProperty, type.FullName);
				type = type.BaseType;
				if (s_stopAtTypes.Contains(type))
					return;
			}
		}

		internal void ReRegisterImplicitStyles(string fallbackTypeName)
		{
			//Clear old implicit Styles
			for (var i = 0; i < _implicitStyles.Count; i++)
				Target.RemoveDynamicResource(_implicitStyles[i]);
			_implicitStyles.Clear();

			//Register the fallback
			BindableProperty implicitStyleProperty = BindableProperty.Create("ImplicitStyle", typeof(Style), typeof(NavigableElement), default(Style),
						propertyChanged: (bindable, oldvalue, newvalue) => OnImplicitStyleChanged());
			_implicitStyles.Add(implicitStyleProperty);
			Target.SetDynamicResource(implicitStyleProperty, fallbackTypeName);

			//and proceed as usual
			RegisterImplicitStyles();
			Apply(Target);
		}

		void SetStyle(IStyle implicitStyle, IList<Style> classStyles, IStyle style)
		{
			bool shouldReApplyStyle = implicitStyle != ImplicitStyle || classStyles != ClassStyles || Style != style;
			bool shouldReApplyClassStyle = implicitStyle != ImplicitStyle || classStyles != ClassStyles;
			bool shouldReApplyImplicitStyle = implicitStyle != ImplicitStyle;

			if (shouldReApplyStyle)
				Style?.UnApply(Target);
			if (shouldReApplyClassStyle && ClassStyles != null)
				foreach (var classStyle in ClassStyles)
					((IStyle)classStyle)?.UnApply(Target);
			if (shouldReApplyImplicitStyle)
				ImplicitStyle?.UnApply(Target);

			_implicitStyle = implicitStyle;
			_classStyles = classStyles;
			_style = style;

			//FIXME compute specificity
			if (shouldReApplyImplicitStyle)
				ImplicitStyle?.Apply(Target, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));

			if (shouldReApplyClassStyle && ClassStyles != null)
				foreach (var classStyle in ClassStyles)
					//FIXME compute specificity
					((IStyle)classStyle)?.Apply(Target, new SetterSpecificity(SetterSpecificity.StyleLocal, 0, 1, 0));
			if (shouldReApplyStyle)
				//FIXME compute specificity
				Style?.Apply(Target, new SetterSpecificity(SetterSpecificity.StyleLocal, 0, 0, 0));
		}
	}
}