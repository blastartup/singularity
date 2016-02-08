using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Singularity.Test
{
	[TestClass]
	public class ExtensionTest
	{
		[TestMethod]
		public void TestFormatWith()
		{
			var entiy = new EntityDto();
			var result = normalTemplate.FormatWith(entiy);
			Assert.IsTrue(result == "Chris went to School today.", "FormatWith worked for normal tags.");

			result = abnormalTemplate.FormatWith(entiy, "«", "»");
			Assert.IsTrue(result == "Chris went to School today.", "FormatWith worked for abnormal tags.");
		}

		private String normalTemplate = "{{Child}} went to {{Place}} today.";
		private String abnormalTemplate = "«Child» went to «Place» today.";

		class EntityDto
		{
			public String Child { get { return "Chris"; } }
			public String Place { get { return "School"; } }
		}
	}
}
