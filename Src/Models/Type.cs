using System.Text.Json.Serialization;

namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public partial class Type : BaseMember
	{
		/// <summary>
		/// Used when building to root the source paths
		/// </summary>
		internal string _projectPath;

		public string Namespace { get; set; }
		public string BaseType { get; set; }
		public List<Method> Methods { get; set; }
		public List<Method> Constructors { get; set; }
		public List<Property> Properties { get; set; }
		public List<Field> Fields { get; set; }
		public bool IsAttribute { get; set; }
		public bool IsClass { get; set; }
		public bool IsInterface { get; set; }
		public bool IsAbstract { get; set; }
		public bool IsSealed { get; set; }
		public bool IsEnum { get; set; }
		public bool IsValueType { get; set; }
		public string Group { get; set; }

		[JsonPropertyName( "a" )]
		public string Assembly { get; set; }

		[JsonIgnore]
		public TypeDefinition Source { get; set; }

		/// <summary>
		/// Complete any actions that are only really possible after processing everything else.
		/// </summary>
		internal void Polish( Schema info )
		{
			if ( Source.BaseType is not null )
			{
				var baseTypeName = Source.BaseType.FullName;
				if ( Source.BaseType.IsGenericInstance ) baseTypeName = baseTypeName.Split( '<' )[0];

				BaseType = info.Types.FirstOrDefault( x => x.Source.FullName == baseTypeName )?.FullName;

				if ( BaseType is null )
				{
					BaseType = Source.BaseType.FullName;
				}
			}

			if ( BaseType == "System.Object" ) BaseType = null;
			if ( BaseType == "System.ValueType" ) BaseType = null;

			DiscoverExtensions();
		}

		/// <summary>
		/// Iterate all members
		/// </summary>
		public IEnumerable<BaseMember> Members()
		{
			if ( Methods is not null )
			{
				foreach ( var member in Methods )
				{
					yield return member;
				}
			}

			if ( Constructors is not null )
			{
				foreach ( var member in Constructors )
				{
					yield return member;
				}
			}

			if ( Properties is not null )
			{
				foreach ( var member in Properties )
				{
					yield return member;
				}
			}

			if ( Fields is not null )
			{
				foreach ( var member in Fields )
				{
					yield return member;
				}
			}

		}

		HashSet<BaseMember> _usage;
		public IEnumerable<BaseMember> GetUsage() => _usage ?? Enumerable.Empty<BaseMember>();

		internal void RegisterUsage( BaseMember member )
		{
			_usage ??= new();
			_usage.Add( member );
		}
	}

}
