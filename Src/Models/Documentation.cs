using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;

namespace Facepunch.AssemblySchema;

public class Documentation
{
	public string Summary { get; set; }
	public string Remarks { get; set; }
	public string Return { get; set; }

	public Dictionary<string, string> Params { get; set; }
	public Dictionary<string, string> Exceptions { get; set; }
	public Dictionary<string, string> TypeParams { get; set; }
	public string[] Examples { get; set; }


	Lazy<string> _summaryPlain;

	[JsonIgnore]
	public string SummaryPlainText => _summaryPlain.Value;

	public Documentation()
	{
		_summaryPlain = new Lazy<string>( () => XmlDocumentation.XmlToPlainText( Summary ) );
	}

	// TODO - handle inherit by flattening if possible
	// public string[] Inherit { get; set; }

	/// <summary>
	/// The resolution name, this is only used during build
	/// </summary>
	[JsonIgnore]
	internal string Name { get; private set; }

	internal static Documentation ParseFromNode( XmlNode m )
	{
		if ( m.Attributes == null )
			return default;

		var d = new Documentation();

		var inherit = m.SelectNodes( "inheritdoc" );
		if ( inherit.Count > 0 )
		{
			List<string> inherits = new();
			foreach ( var x in inherit.OfType<XmlNode>() )
			{
				if ( x.Attributes["cref"] is null ) continue;

				inherits.Add( x.Attributes["cref"].Value );
			}

			// if (inherits.Count > 0)
			//     d.Inherit = inherits.ToArray();
		}

		d.Summary = CleanXml( m.SelectSingleNode( "summary" )?.InnerXml );
		d.Remarks = CleanXml( m.SelectSingleNode( "remarks" )?.InnerXml );
		d.Return = CleanXml( m.SelectSingleNode( "returns" )?.InnerXml );

		var examples = m.SelectNodes( "example" );
		if ( examples.Count > 0 )
		{
			d.Examples = examples.OfType<XmlNode>().Select( x => CleanXml( x.InnerXml ) ).ToArray();
		}

		var parms = m.SelectNodes( "param" );
		if ( parms.Count > 0 )
		{
			d.Params = new();
			foreach ( var x in parms.OfType<XmlNode>() )
			{
				var key = x.Attributes["name"].Value.Trim();
				if ( d.Params.ContainsKey( key ) ) continue;
				d.Params.Add( key, CleanXml( x.InnerXml ) );
			}
		}

		var typeparms = m.SelectNodes( "typeparam" );
		if ( typeparms.Count > 0 )
		{
			d.TypeParams = new();
			foreach ( var x in typeparms.OfType<XmlNode>() )
			{
				var key = x.Attributes["name"].Value.Trim();
				if ( d.TypeParams.ContainsKey( key ) ) continue;
				d.TypeParams.Add( key, CleanXml( x.InnerXml ) );
			}
		}

		var exceptions = m.SelectNodes( "exception" );
		if ( exceptions.Count > 0 )
		{
			d.Exceptions = new();
			foreach ( var x in exceptions.OfType<XmlNode>() )
			{
				var key = x.Attributes["cref"].Value[2..].Trim( '/' );
				if ( d.Exceptions.ContainsKey( key ) ) continue;
				d.Exceptions.Add( key, CleanXml( x.InnerXml ) );
			}
		}

		d.Name = m.Attributes["name"].Value;
		return d;
	}

	private static string CleanXml( string innerXml )
	{
		if ( innerXml is null ) return "";

		// trim newlines
		innerXml = innerXml.Trim( ['\n', '\r'] );

		// allow at most two newlines in a row
		innerXml = Regex.Replace( innerXml, @"(\r?\n){3,}", "\n\n" );

		// remove spaces at the start, if any
		innerXml = innerXml.Unindent();

		// remove any trailing bullshit
		innerXml = innerXml.TrimEnd( ['\n', '\r', '\t', ' '] );

		return innerXml;
	}

	public void InheritFrom( Documentation d )
	{
		Summary ??= d.Summary;
		Remarks ??= d.Remarks;
		Return ??= d.Return;

		foreach ( var param in d.Params.Where( x => !Params.ContainsKey( x.Key ) ) )
			Params[param.Key] = param.Value;

		foreach ( var param in d.TypeParams.Where( x => !TypeParams.ContainsKey( x.Key ) ) )
			TypeParams[param.Key] = param.Value;

		foreach ( var param in d.Exceptions.Where( x => !Exceptions.ContainsKey( x.Key ) ) )
			Exceptions[param.Key] = param.Value;
	}
}
