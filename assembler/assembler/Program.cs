using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace assembler
{
	public class Assembler
	{
		public static class Parser
		{
			static IEnumerable<IEnumerable<string>> Parse( IEnumerable<string> lines )
			{
				//remove cruft
				var cleanLines = lines.Select (RemoveCommentsAndTrim)
					.Where (line => line.Length != 0)
					.ToList ();

				Dictionary<string,string> symbolLookup = new Dictionary<string, string> 
				{
					{ "R0", "0" },
					{ "R1", "1" },
					{ "R2", "2" },
					{ "R3", "3" },
					{ "R4", "4" },
					{ "R5", "5" },
					{ "R6", "6" },
					{ "R7", "7" },
					{ "R8", "8" },
					{ "R9", "9" },
					{ "R1O", "10" },
					{ "R11", "11" },
					{ "R12", "12" },
					{ "R13", "13" },
					{ "R14", "14" },
					{ "R15", "15" },
				};

				//first pass build up a dictionary of symbols
				int removeCount = 0;
				for( int i = 0; i < cleanLines.Count; ++i )
				{
					var line = cleanLines [i];
					if( line.StartsWith("("))
					{
						var address = i + 1 - removeCount;

						var symbol = line.Replace("(","").Replace(")","");

						symbolLookup.Add (symbol, address.ToString());
						removeCount++;
					}

						
				}

				var removed = cleanLines.RemoveAll (line => line.StartsWith ("("));

				if( removed != removeCount )
				{
					Console.WriteLine ("OOPS");
				}

				return cleanLines.Select (cl => Parse( cl, symbolLookup));

			}
	
			static IEnumerable<string> Parse( string line, Dictionary<string,string> symbolLookup )
			{
				//identify if the line is an a instruction or a c instruction, a instructions begin with an @
				if( line.StartsWith("@") )
				{
					yield return "ADD_START";
	
					var stripAt =  line.Replace("@","");
					int address = -1;
					if(int.TryParse(stripAt,out address))
					{
						yield return address.ToString ();
					}
					else
					{
						yield return symbolLookup [stripAt];
					}


					yield return "ADD_END";
				}
				else
				{
					//this is a c instruction so will be of the form 
					//dest=comp;jump
					//so lets split into the parts


					string dest = "";
					string jump = "NO_JUMP";

					if(line.Contains("="))
					{
						var parts = line.Split(new [] {'='}, StringSplitOptions.None);

						line = parts[1];
						dest = parts[0];
					}

					string comp = line;
					if(line.Contains(";"))
					{
						var parts = line.Split(new [] {";"}, StringSplitOptions.None);
						comp = parts[0];
						jump = parts[1];
					}

					yield return "INST_START";

					yield return "DEST_START";
					foreach( var c in dest )
					{
						yield return c.ToString();
					}
					yield return "DEST_END";

					yield return "COMP_START";
	
					yield return comp;

					yield return "COMP_END";

					yield return "JUMP_START";
					yield return jump;
					yield return "JUMP_END";

					yield return "INST_END";
				}

			}

			public static string RemoveCommentsAndTrim( string input )
			{
				return input.Split (new []{ "//" }, StringSplitOptions.None) [0].Trim ().Replace(" ", String.Empty );
			}

			public static IEnumerable<IEnumerable<string>> ParseFile( string filepath )
			{
				if(File.Exists(filepath) == false )
				{
					throw new ArgumentException ("File " + filepath + " does not exist");
				}
					
				return Parse( File.ReadLines(filepath ) );

			}
		}

		public static class Converter 
		{
			static Dictionary<string,string> JumpLookup = new Dictionary<string, string>
			{
				{ "JGT", "001"},
				{ "JEQ", "010"},
				{ "JGE", "011"},
				{ "JLT", "100"},
				{ "JNE", "101"},
				{ "JLE", "110"},
				{ "JMP", "111"}
			};

			static Dictionary<string,string> CompLookup = new Dictionary<string, string>
			{
				{"0", "101010"},
				{"1", "111111"},
				{"-1","111010"},
				{"D","001100"},
				{"A","110000"},
				{"!D","001101"},
				{"!A","110001"},
				{"-D","001111"},
				{"-A","110011"},
				{"D+1","011111"},
				{"A+1","110111"},
				{"D-1","001110"},
				{"A-1","110010"},
				{"D+A","000010"},
				{"D-A","010011"},
				{"A-D","000111"},
				{"D&A","000000"},
				{"D|A","010101"}
			};

			public static string Convert (IEnumerable<string> tokens)
			{
				var start = tokens.First();

				if( start == "ADD_START")
				{
					return ConvertAddress (tokens);

				}

				if( start == "INST_START")
				{
					return ConvertInstruction (tokens);
				}

				throw new NotImplementedException ();
			}


			private static string ConvertAddress( IEnumerable<string> tokens)
			{
				//we only want the second item
				var address = tokens.Skip (1).First ();

				var value = uint.Parse (address);

				var binary = System.Convert.ToString (value, 2);

				//now pad with zeros to make a 16 bit value
				return binary.PadLeft (16, '0');
			}

			static string ConvertInstruction (IEnumerable<string> tokens)
			{
				var dest = tokens.SkipWhile ((str, i) => str != "DEST_START").TakeWhile ((str, i) => str != "DEST_END");
				var comp = tokens.SkipWhile ((str, i) => str != "COMP_START").TakeWhile ((str, i) => str != "COMP_END");
				var jump = tokens.SkipWhile ((str, i) => str != "JUMP_START").TakeWhile ((str, i) => str != "JUMP_END");

				var destBits = string.Empty;
				destBits += dest.Contains("A") ? "1" : "0";
				destBits += dest.Contains("D") ? "1" : "0";
				destBits += dest.Contains("M") ? "1" : "0";

				var compBits = string.Empty;


				var actualComp = comp.Skip (1).First ();

				var aBit = actualComp.Contains ("M") ? "1" : "0";

				if( aBit == "1" )
				{
					compBits = CompLookup [actualComp.Replace ("M", "A")];
				}
				else
				{
					compBits = CompLookup[actualComp];
				}

				var jumpType = jump.Skip (1).First ();

				var jumpBits = JumpLookup.ContainsKey(jumpType) ? JumpLookup[jumpType] : "000";

				var firstBits = "111";

				return firstBits + aBit + compBits + destBits + jumpBits;
			}
		}

		public static void Main (string[] args)
		{
			if(args.Length != 1)
			{
				throw new ArgumentException ("Expected one argument. Assembly file to be assembled");
			}
			string filepath = args [0];

			var parsed = Parser.ParseFile( filepath ).ToList();

			var converted = parsed.Select (Converter.Convert);

			File.WriteAllLines ("out.hack", converted);
			File.WriteAllLines ("out.asm", parsed.SelectMany (p => p));


			Console.WriteLine ("Done!");
		}


	}
}
