using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace PopX
{
	public class Svg
	{
		public const string FileExtension = ".svg";

		public class Contour
		{
			//	todo: winding, style etc
			public List<Vector2> Points = new List<Vector2>();
		}

		public class Group
		{
			public List<Contour> Contours = new List<Contour>();
			public List<Group> Children = new List<Group>();
		};

		public string Title;
		public List<Group> Groups = new List<Group>();
		public Rect? ViewBox = null;	//	the "page". Normalise coords to this

		public Mesh CreateMesh()
		{
			throw new System.Exception("todo");
		}
	}

	class SvgImporter : AssetPostprocessor
	{
		public SvgImporter()
		{
			//throw new System.Exception("Well, don't allocate this.");
		}

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			//	this list contains assets that didn't get imported, ie. unsupported svg files
			foreach (var Filename in importedAssets)
			{
				var Extension = System.IO.Path.GetExtension(Filename);
				if (Extension != Svg.FileExtension)
					continue;

				try
				{
					var Contents = System.IO.File.ReadAllText(Filename);
					var Importer = new SvgImporter(Contents);
					var Mesh = Importer.Svg.CreateMesh();
					AssetDatabase.CreateAsset(Mesh, Filename + ".asset");
					AssetDatabase.SaveAssets();
				}
				catch(System.Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		Svg Svg;
		Dictionary<string, System.Action<Dictionary<string, string>, string,bool, bool>> TagToParserFunc;
		List<Svg.Group> CurrentGroupStack;

		//	magic strings
		const string Tag_Svg = "svg";
		const string Attrib_Svg_ViewBox = "viewBox";
		const string Tag_Group = "g";
		const string Tag_Definitions = "defs";
		const string Tag_Style = "style";
		const string Tag_Title = "title";
		const string Tag_Polyline = "polyline";
		const string Attrib_Polyline_Points = "points";

		SvgImporter(string SvgContents)
		{
			Pop.AllocIfNull(ref TagToParserFunc);
			TagToParserFunc[Tag_Svg] = ParseTag_Svg;
			TagToParserFunc[Tag_Group] = ParseTag_Group;
			TagToParserFunc[Tag_Definitions] = ParseTag_Defs;
			TagToParserFunc[Tag_Style] = ParseTag_Style;
			TagToParserFunc[Tag_Title] = ParseTag_Title;
			TagToParserFunc[Tag_Polyline] = ParseTag_Polyline;

			/*
			< svg xmlns = "http://www.w3.org/2000/svg" viewBox = "0 0 14.25 22.84" >
			< defs >
			< style >.cls - 1{
			fill: none; stroke:#0267ff;stroke-miterlimit:10;stroke-width:4px;}</style>
			</defs>
			<title>Blue Arrow</title>
			<g id="Layer_2" data-name="Layer 2">
			<g id="Layer_1-2" data-name="Layer 1">
			<polyline class="cls-1" points="1.41 1.41 11.42 11.42 1.41 21.43"/>
			</g>
			</g>
			</svg>
*/
			//	parse xml tags
			int Iterations = 0; //	stop infinite loops
			int StringIndex = 0;
			//var TagPattern = "<([/]?)([\\s]+)(.*)>";
			var TagPattern = "<([/]?)([^>\\s]+)([^>]*)>([^<]*)";
			var RegExpression = new Regex(TagPattern);
			do
			{
				var match = RegExpression.Match(SvgContents, StringIndex);
				if (match == null)
					break;
				if (match.Groups.Count < 3)
					break;
				StringIndex = match.Index + match.Length;

				var WholeMatch = match.Groups[0].Captures[0];
				var EndTag = !string.IsNullOrEmpty(match.Groups[1].Captures[0].ToString());
				var Name = match.Groups[2].Captures[0];
				var Attribs = match.Groups[3].Captures[0].Value;
				var LoneTag = string.IsNullOrEmpty(Attribs) ? false : Attribs[Attribs.Length - 1] == '/';
				Attribs = Attribs.TrimEnd(new char[]{'/'});
				var Contents = match.Groups[4].Captures[0].ToString();

				ParseTag(Name.ToString(), Attribs.ToString(), Contents, EndTag, LoneTag);
			}
			while (Iterations++ < 5000);
		}

		Dictionary<string,string> ParseAttribs(string AttribsString)
		{
			//	split at space if we're not in a quote
			bool InsideQuote = false;
			string Key = "";
			string Value = null;
			var Attribs = new Dictionary<string, string>();

			System.Action PushKeyValue = () =>
			{
				if (Key == null)
					throw new System.Exception("Expecting a key here. Double space?");
				Attribs.Add( Key, Value );
				Key = "";
				Value = null;
			};

			foreach ( var Char in AttribsString )
			{
				if ( InsideQuote && Char != '"')
				{
					Value += Char;
					continue;
				}
				
				switch( Char )
				{
					case '"':
						InsideQuote = !InsideQuote;
						break;

					case ' ':
						PushKeyValue();
						break;

					case '=':
						Value = "";
						break;

					default:
						if (Value != null)
							Value += Char;
						else
							Key += Char;
						break;
				}
			}
			//	unfinished
			PushKeyValue();

			return Attribs;
		}



		void ParseTag_Svg(Dictionary<string, string> Attribs,string TagContents,bool EndTag, bool LoneTag)
		{
			if (EndTag)
				return;
			if (Svg != null)
				throw new System.Exception("SVG already allocated (duplicate <svg> tag?)");
			Svg = new Svg();

			if (Attribs.ContainsKey(Attrib_Svg_ViewBox))
			{
				var ViewBoxStr = Attribs[Attrib_Svg_ViewBox];
				Svg.ViewBox = PopX.Math.ParseRect_xywh(ViewBoxStr);
			}
		}

		void ParseTag_Group(Dictionary<string, string> Attribs, string TagContents, bool EndTag, bool LoneTag)
		{
			Pop.AllocIfNull(ref CurrentGroupStack);
			if (!EndTag)
			{
				var NewGroup = new Svg.Group();

				//	add to tree
				if (CurrentGroupStack.Count == 0)
				{
					this.Svg.Groups.Add(NewGroup);
				}
				else
				{
					var CurrentGroup = GetCurrentGroup();
					CurrentGroup.Children.Add( NewGroup );
				}

				//	add to stack
				CurrentGroupStack.Add(NewGroup);
			}

			if ( LoneTag || EndTag )
			{
				CurrentGroupStack.RemoveAt(CurrentGroupStack.Count - 1);
				return;
			}
		}

		void ParseTag_Defs(Dictionary<string, string> Attribs, string TagContents, bool EndTag, bool LoneTag)
		{
		}
		void ParseTag_Style(Dictionary<string, string> Attribs, string TagContents, bool EndTag, bool LoneTag)
		{
		}
		void ParseTag_Title(Dictionary<string, string> Attribs, string TagContents, bool EndTag, bool LoneTag)
		{
			Svg.Title = TagContents;
		}

		void ParseTag_Polyline(Dictionary<string, string> Attribs, string TagContents, bool EndTag, bool LoneTag)
		{
			var PointsStr = Attribs[Attrib_Polyline_Points];
			var Points = PointsStr.Split(' ');
			if ((Points.Length % 2) != 0)
				throw new System.Exception("PolyLine points has odd number of " + Attrib_Polyline_Points + "'s: " + PointsStr);
			var Group = GetCurrentGroup();
			var Contour = new Svg.Contour();
			for (var i = 0; i < Points.Length; i += 2)
			{
				var x = float.Parse(Points[i + 0]);
				var y = float.Parse(Points[i + 1]);
				Contour.Points.Add(new Vector2(x, y));
			}
			Group.Contours.Add(Contour);
		}

		void ParseTag(string Tag,string AttribsString, string TagContents,bool EndTag,bool LoneTag)
		{
			var Attribs = ParseAttribs(AttribsString);

			try
			{
				TagToParserFunc[Tag](Attribs, TagContents, EndTag, LoneTag);
			}
			catch(System.Exception e)
			{
				Debug.LogError("Exception parsing Svg/XML tag " + Tag + ": " + e.Message);
				Debug.LogException(e);
				throw e;
			}
		}

		Svg.Group GetCurrentGroup()
		{
			return CurrentGroupStack[CurrentGroupStack.Count - 1];
		}
	}

}
