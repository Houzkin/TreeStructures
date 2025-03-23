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
		list.AlignBy(Enumerable.Range(8, 4));
		Console.WriteLine(string.Join(", ", list));

		list.AlignBy(list.AsEnumerable().Reverse().Append(11).Append(11));
		Console.WriteLine(string.Join(", ", list));

		list.AlignBy(list.AsEnumerable().Reverse().Append(10).Append(11));
		Console.WriteLine(string.Join(", ", list));

		list.AlignBy(new List<int>(3).AsEnumerable());
		Console.WriteLine(string.Join(", ", list));

		list.AlignBy(Enumerable.Empty<int>());
		Console.WriteLine(string.Join(", ", list));

		list.AlignBy(Enumerable.Range(8, 4));
		Console.WriteLine(string.Join(", ", list));

	}
	public static void TreeNodeSample(){

		var root = "ABbDEFdHIJKL".ToCharArray().AssembleAsNAryTree(2, x => new NamedNode() { Name = x.ToString() });
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		var nodes = root.DescendArrivals(x => x.Name, new string[] { "A", "B", "D" },Equality<string>.ComparerBy(x=>x.ToUpper()));
		Console.WriteLine(string.Join(",", nodes.Select(x => x.Name)));

		var nodetrace = root.DescendTraces(x => x.Name, new string[] { "A", "B", "D" }, Equality<string>.ComparerBy(x => x.ToUpper()));
		foreach(var trace in nodetrace){
			Console.WriteLine(string.Join(",", trace.Select(x => x.Name)));
		}

		Console.WriteLine("test");
		nodes.Last().Parent?.AddChild(nodes.First());
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));
		
		var tests = root.DescendArrivals(x => x.BranchIndex() > 0 || x.IsRoot());
		Console.WriteLine(string.Join(",", tests.Select(x => x.Name)));

		var trctest = root.DescendTraces(x=>x.BranchIndex() > 0 || x.IsRoot());
		foreach(var trace in trctest){
			Console.WriteLine(string.Join(",",trace.Select(x => x.Name)));
		}
	}
}
