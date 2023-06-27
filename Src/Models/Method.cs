namespace Facepunch.AssemblySchema;

public partial class Schema
{
    public class Method : BaseMember
	{
        public string ReturnType { get; set; }
        public List<Parameter> Parameters { get; set; }

		public class Parameter
        {
            public string Name { get; set; }
            public bool IsOut { get; set; }
            public string ParameterType { get; set; }

			internal static Parameter From(ParameterDefinition x)
			{
				var a = new Parameter();
				a.Name = x.Name;
				a.IsOut = x.IsOut;
				a.ParameterType = x.ParameterType.FullName;
				return a;
			}
		}

		internal static Method From(Builder builder, Type t, MethodDefinition member)
		{
            var m = new Method();
			m.Name = member.Name;
			m.ReturnType = member.ReturnType?.FullName ?? "void";
			m.IsPublic = member.IsPublic;
			m.FullName = $"{t.FullName}.{m.Name}";
			m.IsStatic = member.IsStatic;
			m.DeclaringType = t.FullName;
			m.Attributes = Schema.Attribute.From(member.CustomAttributes);
			m.Documentation = builder.FindDocumentation($"M:{member.DeclaringType.FullName}.{member.Name}");
			m.Parameters = member.Parameters.Select(x => Parameter.From(x)).ToList();

			return m;
		}
	}
}
