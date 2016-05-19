using NUnit.Framework;
using System;

namespace assemblertests
{
	[TestFixture]
	public class Test
	{
		[Test]
		public void TestRemoveCommentsAndTrim ()
		{
			Assert.True(assembler.Assembler.Parser.RemoveCommentsAndTrim ("//").Equals (String.Empty));
			Assert.True(assembler.Assembler.Parser.RemoveCommentsAndTrim ("  //  ").Equals (String.Empty));
			Assert.True(assembler.Assembler.Parser.RemoveCommentsAndTrim ("Hello//World").Equals ("Hello"));
			Assert.True(assembler.Assembler.Parser.RemoveCommentsAndTrim ("Foo//Hello //World  ").Equals ("Foo"));
		}
	}
}

