using Mono.Cecil.Cil;
using System.Text.Json.Serialization;

namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public class BaseMember
	{
		public bool IsPublic { get; set; }
		public bool IsProtected { get; set; }
		public bool IsStatic { get; set; }
		public bool IsExtension { get; set; }
		public string FullName { get; set; }
		public string Name { get; set; }
		public string DeclaringType { get; set; }

		public List<Attribute> Attributes { get; set; }
		public Documentation Documentation { get; set; }

		[JsonPropertyName( "DocId" )]
		public string DocumentationId { get; set; }

		Type _declaringType;
		public Type GetDeclaringType() => _declaringType;

		internal virtual void Restore( Type type, Schema schema )
		{
			_declaringType = type;

			if ( Attributes is not null )
			{
				foreach ( var attr in Attributes )
				{
					attr.Restore( this, type, schema );
				}
			}
		}
	}

	public class Location
	{
		[JsonPropertyName( "File" )]
		public string File { get; set; }

		[JsonPropertyName( "Line" )]
		public int Line { get; set; }

		internal static Location From( Builder builder, MethodDefinition member, string projectPath )
		{
			SequencePoint sp = default;
			sp = member.DebugInformation?.SequencePoints?.FirstOrDefault();
			if ( sp == null ) return null;

			var file = sp.Document.Url.Replace( "\\", "/" );
			file = file[projectPath.Length..].TrimStart( '/' );

			return new Location() { File = file, Line = sp.StartLine };
		}
	}

}
