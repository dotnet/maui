#nullable disable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	internal static class TemplateUtilities
	{
		public static async Task<Element> FindTemplatedParentAsync(Element element)
		{
			if (element.RealParent is Application)
				return null;

			var skipCount = 0;

			while (!Application.IsApplicationOrNull(element))
			{
				element = await GetRealParentAsync(element);

				if (element is IControlTemplated controlTemplated && controlTemplated.ControlTemplate != null)
				{
					if (skipCount == 0)
						return element;
					skipCount--;
				}
				if (element is ContentPresenter)
					skipCount++;
			}

			return null;
		}

		public static Task<Element> GetRealParentAsync(Element element)
		{
			Element parent = element.RealParent;
			if (parent is Application)
				return Task.FromResult<Element>(null);

			if (parent != null)
				return Task.FromResult(parent);

			var tcs = new TaskCompletionSource<Element>();
			EventHandler handler = null;
			handler = (sender, args) =>
			{
				tcs.TrySetResult(element.RealParent);
				element.ParentSet -= handler;
			};
			element.ParentSet += handler;

			return tcs.Task;
		}

		public static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (IControlTemplated)bindable;
			if (self.ControlTemplate == null)
			{
				RemoveAllChildren(self);

				if (newValue != null)
					self.InternalChildren.Add((Element)newValue);
			}
			else
			{
				if (newValue != null)
				{
					BindableObject.SetInheritedBindingContext((BindableObject)newValue, bindable.BindingContext);
				}
			}
		}

		public static void OnControlTemplateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (IControlTemplated)bindable;

			// First make sure any old ContentPresenters are no longer bound up. This MUST be
			// done before we attempt to make the new template.
            if (oldValue != null)
            {
                ClearContentPresenters((Element)self);
            }

			// Now remove all remnants of any other children just to be sure
			RemoveAllChildren(self);

			ControlTemplate template = self.ControlTemplate;
			if (template == null)
			{
				// do nothing for now
			}
			else
			{
				if (template.CreateContent() is not View content)
				{
					throw new NotSupportedException("ControlTemplate must return a type derived from View.");
				}

				self.InternalChildren.Add(content);
				self.OnControlTemplateChanged((ControlTemplate)oldValue, (ControlTemplate)newValue);
				self.TemplateRoot = content;
				self.OnApplyTemplate();
			}
		}

		private static void ClearContentPresenters(Element element)
        {
            IReadOnlyList<Element> children = element.LogicalChildrenInternal;
            for (var i = 0; i < children.Count; i++)
            {
                Element child = children[i];
                if (child is ContentPresenter presenter)
                    presenter.Clear();
                else if (child is not IControlTemplated templatedChild || templatedChild.ControlTemplate == null)
                    ClearContentPresenters(child);
            }
        }

		public static object GetTemplateChild(this IControlTemplated controlTemplated, string name)
		{
			return controlTemplated.TemplateRoot?.FindByName(name);
		}

		internal static void OnChildRemoved(IControlTemplated controlTemplated, Element removedChild)
		{
			if (removedChild == controlTemplated.TemplateRoot)
				controlTemplated.TemplateRoot = null;
		}

		private static void RemoveAllChildren(IControlTemplated self)
		{
			while (self.InternalChildren.Count > 0)
			{
				self.InternalChildren.RemoveAt(self.InternalChildren.Count - 1);
			}
		}
	}
}