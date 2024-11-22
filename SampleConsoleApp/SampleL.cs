using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace SampleConsoleApp {
	public static partial class UseageSample{

		public static void MethodL(){

			var root = "ADSEFSFSYELILIYOM".ToCharArray().Select(x => x.ToString())
			.AssembleAsNAryTree(2, x => new NamedNode() { Name = x });

			Console.WriteLine(root.ToTreeDiagram(x => $"{x.Name}, {x.NodeIndex()}"));

			var testseq = root.DescendFirstMatches(x => x.Name == "S");
			foreach( var x in testseq)Console.WriteLine($"{x.Name}, {x.NodeIndex()}");

		}
	}
}
