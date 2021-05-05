using System.Collections.Generic;

namespace Microsoft.Maui.UnitTests
{
	interface IFooService
	{
	}

	interface IBarService
	{
	}

	interface ICatService
	{
	}

	interface IFooBarService
	{
	}

	class FooService : IFooService
	{
	}

	class BadFooService : IFooService
	{
		private BadFooService()
		{
		}
	}

	class FooService2 : IFooService
	{
	}

	class BarService : IBarService
	{
	}

	class CatService : ICatService
	{
	}

	class FooEnumerableService : IFooBarService
	{
		public FooEnumerableService(IEnumerable<IFooService> foos)
		{
			Foos = foos;
		}

		public IEnumerable<IFooService> Foos { get; }
	}

	class FooBarService : IFooBarService
	{
		public FooBarService(IFooService foo, IBarService bar)
		{
			Foo = foo;
			Bar = bar;
		}

		public IFooService Foo { get; }

		public IBarService Bar { get; }
	}

	class FooTrioConstructor : IFooBarService
	{
		public FooTrioConstructor()
		{
			Option = "()";
		}

		public FooTrioConstructor(IFooService foo)
		{
			Foo = foo;
			Option = "(Foo)";
		}

		public FooTrioConstructor(IFooService foo, IBarService bar)
		{
			Foo = foo;
			Bar = bar;
			Option = "(Foo, Bar)";
		}

		public FooTrioConstructor(IFooService foo, IBarService bar, ICatService cat)
		{
			Foo = foo;
			Bar = bar;
			Cat = cat;
			Option = "(Foo, Bar, Cat)";
		}

		public IFooService Foo { get; }

		public IBarService Bar { get; }

		public ICatService Cat { get; }

		public string Option { get; }
	}

	class FooDualConstructor : IFooBarService
	{
		public FooDualConstructor(IFooService foo)
		{
			Foo = foo;
		}

		public FooDualConstructor(IBarService bar)
		{
			Bar = bar;
		}

		public IFooService Foo { get; }

		public IBarService Bar { get; }
	}

	class FooDefaultValueConstructor : IFooBarService
	{
		public FooDefaultValueConstructor(IBarService bar = null)
		{
			Bar = bar;
		}

		public IBarService Bar { get; }
	}

	class FooDefaultSystemValueConstructor : IFooBarService
	{
		public FooDefaultSystemValueConstructor(string text = "Default Value")
		{
			Text = text;
		}

		public string Text { get; }
	}

	class MappingService
	{
		readonly List<(string Key, string Value)> _pairs = new List<(string Key, string Value)>();

		public MappingService()
		{
		}

		public MappingService(string key, string value)
		{
			Add(key, value);
		}

		public int Count => _pairs.Count;

		public void Add(string key, string value)
		{
			_pairs.Add((key, value));
		}

		public IEnumerable<string> Get(string key)
		{
			foreach (var (Key, Value) in _pairs)
				if (Key == key)
					yield return Value;
		}
	}
}