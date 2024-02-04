
/// <summary>
/// The town where the Simpsons live.
/// </summary>
[Tag("stinktown")]
public partial class Springfield : Town
{
	/// <summary>
	/// A baby for 30 years
	/// </summary>
	[Tag("baby")]
	public float Maggie;

	/// <summary>
	/// An annoying know it all
	/// </summary>
	[Tag("nerd")]
	public float Lisa { get; }

	/// <summary>
	/// The world's least baddest bad boy
	/// </summary>
	/// <example>
	///		sp.Bart = 10.0f;
	///		Console.WriteLine( sp.Bart );
	/// </example>
	[Tag("badboy")]
	public float Bart { get; set; }

	/// <summary>
	/// Do something to another town
	/// </summary>
	public void DestroyTown(Town town)
	{

	}
}

/// <summary>
/// A town
/// </summary>
public class Town
{
	/// <summary>
	/// Name of the town
	/// </summary>
	public string TownName { get; set; }
}

public class TagAttribute : Attribute
{
	public string Tag { get; set; }

	public TagAttribute(string tag)
	{
		Tag = tag;
	}
}