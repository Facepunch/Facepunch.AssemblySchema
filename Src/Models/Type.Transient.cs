namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public partial class Type : BaseMember
	{
		Type _baseType;

		/// <summary>
		/// Get the actual base type, if it's in our schema. Should be null if it's not or need Restore.
		/// </summary>
		public Type GetBaseType() => _baseType;


		List<Type> _derivedTypes;

		/// <summary>
		/// Gets a list of types that derive from this type
		/// </summary>
		public List<Type> GetDerivedTypes() => _derivedTypes;

		public void Restore(Schema schema)
		{
			_baseType = schema.FindType(BaseType);
			_derivedTypes = schema.Types.Where(x => x.BaseType == FullName).ToList();

			foreach (var member in Members())
			{
				member.Restore(this, schema);
			}
		}
	}

	private Type FindType(string typeName, bool isAttribute = false)
	{
		if (isAttribute && !typeName.EndsWith("Attribute"))
			return Types.FirstOrDefault(x => x.FullName == typeName || x.FullName == $"{typeName}Attribute");

		return Types.FirstOrDefault(x => x.FullName == typeName);
	}
}