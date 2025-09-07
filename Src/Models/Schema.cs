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

	/// <summary>
	/// Strip all types and members that aren't public
	/// </summary>
	public void StripNonPublic( bool keepProtected = true )
	{
		if ( Types is null )
			return;

		Types = Types.Where( x => x.IsPublic ).ToList();

		foreach ( var t in Types )
		{
			bool captureProtected = keepProtected && !t.IsSealed;

			if ( t.Methods is not null ) t.Methods = t.Methods.Where( x => x.IsPublic || (captureProtected && x.IsProtected) ).ToList();
			if ( t.Fields is not null ) t.Fields = t.Fields.Where( x => x.IsPublic || (captureProtected && x.IsProtected) ).ToList();
			if ( t.Properties is not null ) t.Properties = t.Properties.Where( x => x.IsPublic || (captureProtected && x.IsProtected) ).ToList();
			if ( t.Constructors is not null ) t.Constructors = t.Constructors.Where( x => x.IsPublic || (captureProtected && x.IsProtected) ).ToList();
		}
	}

	/// <summary>
	/// Restore all the transient data, after being deserialized
	/// </summary>
	public void Rebuild()
	{
		foreach ( var t in Types )
		{
			t.Restore( this );
		}
	}
}

