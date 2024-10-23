namespace Microsoft.Maui.Controls.BindingSourceGen;

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
