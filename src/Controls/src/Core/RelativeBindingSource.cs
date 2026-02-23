#nullable disable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies a relative source for a binding, such as Self, TemplatedParent, or FindAncestor.
	/// </summary>
	public sealed class RelativeBindingSource
	{
		static RelativeBindingSource _self;
		static RelativeBindingSource _templatedParent;

		/// <summary>
		/// Creates a new <see cref="RelativeBindingSource"/> with the specified mode, ancestor type, and level.
		/// </summary>
		/// <param name="mode">The relative binding source mode.</param>
		/// <param name="ancestorType">The type of ancestor to find. Required for FindAncestor modes.</param>
		/// <param name="ancestorLevel">The level of ancestor to find (1-based). Default is 1.</param>
		public RelativeBindingSource(
			RelativeBindingSourceMode mode,
			Type ancestorType = null,
			int ancestorLevel = 1)
		{
			if ((mode == RelativeBindingSourceMode.FindAncestor ||
				 mode == RelativeBindingSourceMode.FindAncestorBindingContext) &&
				ancestorType == null)
			{
				throw new InvalidOperationException(
					$"{nameof(RelativeBindingSourceMode.FindAncestor)} and " +
					$"{nameof(RelativeBindingSourceMode.FindAncestorBindingContext)} " +
					$"require non-null {nameof(AncestorType)}");
			}

			Mode = mode;
			AncestorType = ancestorType;
			AncestorLevel = ancestorLevel;
		}

		/// <summary>
		/// Gets the relative binding source mode.
		/// </summary>
		public RelativeBindingSourceMode Mode
		{
			get;
		}

		/// <summary>
		/// Gets the ancestor type to find when using FindAncestor or FindAncestorBindingContext modes.
		/// </summary>
		public Type AncestorType
		{
			get;
		}

		/// <summary>
		/// Gets the 1-based level of ancestor to find.
		/// </summary>
		public int AncestorLevel
		{
			get;
		}

		/// <summary>
		/// Gets a <see cref="RelativeBindingSource"/> that binds to the element itself.
		/// </summary>
		public static RelativeBindingSource Self
		{
			get
			{
				return _self ?? (_self = new RelativeBindingSource(RelativeBindingSourceMode.Self));
			}
		}

		/// <summary>
		/// Gets a <see cref="RelativeBindingSource"/> that binds to the templated parent.
		/// </summary>
		public static RelativeBindingSource TemplatedParent
		{
			get
			{
				return _templatedParent ?? (_templatedParent = new RelativeBindingSource(RelativeBindingSourceMode.TemplatedParent));
			}
		}

#nullable enable

		internal async Task Apply(BindingExpression expression, Element relativeSourceTarget, BindableObject targetObject, BindableProperty targetProperty, SetterSpecificity specificity)
		{
			var bindingAdapter = new BindingExpressionAdapter(expression, targetObject, targetProperty, specificity);
			await Apply(bindingAdapter, relativeSourceTarget);
		}

		internal async Task Apply(TypedBindingBase binding, Element relativeSourceTarget, BindableObject targetObject, BindableProperty targetProperty, SetterSpecificity specificity)
		{
			var bindingAdapter = new TypedBindingAdapter(binding, targetObject, targetProperty, specificity);
			await Apply(bindingAdapter, relativeSourceTarget);
		}

		private async Task Apply(IBindingAdapter bindingAdapter, Element relativeSourceTarget)
		{
			object? resolvedSource = null;
			switch (Mode)
			{
				case RelativeBindingSourceMode.Self:
					resolvedSource = relativeSourceTarget;
					break;

				case RelativeBindingSourceMode.TemplatedParent:
					resolvedSource = await TemplateUtilities.FindTemplatedParentAsync(relativeSourceTarget);
					break;

				case RelativeBindingSourceMode.FindAncestor:
				case RelativeBindingSourceMode.FindAncestorBindingContext:
					ApplyAncestorTypeBinding(
						bindingAdapter,
						relativeSourceTarget,
						chain: new List<Element> { relativeSourceTarget },
						currentLevel: 0);
					return;

				default:
					throw new InvalidOperationException();
			}

			bindingAdapter.Apply(resolvedSource);
		}

		private void ApplyAncestorTypeBinding(
			IBindingAdapter bindingAdapter,
			Element currentElement,
			int currentLevel,
			List<Element> chain,
			object? lastMatchingBctx = null)
		{
			if (currentElement.RealParent is Application ||
				currentElement.RealParent == null)
			{
				// Couldn't find the desired ancestor type in the chain, but it may be added later, 
				// so apply with a null source for now.
				bindingAdapter.Apply(null);
				bindingAdapter.SubscribeToAncestryChanges(
					chain, includeBindingContext: Mode == RelativeBindingSourceMode.FindAncestorBindingContext, rootIsSource: false);
			}
			else if (currentElement.RealParent != null)
			{
				chain.Add(currentElement.RealParent);
				if (ElementFitsAncestorTypeAndLevel(currentElement.RealParent, ref currentLevel, ref lastMatchingBctx))
				{
					object? resolvedSource;
					if (Mode == RelativeBindingSourceMode.FindAncestor)
						resolvedSource = currentElement.RealParent;
					else
						resolvedSource = currentElement.RealParent?.BindingContext;
					bindingAdapter.Apply(resolvedSource);
					bindingAdapter.SubscribeToAncestryChanges(
						chain, includeBindingContext: Mode == RelativeBindingSourceMode.FindAncestorBindingContext, rootIsSource: true);
				}
				else
				{
					ApplyAncestorTypeBinding(bindingAdapter, currentElement.RealParent, currentLevel, chain, lastMatchingBctx);
				}
			}
			else
			{
				EventHandler? onElementParentSet = null;
				onElementParentSet = (sender, e) =>
				{
					currentElement.ParentSet -= onElementParentSet;
					ApplyAncestorTypeBinding(bindingAdapter, currentElement, currentLevel, chain, lastMatchingBctx);
				};
				currentElement.ParentSet += onElementParentSet;
			}
		}

		private bool ElementFitsAncestorTypeAndLevel(Element element, ref int level, ref object? lastPotentialBctx)
		{
			bool fitsElementType =
				Mode == RelativeBindingSourceMode.FindAncestor &&
				AncestorType!.IsAssignableFrom(element.GetType());

			bool fitsBindingContextType =
				element.BindingContext != null &&
				Mode == RelativeBindingSourceMode.FindAncestorBindingContext &&
				AncestorType!.IsAssignableFrom(element.BindingContext.GetType());

			if (!fitsElementType && !fitsBindingContextType)
				return false;

			if (fitsBindingContextType)
			{
				if (!object.ReferenceEquals(lastPotentialBctx, element.BindingContext))
				{
					lastPotentialBctx = element.BindingContext;
					level++;
				}
			}
			else
			{
				level++;
			}

			return level >= AncestorLevel;
		}

		private interface IBindingAdapter
		{
			void Apply(object? resolvedSource);
			void Unapply();
			void SubscribeToAncestryChanges(List<Element> chain, bool includeBindingContext, bool rootIsSource);
		}

		private struct BindingExpressionAdapter(
			BindingExpression expression,
			BindableObject target,
			BindableProperty property,
			SetterSpecificity specificity)
			: IBindingAdapter
		{
			public void Apply(object? resolvedSource)
				=> expression.Apply(resolvedSource, target, property, specificity);

			public void Unapply()
				=> expression.Unapply();

			public void SubscribeToAncestryChanges(List<Element> chain, bool includeBindingContext, bool rootIsSource)
				=> expression.SubscribeToAncestryChanges(chain, includeBindingContext, rootIsSource);
		}

		private struct TypedBindingAdapter(
			TypedBindingBase binding,
			BindableObject target,
			BindableProperty property,
			SetterSpecificity specificity)
			: IBindingAdapter
		{
			public void Apply(object? resolvedSource)
				=> binding.ApplyToResolvedSource(resolvedSource, target, property, false, specificity);

			public void Unapply()
				=> binding.Unapply();

			public void SubscribeToAncestryChanges(List<Element> chain, bool includeBindingContext, bool rootIsSource)
				=> binding.SubscribeToAncestryChanges(chain, includeBindingContext, rootIsSource);
		}
	}
}
