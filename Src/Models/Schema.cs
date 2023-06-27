namespace Facepunch.AssemblySchema;

/// <summary>
/// Represents the collective contents of one or more assemblies
/// </summary>
public partial class Schema
{
    /// <summary>
    /// A list of types found in this collection of assemblies
    /// </summary>
    public List<Type> Types { get; set; }
}

