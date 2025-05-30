namespace Facepunch.AssemblySchema;

public partial class Builder
{

	static string GetDocumentationId( TypeDefinition name )
	{
		return "T:" + GetTypeDocName( name );
	}

	static string GetTypeDocName( TypeReference type )
	{
		if ( type.DeclaringType != null )
			return GetTypeDocName( type.DeclaringType ) + "." + type.Name;

		return string.IsNullOrEmpty( type.Namespace ) ? type.Name : $"{type.Namespace}.{type.Name}";
	}
}

