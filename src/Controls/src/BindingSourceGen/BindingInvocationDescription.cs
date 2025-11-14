using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.BindingSourceGen;

public sealed record BindingInvocationDescription(
	InterceptableLocationRecord? InterceptableLocation,
	SimpleLocation? SimpleLocation,
	TypeDescription SourceType,
	TypeDescription PropertyType,
	EquatableArray<IPathPart> Path,
	SetterOptions SetterOptions,
	bool NullableContextEnabled,
	InterceptedMethodType MethodType,
	bool IsPublic = true,
	bool RequiresAllUnsafeGetters = false);

public sealed record InterceptableLocationRecord(int Version, string Data);

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

	public SimpleLocation ToSimpleLocation()
	{
		return new SimpleLocation(FilePath, LineSpan.Start.Line + 1, LineSpan.Start.Character + 1);
	}
}

public sealed record SimpleLocation(string FilePath, int Line, int Column);

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

public enum InterceptedMethodType
{
	SetBinding,
	Create
}

[Flags]
public enum BindingPropertyFlags
{
	None = 0,
	Mode = 1 << 0,
	Converter = 1 << 1,
	ConverterParameter = 1 << 2,
	StringFormat = 1 << 3,
	Source = 1 << 4,
	FallbackValue = 1 << 5,
	TargetNullValue = 1 << 6,
	All = Mode | Converter | ConverterParameter | StringFormat | Source | FallbackValue | TargetNullValue
}

public sealed record SetterOptions(bool IsWritable, bool AcceptsNullValue = false);
