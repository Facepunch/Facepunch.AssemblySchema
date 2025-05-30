namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public partial class Type : BaseMember
	{
		/// <summary>
		/// Loop through members and find extensions. Add those extensions to the original type.
		/// </summary>
		void DiscoverExtensions()
		{
			// we'll get to do this for properties too at some point in the future!

			foreach ( var member in Methods?.ToArray() ?? [] )
			{
				if ( !IsExtensionMethod( member.Source ) ) continue;

				IsExtension = true;
				member.IsExtension = true;
			}
		}

		/// <summary>
		/// Is this a method defined for another type
		/// </summary>
		static bool IsExtensionMethod( MethodDefinition method )
		{
			if ( !method.IsStatic || !method.DeclaringType.IsSealed || !method.DeclaringType.IsAbstract )
				return false;

			if ( !method.CustomAttributes.Any( attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute" ) )
				return false;

			return method.HasParameters && method.Parameters.Count > 0;
		}

	}
}
