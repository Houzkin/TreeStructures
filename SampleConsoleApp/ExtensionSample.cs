using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class ExtensionSample{

	public static void EnumerableSample(){

		var list = new List<int> { 10, 21, 32, 43, 5, 65, 70, 18, 29 };
		list.AlignBy(list.OrderBy(x => x).TakeWhile(x => x < 50));

		Console.WriteLine(string.Join(", ", list));
		// 5, 10, 18, 21, 29, 32, 43
	}
	public static void TreeNodeSample(){

		var root = "ABbDEFdHIJK".ToCharArray().AssembleAsNAryTree(2, x => new NamedNode() { Name = x.ToString() });
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		var nodes = root.DescendArrivals(x => x.Name, new string[] { "A", "B", "D" },Equality<string>.ComparerBy(x=>x.ToUpper()));
		Console.WriteLine(string.Join(",", nodes.Select(x => x.Name)));

		var nodetrace = root.DescendTraces(x => x.Name, new string[] { "A", "B", "D" }, Equality<string>.ComparerBy(x => x.ToUpper()));
		foreach(var trace in nodetrace){
			Console.WriteLine(string.Join(",", trace.Select(x => x.Name)));
		}
	}
}
