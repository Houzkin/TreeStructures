using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class ExtensionSample{

	public static void EnumerableSample(){

		//var list = new List<int> { 10, 21, 32, 43, 5, 65, 70, 18, 29 };
		var list = new ObservableCollection<int>(new[] { 10, 21, 32, 43, 5, 65, 70, 18, 29 });
		list.CollectionChanged += (s, e) => {
			Console.WriteLine($"{e.Action}, newIndex:{e.NewStartingIndex}, oldIndex:{e.OldStartingIndex}  newItems:{string.Join(',', e.NewItems?.OfType<int>().Select(x => x.ToString()) ?? new string[] { })}, oldItems:{string.Join(',', e.OldItems?.OfType<int>().Select(x => x.ToString()) ?? new string[] { })}");
			Console.WriteLine(string.Join(", ", list)+"\n");
		};
		Console.WriteLine(string.Join(", ", list)+"\n");
		list.AlignBy(list.OrderBy(x => x).TakeWhile(x => x < 50));

		Console.WriteLine(string.Join(", ", list)+"  ---compleate\n");
		// 5, 10, 18, 21, 29, 32, 43

		list.AlignBy(Enumerable.Range(8, 4));
		Console.WriteLine(string.Join(", ", list)+"  ---compleate\n");
		//  8, 9, 10, 11

		list.AlignBy(list.AsEnumerable().Reverse().Append(11).Append(11));
		Console.WriteLine(string.Join(", ", list)+"  ---compleate\n");
		// 11, 10, 9, 8, 11, 11

		list.AlignBy(list.AsEnumerable().Reverse().Append(10).Append(11));
		Console.WriteLine(string.Join(", ", list)+"  ---compleate\n");
		// 11, 11, 8, 9, 10, 11, 10, 11

		//list.AlignBy(Enumerable.Empty<int>());
		//list.AlignBy(list.SkipLast(1));
		//Console.WriteLine(string.Join(", ", list)+"  ---compleate\n");
		//

		list.AlignBy(Enumerable.Range(8, 4));
		Console.WriteLine(string.Join(", ", list)+"  ---compleate\n");
		// 8, 9, 10, 11

		list.AlignBy(new[] { 8,9,11,10 });
		Console.WriteLine(string.Join(", ", list)+"  ---compleate\n");
		
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
