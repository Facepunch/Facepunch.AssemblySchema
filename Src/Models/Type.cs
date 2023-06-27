using System.Text.Json.Serialization;

namespace Facepunch.AssemblySchema;

public partial class Schema
{
    public class Type : BaseMember
	{
		public string Namespace { get; set; }
		public string BaseType { get; set; }
        public List<Method> Methods { get; set; }
        public List<Method> Constructors { get; set; }
        public List<Property> Properties { get; set; }
        public List<Field> Fields { get; set; }
        public bool IsClass { get; set; }
        public bool IsInterface { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsSealed { get; set; }
        public bool IsEnum { get; set; }
        public bool IsValueType { get; set; }
        public string Group { get; set; }

        [JsonIgnore]
        public TypeDefinition Source { get; set; }

        /// <summary>
        /// Complete any actions that are only really possible after processing everything else.
        /// </summary>
        internal void Polish(Schema info)
        {
            if (Source.BaseType is not null)
            {
                var baseTypeName = Source.BaseType.FullName;
                if (Source.BaseType.IsGenericInstance) baseTypeName = baseTypeName.Split('<')[0];

                BaseType = info.Types.FirstOrDefault(x => x.Source.FullName == baseTypeName)?.FullName;

                if (BaseType is null)
                {
                    BaseType = Source.BaseType.FullName;
                }
            }

            if (BaseType == "System.Object") BaseType = null;
            if (BaseType == "System.ValueType") BaseType = null;
        }
    }

}