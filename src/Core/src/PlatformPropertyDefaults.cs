using System;
using System.Collections.Generic;

namespace Microsoft.Maui;

// TODO: Potentially make public in NET10
internal interface IPlatformPropertyDefaults
{
	Func<IElement, bool>? GetProperty(string key);
}

internal class PlatformPropertyDefaults : IPlatformPropertyDefaults
{
	private readonly Dictionary<string, Func<IElement, bool>> _mapper = new(StringComparer.Ordinal);
	readonly IPlatformPropertyDefaults[] _chained;

	public PlatformPropertyDefaults(params IPlatformPropertyDefaults[] chained)
	{
		_chained = chained;
	}

	public Func<IElement, bool>? GetProperty(string key)
	{
		if (_mapper.TryGetValue(key, out var action))
		{
			return action;
		}

		if (_chained.Length <= 0)
		{
			return null;
		}

		foreach (var ch in _chained)
		{
			var returnValue = ch.GetProperty(key);
			if (returnValue != null)
			{
				return returnValue;
			}
		}

		return null;
	}

	public void SetProperty(string key, Func<IElement, bool> hasDefaultFunc)
	{
		_mapper[key] = hasDefaultFunc;
	}
}

internal interface IPlatformPropertyDefaults<TVirtualView> : IPlatformPropertyDefaults
	where TVirtualView : IElement
{
	void Add(string key, Func<TVirtualView, bool> hasDefaultFunc);
}

internal class PlatformPropertyDefaults<TVirtualView> : PlatformPropertyDefaults, IPlatformPropertyDefaults<TVirtualView>
	where TVirtualView : IElement
{
	public PlatformPropertyDefaults(params IPlatformPropertyDefaults[] chained)
		: base(chained)
	{
	}

	public Func<TVirtualView, bool> this[string key]
	{
		get
		{
			var hasDefaultFunc = GetProperty(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
			return v => hasDefaultFunc.Invoke(v);
		}
		set => Add(key, value);
	}

	public void Add(string key, Func<TVirtualView, bool> hasDefaultFunc)
	{
		SetProperty(key, element =>
		{
			if (element is TVirtualView virtualView)
			{
				hasDefaultFunc(virtualView);
				return true;
			}

			return false;
		});
	}
}