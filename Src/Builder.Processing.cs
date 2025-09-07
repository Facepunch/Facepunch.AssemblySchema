global using Mono.Cecil;

namespace Facepunch.AssemblySchema;

public partial class Builder
{
	private void ProcessAssembly( Schema info, AssemblyDefinition assembly )
	{
		// we want to find the smallest path they all have in common
		// and we'll treat this as the project root
		var rootPath = FindCommonPathPrefix( assembly );
		rootPath = rootPath.Replace( "\\", "/" ).TrimEnd( '/' );

		foreach ( var type in assembly.MainModule.Types )
		{
			info.Types.AddRange( ProcessType( assembly, type, rootPath ) );
		}
	}

	static IEnumerable<MethodDefinition> GetAllMethods( TypeDefinition type )
	{
		foreach ( var m in type.Methods )
			yield return m;

		foreach ( var nested in type.NestedTypes )
			foreach ( var nm in GetAllMethods( nested ) )
				yield return nm;
	}

	static string FindCommonPathPrefix( AssemblyDefinition assembly )
	{
		var paths = assembly.MainModule.Types
			.SelectMany( t => GetAllMethods( t ) )
			.Where( m => m.DebugInformation.HasSequencePoints )
			.SelectMany( m => m.DebugInformation.SequencePoints )
			.Select( sp => sp.Document.Url )
			.Where( x => !x.Contains( ".nuget" ) )
			.Distinct()
			.ToList();

		if ( paths.Count == 0 ) return "";

		var splitPaths = paths.Select( p => p.Split( Path.DirectorySeparatorChar ) ).ToList();
		var minLen = splitPaths.Min( s => s.Length );

		var common = new List<string>();
		for ( int i = 0; i < minLen; i++ )
		{
			var segment = splitPaths[0][i];
			if ( splitPaths.All( p => p[i] == segment ) )
				common.Add( segment );
			else
				break;
		}

		return string.Join( Path.DirectorySeparatorChar, common );
	}

	internal Documentation FindDocumentation( string name )
	{
		foreach ( var doc in Documentation )
		{
			if ( doc.Entries.TryGetValue( name, out var entry ) )
				return entry;
		}

		return null;
	}

	bool IsAttribute( TypeDefinition typeDef )
	{
		if ( typeDef?.BaseType is null ) return false;
		if ( typeDef.BaseType.FullName == "System.Attribute" ) return true;

		return IsAttribute( typeDef.BaseType.Resolve() );
	}

	private IEnumerable<Schema.Type> ProcessType( AssemblyDefinition a, TypeDefinition type, string projectPath, bool nestingTypeIsPublic = true )
	{
		if ( type.IsNestedPrivate || type.FullName == "<Module>" )
			return Array.Empty<Schema.Type>();

		if ( type.CustomAttributes.Any( x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute" ) )
			return Array.Empty<Schema.Type>();

		var t = new Schema.Type();

		t.Assembly = a.Name.Name;
		t.Source = type;
		t.IsPublic = (type.IsPublic || type.IsNestedPublic) && nestingTypeIsPublic;
		t.IsStatic = type.IsSealed && type.IsAbstract;
		t.FullName = type.FullName.Replace( "/", "." );
		t.Namespace = type.Namespace;
		t.Name = type.Name;
		t.IsAttribute = type.IsClass && IsAttribute( type );
		t.IsClass = type.IsClass;
		t.IsEnum = type.IsEnum;
		t.IsValueType = type.IsValueType;
		t.IsInterface = type.IsInterface;
		t.IsAbstract = type.IsAbstract;
		t.IsSealed = type.IsSealed;
		t.DeclaringType = type.DeclaringType?.FullName;
		t.Group = "class";
		t.Attributes = Schema.Attribute.From( type.CustomAttributes );
		t.DocumentationId = GetDocumentationId( type );
		t.Documentation = FindDocumentation( t.DocumentationId );
		t._projectPath = projectPath;

		if ( type.IsValueType ) t.Group = "struct";
		if ( type.IsInterface ) t.Group = "interface";
		if ( type.IsEnum ) t.Group = "enum";

		if ( type.HasGenericParameters )
		{
			var p = "<";

			p += string.Join( ",", type.GenericParameters.Select( x => $"{x.Name}" ) );
			p += ">";

			var paramCount = type.GenericParameters.Count;
			var replace = $"`{paramCount}";
			t.Name = t.Name.Replace( replace, p );
			t.FullName = t.FullName.Replace( replace, p );
		}

		foreach ( var member in type.Methods )
		{
			ProcessMethod( t, member );
		}

		foreach ( var property in type.Properties )
		{
			ProcessProperty( t, property );
		}

		foreach ( var field in type.Fields )
		{
			ProcessField( t, field );
		}

		var types = new List<Schema.Type> { t };

		foreach ( var nestedType in type.NestedTypes )
		{
			if ( nestedType.IsNestedPrivate ) continue;

			types.AddRange( ProcessType( a, nestedType, projectPath, t.IsPublic ) );
		}

		return types;
	}

	private void ProcessMethod( Schema.Type t, MethodDefinition member )
	{
		if ( member.IsConstructor )
		{
			ProcessConstructor( t, member );
			return;
		}

		if ( member.IsPrivate ) return;
		if ( member.IsSpecialName ) return;
		if ( member.Name is "GetHashCode" or "ToString" or "Equals" ) return;
		if ( member.SemanticsAttributes != MethodSemanticsAttributes.None ) return;

		var m = Schema.Method.From( this, t, member );

		t.Methods ??= new List<Schema.Method>();
		t.Methods.Add( m );
	}

	private void ProcessConstructor( Schema.Type t, MethodDefinition member )
	{
		if ( member.IsPrivate ) return;

		var m = Schema.Method.From( this, t, member );

		t.Constructors ??= new List<Schema.Method>();
		t.Constructors.Add( m );
	}

	private void ProcessProperty( Schema.Type t, PropertyDefinition member )
	{
		var m = Schema.Property.From( this, t, member );
		t.Properties ??= new List<Schema.Property>();
		t.Properties.Add( m );
	}

	private void ProcessField( Schema.Type t, FieldDefinition member )
	{
		if ( member.IsPrivate ) return;
		if ( member.Name == "value__" && t.IsEnum ) return;

		var m = Schema.Field.From( this, t, member );

		t.Fields ??= new();
		t.Fields.Add( m );
	}

	internal static string GetTypeName( TypeReference propertyType, string @default = null )
	{
		if ( propertyType is null ) return @default;

		var name = propertyType.FullName;
		name = name.TrimEnd( '&' ); // 'in Vector3' becomes 'Vector3&' - we don't need to see that

		return name.Replace( "+", "." ).Replace( "/", "." );
	}
}

