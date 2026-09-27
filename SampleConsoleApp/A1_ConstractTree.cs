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

			Console.WriteLine(root.ToTreeDiagram(x => $"{x.Name}, {x.GetNodeIndex()}"));

			var testseq = root.DescendFirstMatches(x => x.Name == "S");
			foreach( var x in testseq)Console.WriteLine($"{x.Name}, {x.GetNodeIndex()}");

		}
		public class CategoryRow {
			public int Id { get; set; }
			public int? ParentId { get; set; }
			public string Name { get; set; }
		}
		public static void MethodAAAA() {
			var SolarSystem = new List<CategoryRow>() {
				new() { Id = 0, ParentId = null, Name = "Sun" },
				new() { Id = 1, ParentId = 0, Name = "Mercury" },
				new() { Id = 2, ParentId = 0, Name = "Venus" },
				new() { Id = 3, ParentId = 0, Name = "Earth" },
				new() { Id = 4, ParentId = 0, Name = "Mars" },
				new() { Id = 5, ParentId = 0, Name = "Jupiter" },
				new() { Id = 6, ParentId = 0, Name = "Saturn" },
				new() { Id = 7, ParentId = 0, Name = "Uranus" },
				new() { Id = 8, ParentId = 0, Name = "Neptune" },
				new() { Id = 9, ParentId = 3, Name = "Moon" },
				new() { Id = 10, ParentId = 4, Name = "Phobos" },
				new() { Id = 11, ParentId = 4, Name = "Deimos" },
				new() { Id = 12, ParentId = 5, Name = "Io" },
				new() { Id = 13, ParentId = 5, Name = "Europa" },
				new() { Id = 14, ParentId = 5, Name = "Ganymede" },
				new() { Id = 15, ParentId = 5, Name = "Callisto" },
				new() { Id = 16, ParentId = 5, Name = "Amalthea" },
			};
			var SolarSystemTrees = SolarSystem.AssembleForestById(x => x.Id, x => x.ParentId, x => new NamedNode() { Name = x.Name });
			Console.WriteLine(SolarSystemTrees.First().ToTreeDiagram(x => x.Name));

		}
	}
}
