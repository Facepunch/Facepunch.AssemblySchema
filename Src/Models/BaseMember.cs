using Mono.Cecil.Cil;
using System.Text.Json.Serialization;

namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public class BaseMember
	{
		[JsonPropertyName( "pb" )]
		public bool IsPublic { get; set; }

		[JsonPropertyName( "st" )]
		public bool IsStatic { get; set; }

		[JsonPropertyName( "e" )]
		public bool IsExtension { get; set; }

		[JsonPropertyName( "fn" )]
		public string FullName { get; set; }

		[JsonPropertyName( "n" )]
		public string Name { get; set; }

		[JsonPropertyName( "dt" )]
		public string DeclaringType { get; set; }

		[JsonPropertyName( "att" )]
		public List<Attribute> Attributes { get; set; }

		[JsonPropertyName( "dc" )]
		public Documentation Documentation { get; set; }

		[JsonPropertyName( "d" )]
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
		[JsonPropertyName( "f" )]
		public string File { get; set; }

		[JsonPropertyName( "l" )]
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
