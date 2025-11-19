# Roslyn Incremental Generators Dependency Proposal

## Motivation

Complex real-life projects depend on multiple tools and libraries, and more and more of those dependencies try to simplify developer life and minimize boilerplate code needed by using source generators.

A typical MAUI application could use:
- XAML (source generated)
- Blazor (source generated)
- MAUI Community Toolkit
- Another library for implementing observable patterns

Some of those depend on types and properties created by other source generators, and... they fail.

## Workaround

We (at MAUI) are putting multiple workarounds in place to avoid the issue:

1. **Running one source generator inside another** - We run one source generator inside another to complement the compilation. That's probably not optimal, but we're only doing this for `RegisterImplementationSourceOutput`. This doesn't scale, as it only works with source generators we know about.

2. **Heuristic-based guessing** - We guess based on heuristics and let the compiler complain.

## Proposal

There are a few proposals out there to use `After`/`Before` as MSBuild properties for ordering generators. Defining those before/after relationships is hard and requires knowledge of your potential dependencies. It would also invalidate the compilation too often and will be a performance nightmare.

### Alternate Approach

An alternate proposal I'd like to make is allowing each generator to contribute to the compilation with **only the type signature** (partial types, properties, methods). I have the feeling that the differentiation between `RegisterSourceOutput` and `RegisterImplementationSourceOutput` is what it could have been. 

> **Side note**: That's how we use it: declare API in `RegisterSourceOutput`, create implementation in `RegisterImplementationSourceOutput`. There is also `RegisterPostInitializationOutput` that actually contributes to the compilation.

Let's assume the new method is named `RegisterSourceOutputForCompilation` (I'm bad at naming, and if the ship hasn't sailed, I'd better reuse `RegisterSourceOutput`).

### Proposed Flow

This is how things could go:

1. Create an `InitialCompilation` (and an `InitialCompilationProvider`)
2. Execute all the actions registered with `RegisterSourceOutputForCompilation`
3. Collect the sources (partial types), add them to a `Compilation`, `CompilationProvider`
4. Business as usual

The `InitialCompilationProvider` (again, bad name) would have no knowledge of other source generated types (like `CompilationProvider` right now), but the `CompilationProvider` would.

**Doing this in multiple steps solves the issue of potential loops and too much invalidation of the Compilation.**

## References

After doing some research, some others have had similar ideas:
- [Roslyn Issue #57589](https://github.com/dotnet/roslyn/issues/57589) (Oct 10, 2020)
