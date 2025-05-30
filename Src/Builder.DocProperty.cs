namespace Facepunch.AssemblySchema;

public partial class Builder
{

	internal string GetDocumentationId( PropertyDefinition property )
	{
		return "P:" + GetTypeDocName( property.DeclaringType ) + "." + property.Name;
	}
}

