namespace Facepunch.AssemblySchema;

public partial class Schema
{
    public class BaseMember
    {
        public bool IsPublic { get; set; }
        public bool IsStatic { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string DeclaringType { get; set; }
        public List<Attribute> Attributes { get; set; }
        public Documentation Documentation { get; set; }
    }

}