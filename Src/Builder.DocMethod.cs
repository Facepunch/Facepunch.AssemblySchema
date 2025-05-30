using System.Text;

namespace Facepunch.AssemblySchema;

public partial class Builder
{
	internal string GetDocumentationId( MethodDefinition method )
	{
		var sb = new StringBuilder();
		sb.Append( "M:" );

		// Type
		sb.Append( GetTypeDocName( method.DeclaringType ) );
		sb.Append( "." );

		// Method name or constructor
		sb.Append( method.IsConstructor ? "#ctor" : method.Name );

		// Generic arity
		if ( method.HasGenericParameters )
			sb.Append( "``" + method.GenericParameters.Count );

		// Parameters
		if ( method.HasParameters )
		{
			sb.Append( "(" );
			sb.Append( string.Join( ",", method.Parameters.Select( p => GetParameterTypeName( p.ParameterType ) ) ) );
			sb.Append( ")" );
		}

		return sb.ToString();
	}

	static string GetParameterTypeName( TypeReference type )
	{
		switch ( type )
		{
			case GenericParameter gp:
				return "``" + gp.Position;

			case GenericInstanceType git:
				var baseName = GetTypeDocName( git.ElementType );
				var args = string.Join( ",", git.GenericArguments.Select( GetParameterTypeName ) );
				return $"{baseName}{{{args}}}";

			case ArrayType at:
				return GetParameterTypeName( at.ElementType ) + "[]";

			case ByReferenceType br:
				return GetParameterTypeName( br.ElementType ) + "@";

			case PointerType pt:
				return GetParameterTypeName( pt.ElementType ) + "*";

			default:
				return type.FullName.Replace( "/", "+" );
		}
	}
}

