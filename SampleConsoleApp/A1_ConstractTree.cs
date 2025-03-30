using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp {
	public static partial class UseageSample{

		public static void MethodAAA() {
			var paths = new List<NodePath<string>>() {
				new("K", "G", "F", "J"),
				new("K", "A", "B")
			};
			var tree = paths.AssembleTreeByPath(x => new OtherHierarchy(x.Last()), (p, c) => p.Nests.Add(c));
			Console.WriteLine(tree.AsValuedTreeNode(x => x.Nests, x => x).ToTreeDiagram(x => x.Value.Name));
			/*
				K
				├ G
				│ └ F
				│   └ J
				└ A
				  └ B
			 * */
		}
		public static void MethodAA(){

			var pathlist = new List<NodePath<string>>() {
				new("A","BB"),
				new("A"),
				new("A","C"),
				new("A","C","D"),
				new("A","B"),
				new("A","C","E"),
				new("A","F"),
				new("A","G","O","R"),
				new("A","B","D"),
				new("A","B","D","O"),
			};
			var pt = pathlist.AssembleTreeByPath(x => new NamedNode() { Name = x.Last() });
			Console.WriteLine(pt.ToTreeDiagram(x => x.Name));

			pt.RemoveAllDescendant(a => a.Name == "D");
			Console.WriteLine(pt.ToTreeDiagram(x => x.Name));

			var pathDic = pathlist.ToDictionary(x => x, x => new NamedNode() { Name = x.Last() });
			var ptt = pathDic.AssembleTreeByPath();
			Console.WriteLine(ptt.ToTreeDiagram(x => x.Name));

			pathlist.AddRange( new List<NodePath<string>>() { new("FF"),new("FF", "G"),});

			foreach(var p in pathlist.AssembleForestByPath(x=> new NamedNode(){ Name = x.Last() })){
				Console.WriteLine(p.ToTreeDiagram(x => x.Name));
			}

			var root = "ABSCDSESFGHIJKLMN".ToCharArray().Select(x => x.ToString())
			.AssembleAsNAryTree(2, x => new NamedNode() { Name = x });

			Console.WriteLine(root.ToTreeDiagram(x => $"{x.Name}, {x.TreeIndex()}"));

			var testseq = root.DescendFirstMatches(x => x.Name == "S");
			foreach( var x in testseq)Console.WriteLine($"{x.Name}, {x.TreeIndex()}");

		}
	}
}
