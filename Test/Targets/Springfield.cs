
/// <summary>
/// The town where the Simpsons live.
/// </summary>
[Tag( "stinktown" )]
public partial class Springfield : Town
{
	/// <summary>
	/// A baby for 30 years
	/// </summary>
	[Tag( "baby" )]
	public float Maggie;

	/// <summary>
	/// An annoying know it all
	/// </summary>
	[Tag( "nerd" )]
	public float Lisa { get; }

	/// <summary>
	/// The world's least baddest bad boy
	/// </summary>
	/// <example>
	///		sp.Bart = 10.0f;
	///		Console.WriteLine( sp.Bart );
	///		if ( ralph )
	///		{
	///			marry( ralph );
	///		}
	/// </example>
	[Tag( "badboy" )]
	public float Bart { get; set; }

	/// <summary>
	/// Do something to another town
	/// </summary>
	public void DestroyTown( Town town )
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

	/// <summary>
	/// House is full of <a href="http://ikea.com">nice house furniture</a>. 
	/// A house is in a <see cref="Town"/>. 
	/// You can change the <see cref="House.HouseNumber"/> to change the house's house number.
	/// </summary>
	public class House
	{
		public int HouseNumber { get; set; }
	}
}

public class TagAttribute : Attribute
{
	public string Tag { get; set; }

	public TagAttribute( string tag )
	{
		Tag = tag;
	}
}

public static class SpringfieldExtensions
{
	/// <summary>
	/// Is this Springfield a good Springfield?
	/// </summary>
	public static bool IsGoodSpringfield( this Springfield springfield )
	{
		return springfield.Bart < 5.0f;
	}
}

// to test Obsolete attribute handling, make sure we remove it
public ref struct Frink
{
	public bool Professor;
}


file class MyFileClass
{
	public bool ShouldBeNonPublic { get; set; }

	public class MyFileClassNested
	{
		public bool ShouldBeNonPublicToo { get; set; }
	}
}
