using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.BindingSourceGen;

public enum InterceptedMethodType
{
	SetBinding,
	Create
}

public class TrackingNames
{
	public const string BindingsWithDiagnostics = nameof(BindingsWithDiagnostics);
	public const string Bindings = nameof(Bindings);
}

public sealed record BindingInvocationDescription(
	InterceptorLocation Location,
	TypeDescription SourceType,
	TypeDescription PropertyType,
	EquatableArray<IPathPart> Path,
	SetterOptions SetterOptions,
	bool NullableContextEnabled,
	InterceptedMethodType MethodType);

public sealed record SourceCodeLocation(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
	public static SourceCodeLocation? CreateFrom(Location location)
		=> location.SourceTree is null
			? null
			: new SourceCodeLocation(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);

	public Location ToLocation()
	{
		return Location.Create(FilePath, TextSpan, LineSpan);
	}

	public InterceptorLocation ToInterceptorLocation()
	{
		return new InterceptorLocation(FilePath, LineSpan.Start.Line + 1, LineSpan.Start.Character + 1);
	}
}

public sealed record InterceptorLocation(string FilePath, int Line, int Column);

public sealed record TypeDescription(
	string GlobalName,
	bool IsValueType = false,
	bool IsNullable = false,
	bool IsGenericParameter = false)
{
	public override string ToString()
		=> IsNullable
			? $"{GlobalName}?"
			: GlobalName;
}

public sealed record SetterOptions(bool IsWritable, bool AcceptsNullValue = false);

public sealed record Result<T>(T? OptionalValue, EquatableArray<DiagnosticInfo> Diagnostics)
{
	public bool HasDiagnostics => Diagnostics.Length > 0;

	public T Value => OptionalValue ?? throw new InvalidOperationException("Result does not contain a value.");

	public static Result<T> Success(T value)
	{
		return new Result<T>(value, new EquatableArray<DiagnosticInfo>(Array.Empty<DiagnosticInfo>()));
	}

	public static Result<T> Failure(EquatableArray<DiagnosticInfo> diagnostics)
	{
		return new Result<T>(default, diagnostics);
	}

	public static Result<T> Failure(DiagnosticInfo diagnostic)
	{
		return new Result<T>(default, new EquatableArray<DiagnosticInfo>(new[] { diagnostic }));
	}
}
