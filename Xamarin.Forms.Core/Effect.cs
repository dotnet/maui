using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public abstract class Effect
	{
		internal Effect()
		{
		}

		public Element Element { get; internal set; }

		public bool IsAttached { get; private set; }

		public string ResolveId { get; internal set; }

		#region Statics

		public static Effect Resolve(string name)
		{
			Type effectType;
			Effect result = null;
			if (Internals.Registrar.Effects.TryGetValue(name, out effectType))
			{
				result = (Effect)Activator.CreateInstance(effectType);
			}

			if (result == null)
				result = new NullEffect();
			result.ResolveId = name;
			return result;
		}

		#endregion

		// Received after Control/Container/Element made valid
		protected abstract void OnAttached();

		// Received after Control/Container made invalid
		protected abstract void OnDetached();

		internal virtual void ClearEffect()
		{
			if (IsAttached)
				SendDetached();
			Element = null;
		}

		internal virtual void SendAttached()
		{
			if (IsAttached)
				return;
			OnAttached();
			IsAttached = true;
		}

		internal virtual void SendDetached()
		{
			if (!IsAttached)
				return;
			OnDetached();
			IsAttached = false;
		}

		internal virtual void SendOnElementPropertyChanged(PropertyChangedEventArgs args)
		{
		}
	}
}