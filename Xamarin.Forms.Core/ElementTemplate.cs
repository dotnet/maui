using System;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
#pragma warning disable 612
	public class ElementTemplate : IElement, IDataTemplate
#pragma warning restore 612
	{
		List<Action<object, ResourcesChangedEventArgs>> _changeHandlers;
		Element _parent;
		bool _canRecycle; // aka IsDeclarative

		internal ElementTemplate()
		{
		}

		internal ElementTemplate(Type type) : this()
		{
			if (type == null)
				throw new ArgumentNullException("type");

			_canRecycle = true;

			LoadTemplate = () => Activator.CreateInstance(type);
		}

		internal ElementTemplate(Func<object> loadTemplate) : this()
		{
			if (loadTemplate == null)
				throw new ArgumentNullException("loadTemplate");

			LoadTemplate = loadTemplate;
		}

		Func<object> LoadTemplate { get; set; }
#pragma warning disable 0612
		Func<object> IDataTemplate.LoadTemplate
		{
#pragma warning restore 0612
			get { return LoadTemplate; }
			set { LoadTemplate = value; }
		}

		void IElement.AddResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged)
		{
			_changeHandlers = _changeHandlers ?? new List<Action<object, ResourcesChangedEventArgs>>(1);
			_changeHandlers.Add(onchanged);
		}

		internal bool CanRecycle => _canRecycle;
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

		public object CreateContent()
		{
			if (LoadTemplate == null)
				throw new InvalidOperationException("LoadTemplate should not be null");
			if (this is DataTemplateSelector)
				throw new InvalidOperationException("Cannot call CreateContent directly on a DataTemplateSelector");

			object item = LoadTemplate();
			SetupContent(item);

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