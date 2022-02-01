using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ElementTemplate.xml" path="Type[@FullName='Microsoft.Maui.Controls.ElementTemplate']/Docs" />
	public class ElementTemplate : IElement
	{
		List<Action<object, ResourcesChangedEventArgs>> _changeHandlers;
		Element _parent;
		bool _canRecycle; // aka IsDeclarative
		readonly Type _type;

		internal ElementTemplate()
		{
		}

		internal ElementTemplate(Type type) : this()
		{
			if (type == null)
				throw new ArgumentNullException("type");

			_canRecycle = true;
			_type = type;

			LoadTemplate = () => Activator.CreateInstance(type);
		}

		internal ElementTemplate(Func<object> loadTemplate) : this() => LoadTemplate = loadTemplate ?? throw new ArgumentNullException("loadTemplate");

		/// <include file="../../docs/Microsoft.Maui.Controls/ElementTemplate.xml" path="//Member[@MemberName='LoadTemplate']/Docs" />
		public Func<object> LoadTemplate { get; set; }

		void IElement.AddResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged)
		{
			_changeHandlers = _changeHandlers ?? new List<Action<object, ResourcesChangedEventArgs>>(1);
			_changeHandlers.Add(onchanged);
		}

		internal bool CanRecycle => _canRecycle;
		internal Type Type => _type;

		Element IElement.Parent
		{
			get { return _parent; }
			set
			{
				if (_parent == value)
					return;
				if (_parent != null)
					((IElement)_parent).RemoveResourcesChangedListener(OnResourcesChanged);
				_parent = value;
				if (_parent != null)
					((IElement)_parent).AddResourcesChangedListener(OnResourcesChanged);
			}
		}

		void IElement.RemoveResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged)
		{
			if (_changeHandlers == null)
				return;
			_changeHandlers.Remove(onchanged);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ElementTemplate.xml" path="//Member[@MemberName='CreateContent']/Docs" />
		public object CreateContent()
		{
			if (LoadTemplate == null)
			{
				// Returning a Label here instead of throwing an exception because HotReload may temporarily be in state
				// where the user is creating a template; this keeps everything else (which expects a result from CreateContent)
				// from crashing during that time. 
				return new Label();
			}

			if (this is DataTemplateSelector)
				throw new InvalidOperationException("Cannot call CreateContent directly on a DataTemplateSelector");

			object item = LoadTemplate();
			SetupContent(item);

			if (item is Element elem)
				elem.IsTemplateRoot = true;

			return item;
		}

		internal virtual void SetupContent(object item)
		{
		}

		void OnResourcesChanged(object sender, ResourcesChangedEventArgs e)
		{
			if (_changeHandlers == null)
				return;
			foreach (Action<object, ResourcesChangedEventArgs> handler in _changeHandlers)
				handler(this, e);
		}
	}
}