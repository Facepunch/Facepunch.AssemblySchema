namespace Facepunch.AssemblySchema;

public partial class Builder
{

	internal string GetDocumentationId( FieldDefinition field )
	{
		return "F:" + GetTypeDocName( field.DeclaringType ) + "." + field.Name;
	}
}

