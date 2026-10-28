using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Collections;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class Demo {

	public static void Traverse_1() {
		var gTree = "ABCDEF".ToCharArray().AssembleAsNAryTree(2, x => new NamedNode() { Name = x.ToString() });
		Console.WriteLine(gTree.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ ├ D
		│ └ E
		└ C
		  └ F
		 */

		var lvseaq = string.Join(',', gTree.LevelOrder());
		Console.WriteLine(lvseaq);
		//A,B,C,D,E,F

		var preseaq = string.Join(',', gTree.PreOrder());
		Console.WriteLine(preseaq);
		//A,B,D,E,C,F

		var postseaq = string.Join(',', gTree.PostOrder());
		Console.WriteLine(postseaq);
		//D,E,B,F,C,A

		var inseaq = string.Join(',', gTree.InOrder());
		Console.WriteLine(inseaq);
		//D,B,E,A,F,C

		Console.ReadKey();
	}
	public static void Traverse_2() {

		var tree = new List<NodePath<string>>() {
			new ("A","B"),
			new ("A","C","D","E"),
			new ("A","C","D","F","G"),
			new ("A","C","I"),
			new ("A","H","J"),
		}.AssembleTreeByPath(x => new NamedNode() { Name = x.Last() });

		var nodes = tree.PreOrder().ToDictionary(x => x.Name, x => x);

		Console.WriteLine(nodes["A"].ToTreeDiagram(x=>x.Name));
		/*
		A
		├ B
		├ C
		│ ├ D
		│ │ ├ E
		│ │ └ F
		│ │   └ G
		│ └ I
		└ H
		  └ J
		 */
		var nodeD = nodes["D"];
		Console.WriteLine($"Upstream:{string.Join(",",nodeD.Upstream())}");
		Console.WriteLine($"Ancestors:{string.Join(",",nodeD.Ancestors())}");
		Console.WriteLine($"Leafs:{string.Join(",",nodeD.Leafs())}");
		Console.WriteLine($"Siblings:{string.Join(",",nodeD.Siblings())}");
		Console.WriteLine($"Generations:{string.Join(",",nodeD.Generations())}");
		/*
		Upstream:D,C,A
		Ancestors:C,A
		Leafs:E,G
		Siblings:D,I
		Generations:D,I,J
		 */


		Console.WriteLine(tree.ToTreeDiagram(x => $"{x.Name}, Depth:{x.Depth()}, Height:{x.Height()}, Width:{x.Width()}, BranchIndex:{x.BranchIndex()}"));
		/*
		A, Depth:0, Height:4, Width:1, BranchIndex:-1
		├ B, Depth:1, Height:0, Width:3, BranchIndex:0
		├ C, Depth:1, Height:3, Width:3, BranchIndex:1
		│ ├ D, Depth:2, Height:2, Width:3, BranchIndex:0
		│ │ ├ E, Depth:3, Height:0, Width:2, BranchIndex:0
		│ │ └ F, Depth:3, Height:1, Width:2, BranchIndex:1
		│ │   └ G, Depth:4, Height:0, Width:1, BranchIndex:0
		│ └ I, Depth:2, Height:0, Width:3, BranchIndex:1
		└ H, Depth:1, Height:1, Width:3, BranchIndex:2
		  └ J, Depth:2, Height:0, Width:3, BranchIndex:0
		 */


		Console.WriteLine($"Root:{nodeD.Root()}");
		//Root:A

		Console.WriteLine($"NextSibling:{nodeD.NextSibling()}");
		//NextSibling:I

		nodeD.PreviousSiblingOrSelf().When(
			o => Console.WriteLine($"PreviousSibling:{o.Name}"),
			x => Console.WriteLine($"{x.Name} is first sibling."));
		//D is first sibling.

		Console.ReadKey();
	}
	public static void Search_1() {

		var root = "ABbDEFdHIJKL".ToCharArray().AssembleAsNAryTree(2, x => new NamedNode() { Name = x.ToString() });
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ ├ D
		│ │ ├ H
		│ │ └ I
		│ └ E
		│   ├ J
		│   └ K
		└ b
		  ├ F
		  │ └ L
		  └ d
		 */

		var nodes = root.DescendArrivals(x => x.Name, new string[] { "A", "B", "D" },Equality<string>.ComparerBy(x=>x.ToUpper()));
		Console.WriteLine(string.Join(",", nodes.Select(x => x.Name)));
		//D,d

		var nodetrace = root.DescendTraces(x => x.Name, new string[] { "A", "B", "D" }, Equality<string>.ComparerBy(x => x.ToUpper()));
		foreach(var trace in nodetrace){
			Console.WriteLine(string.Join(",", trace.Select(x => x.Name)));
		}
		//A,B,D
		//A,B,d

		nodes.Last().Parent?.AddChild(nodes.First());
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ └ E
		│   ├ J
		│   └ K
		└ b
		  ├ F
		  │ └ L
		  ├ d
		  └ D
			├ H
			└ I
		 */

		var tests = root.DescendArrivals(x => x.BranchIndex() > 0 || x.IsRoot());
		Console.WriteLine(string.Join(",", tests.Select(x => x.Name)));
		//d,I

		var trctest = root.DescendTraces(x=>x.BranchIndex() > 0 || x.IsRoot());
		foreach(var trace in trctest){
			Console.WriteLine(string.Join(",",trace.Select(x => x.Name)));
		}
		//A,b,d
		//A,b,D,I

	}
	public static void Edit_1() {
		var nodes = "ABCDEF".ToDictionary(x => x.ToString(), x => new NamedNode() { Name = x.ToString() });

		//Add,Insert
		nodes["A"].AddChild(nodes["B"]).AddChild(nodes["C"]).InsertChild(0, nodes["D"]).InsertChild(3, nodes["E"]);
		nodes["E"].AddChild(nodes["F"]);
		foreach(var root in nodes.Values.Where(n => n.IsRoot())) 
			Console.WriteLine(root.ToTreeDiagram(x => x.Name));
		/*
		A
		├ D
		├ B
		├ C
		└ E
		  └ F
		 */

		//Move
		nodes["B"].AddChild(nodes["C"]);
		foreach(var root in nodes.Values.Where(n => n.IsRoot())) 
			Console.WriteLine(root.ToTreeDiagram(x => x.Name));
		/*
		A
		├ D
		├ B
		│ └ C
		└ E
		  └ F
		 */

		//Remove
		nodes["A"].RemoveChild(nodes["E"]);
		foreach(var root in nodes.Values.Where(n => n.IsRoot())) 
			Console.WriteLine(root.ToTreeDiagram(x => x.Name));
		/*
		A
		├ D
		└ B
		  └ C

		E
		└ F
		 */

		//Remove all child nodes.
		nodes["A"].ClearChildren();
		foreach(var root in nodes.Values.Where(n => n.IsRoot())) 
			Console.WriteLine(root.ToTreeDiagram(x => x.Name));
		/*
		A

		B
		└ C

		D

		E
		└ F
		 */
		Console.ReadKey();
	}
	public static void Edit_2() { 
		var nodes = "ABCDEF".ToDictionary(x => x.ToString(), x => new NamedNode() { Name = x.ToString() });
		nodes.Values.AssembleAsNAryTree(2);

		Console.WriteLine(nodes["A"].ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ ├ D
		│ └ E
		└ C
		  └ F
		 */

		nodes["A"].AddChild(nodes["B"]);
		Console.WriteLine(nodes["A"].ToTreeDiagram(x => x.Name));
		/* Node B cannot be added because it is a duplicate.
		A
		├ B
		│ ├ D
		│ └ E
		└ C
		  └ F
		 */

		nodes["D"].AddChild(nodes["A"]);
		Console.WriteLine(nodes["A"].ToTreeDiagram(x => x.Name));
		/* Node A cannot be added to Node D because it would create a cycle.
		A
		├ B
		│ ├ D
		│ └ E
		└ C
		  └ F
		 */

	}
	public static void Edit_3() {
		var root1 = new List<NodePath<string>>() {
			new("A","B","C","D"),
			new("A","B","E"),
			new("A","F"),
		}.AssembleTreeByPath(x => new NamedNode() { Name = x.Last() });

		var nodes = root1.LevelOrder().ToDictionary(x => x.Name, x => x);

		foreach (var rt in nodes.Values.Where(x => x.IsRoot()))
			Console.WriteLine(rt.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ ├ C
		│ │ └ D
		│ └ E
		└ F
		 */

		var disassembleNodes = nodes["B"].Disassemble();
		Console.WriteLine("Disassemble nodes: "+  string.Join(",", disassembleNodes));
		// Disassemble nodes: 

		foreach (var rt in nodes.Values.Where(x => x.IsRoot()))
			Console.WriteLine(rt.ToTreeDiagram(x => x.Name));
		/*
		A
		└ F

		B

		C

		E

		D
		*/

		var root2 = "ABCDEFGHIJK".ToCharArray().AssembleAsNAryTree(3, x => new NamedNode() { Name = x.ToString() });
		nodes = root2.LevelOrder().ToDictionary(x => x.Name, x => x);

		foreach (var rt in nodes.Values.Where(x => x.IsRoot()))
			Console.WriteLine(rt.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ ├ E
		│ ├ F
		│ └ G
		├ C
		│ ├ H
		│ ├ I
		│ └ J
		└ D
		  └ K
		 */

		var removedDescendants = root2.RemoveAllDescendant(x => x.BranchIndex() == 1);
		Console.WriteLine("Removed Descendants: " + string.Join(",", removedDescendants));

		foreach (var rt in nodes.Values.Where(x => x.IsRoot()))
			Console.WriteLine(rt.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ ├ E
		│ └ G
		└ D
		  └ K

		C
		├ H
		├ I
		└ J

		F
		 */

		//var rv = nodes["E"].TryAddChild(nodes["A"]);
		//if (rv) {
		//	Console.WriteLine($"A child node has been added to node{rv.Value}.");
		//} else {
		//	Console.WriteLine($"Node{rv.Value} cannot be added.");
		//}

		Console.ReadKey();
	}
	public static void Edit_4() {

		var dic = "ABCDEFG".ToCharArray().Select(x => x.ToString()).ToDictionary(x => x, x => new NamedNode() { Name = x });
		var root = dic.Values.AssembleAsNAryTree(2);
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ ├ D
		│ └ E
		└ C
		  ├ F
		  └ G
		 */

		var removedNodes = root.RemoveAllDescendant(x => x.BranchIndex() == 0);
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));
		/*
		A
		└ C
		  └ G
		 */
		Console.WriteLine("Removed nodes: "+  string.Join(",", removedNodes));
		//Removed nodes: B,F

		foreach (var subtree in removedNodes) {
			Console.WriteLine(subtree.ToTreeDiagram(x => x.Name));
		}
		/*
		B
		├ D
		└ E

		F
		 */

	}
}
