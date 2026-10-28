using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TreeStructures;
using TreeStructures.Linq;
using TreeStructures.Xml.Serialization;

namespace SampleConsoleApp;

public static partial class Demo {
	public static void Assemble_1() {
		var NodeA = new NamedNode() { Name = "A" };
		var NodeB = new NamedNode() { Name = "B" };
		var NodeC = new NamedNode() { Name = "C" };
		var NodeD = new NamedNode() { Name = "D" };
		var NodeE = new NamedNode() { Name = "E" };
		var NodeF = new NamedNode() { Name = "F" };

		NodeA.AddChild(NodeB).AddChild(NodeC);
		NodeB.AddChild(NodeD);
		NodeC.AddChild(NodeE);
		NodeA.InsertChild(0, NodeF);

		Console.WriteLine(NodeA.ToTreeDiagram(x => x.Name));
		/*
		A
		├ F
		├ B
		│ └ D
		└ C
		  └ E
		*/
		Console.ReadKey();
	}
	public static void Assemble_2() {
		var nodes = "ABCDEFGH".ToCharArray().AssembleAsNAryTree(3, x => new NamedNode() { Name = x.ToString() });
		Console.WriteLine(nodes.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ ├ E
		│ ├ F
		│ └ G
		├ C
		│ └ H
		└ D
		*/
		Console.ReadKey();
	}
	public static void Assemble_3() {
		var dic = new Dictionary<NodeIndex, string>() {
			[new NodeIndex()] = "A",
			[new NodeIndex(0)] = "B",
			[new NodeIndex(1)] = "C",
			[new NodeIndex(0,0)] = "D",
			[new NodeIndex(1,0,5)] = "E",
			[new NodeIndex(1,8)] = "F",
		};
		var tree1 = dic.AssembleTree(x => new NamedNode() { Name = x });
		Console.WriteLine(tree1.ToTreeDiagram(x => x.Name));

		/* Node node "E" is excluded because there is no node corresponding to NodeIndex [1,0].
		A
		├ B
		│ └ D
		└ C
		  └ F
		 */
		try {
			var tree2 = dic.AssembleTree(x=> new NamedNode() { Name = x.ToString() }, (p, c, i) => p.InsertChild(i,c));
			Console.WriteLine(tree2.ToTreeDiagram(x => x.Name));
		}catch(ArgumentOutOfRangeException e) {
			Console.WriteLine(e.Message);

		/* An error occurs when attempting to add node "F" to node "C" with index [8].
		Index must be within the bounds of the List. (Parameter 'index')
		*/
		}
		Console.ReadKey();
	}
	public static void Assemble_4() {
		var dic = new Dictionary<NodePath<string>, NamedNode>() {
			[new NodePath<string>("A")] = new NamedNode() { Name = "A"},
			[new NodePath<string>("A","B")] = new NamedNode() { Name = "B"},
			[new NodePath<string> ("A","C","D")] = new NamedNode() { Name ="D"},
		};
		Console.WriteLine(dic.AssembleTreeByPath().ToTreeDiagram(x=>x.Name));

		/* There is no node corresponding to the node path("A", "C"). 
		A
		└ B
		*/

		var paths = new List<NodePath<string>>() {
			new NodePath<string>("A","B"),
			new NodePath<string>("A","C","D"),
		};
		var trees1 = paths.AssembleTreeByPath(path => new NamedNode() { Name = path.Last() });
		Console.WriteLine(trees1.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		└ C
		  └ D
		*/

		var paths2 = new List<NodePath<string>>() {
			new NodePath<string>("A"),
			new NodePath<string>("A","B"),
			new NodePath<string>("A","C","D"),
			new NodePath<string>("E","F","G"),
			new NodePath<string>("E","F","H"),
		};

		var trees2 = paths2.AssembleForestByPath(path => new NamedNode() { Name = path.Last() });
		foreach(var tr in trees2) {
			Console.WriteLine(tr.ToTreeDiagram(x => x.Name));
		}
		/*
		A
		├ B
		└ C
		  └ D

		E
		└ F
		  ├ G
		  └ H
		 */
		Console.ReadKey();
	}
	public static void Assemble_5() {

		var kernels = new List<NamedRow>() {
			new(){ Id = 0, ParentId = null, Name = "Unix" },
			new(){ Id = 1, ParentId = 0, Name = "SystemV" },
			new(){ Id = 2, ParentId = 0, Name = "BSD" },
			new(){ Id = 3, ParentId = 2, Name = "MacOS" },
			new(){ Id = 4, ParentId = null, Name = "Linux" },
			new(){ Id = 5, ParentId = 4, Name = "zen" },
			new(){ Id = 6, ParentId = 4, Name = "pf" },
		};
		var kernelsTree = kernels.AssembleForestById(x => x.Id, x => x.ParentId, x => new NamedNode() { Name = x.Name });
		foreach( var kernel in kernelsTree) {
			Console.WriteLine(kernel.ToTreeDiagram(x => x.Name));
		}
		/*
		Unix
		├ SystemV
		└ BSD
		  └ MacOS

		Linux
		├ zen
		└ pf
		 */
		Console.ReadKey();
	}
	public static void Assemble_6() {
		var tree = new List<NodePath<string>>() {
			new NodePath<string>("A","B"),
			new NodePath<string>("A","C","D"),
		}.AssembleTreeByPath(x => new NamedNode() { Name = x.Last() });
		Console.WriteLine(tree.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		└ C
		  └ D
		 */

		// NamedNode -> NamedLayer
		var layer = tree.Convert(x => new NamedLayer() { Name = x.Name.ToLower() }, (p, c) => p.ChildLayers.Add(c));
		PrintHierarchy(layer);
		/*
		 
		└ a
 　		  └ b
 　		  └ c
 　 　		└ d
		 */
		
		//NamedLayer -> ReadOnlyValuedTreeNode
		var valuedTreeRoot = layer.AsValuedTreeNode(x => x.ChildLayers, x => x);

		Console.WriteLine(valuedTreeRoot.ToTreeDiagram(wrapper => wrapper.Value.Name));
		/*
		a
		├ b
		└ c
		  └ d
		 */

		//ReadOnlyValuedTreeNode -> NamedNode
		var tree2 = valuedTreeRoot.Convert(x => new NamedNode() { Name = x.Value.Name.ToUpper() });
		Console.WriteLine(tree2.ToTreeDiagram(node => node.Name));
		/*
		A
		├ B
		└ C
		  └ D
		 */

		Console.ReadKey();
	}
	public static void Assemble_7() {

		var layer1 = "ABCDEF".ToCharArray()
			.AssembleAsNAryTree(3, x => new NamedLayer() { Name = x.ToString() }, x => x.ChildLayers, (p, c) => p.ChildLayers.Add(c));
		PrintHierarchy(layer1);
		/*
		└ A
 　		  └ B
 　 　		└ E
 　 　		└ F
 　		  └ C
 　		  └ D
		 */

		var layer2 = new Dictionary<NodeIndex, string>() {
			[new NodeIndex(0,1)]="D",
			[new NodeIndex(1)]="C",
			[new NodeIndex(0)]="B",
			[new NodeIndex()]="A",
		}.AssembleTree(x => new NamedLayer() { Name = x }, (p, c) => p.ChildLayers.Add(c));

		PrintHierarchy(layer2);
		/*
		└ A
 　		  └ B
 　 　		└ D
 　		  └ C
		 */

		var layer3 = new List<NodePath<string>>() {
			new NodePath<string>("A","B"),
			new NodePath<string>("A","C","D"),
			new NodePath<string>("E","F","G"),
			new NodePath<string>("E","F","H"),
		}.AssembleForestByPath(x => new NamedLayer() { Name = x.Last() }, (p, c) => p.ChildLayers.Add(c));

		foreach (var t in layer3) PrintHierarchy(t);
		/*
		└ A
		 　└ B
		 　└ C
		 　 　└ D
		└ E
		 　└ F
		 　 　└ G
		 　 　└ H
		 */

		var layer4 = new List<NamedRow>() {
			new NamedRow(){ Id = 0, ParentId = null, Name="RedHat"},
			new NamedRow(){ Id = 1, ParentId = 0, Name="Fedora"},
			new NamedRow(){ Id = 2, ParentId = 1, Name="Nobara"},
			new NamedRow(){ Id = 3, ParentId = 0, Name="SentOS"},
			new NamedRow(){ Id = 4, ParentId = null, Name="Arch"},
			new NamedRow(){ Id = 5, ParentId = 4, Name="Manjaro"},
			new NamedRow(){ Id = 6, ParentId = null, Name="Debian"},
			new NamedRow(){ Id = 7, ParentId = 6, Name="Ubuntu"},
			new NamedRow(){ Id = 8, ParentId = 7, Name="Mint"},
			new NamedRow(){ Id = 9, ParentId = 7, Name="ZorinOS"},

		}.AssembleForestById(x => x.Id, x => x.ParentId, x => new NamedLayer() { Name = x.Name }, (p, c) => p.ChildLayers.Add(c));

		foreach (var t in layer4) PrintHierarchy(t);
		/*
		└ RedHat
		 　└ Fedora
		 　 　└ Nobara
		 　└ SentOS
		└ Arch
		 　└ Manjaro
		└ Debian
		 　└ Ubuntu
		 　 　└ Mint
		 　 　└ ZorinOS
		 */

		Console.ReadKey();
	}
	public static void Transform_1() {
		var t = "ABCDEFG".ToCharArray().AssembleAsNAryTree(2, x => new NamedNode() { Name = x.ToString() });
		Console.WriteLine(t.ToTreeDiagram(x=> x.Name));
		/*
		A
		├ B
		│ ├ D
		│ └ E
		└ C
		  ├ F
		  └ G
		 */
		var dic = t.ToNodeMap(x => x.Name);
		foreach(var pair in dic) {
			Console.WriteLine($"key:{pair.Key},value:{pair.Value}");
		}
		/*
		key:[],value:A
		key:[0],value:B
		key:[1],value:C
		key:[0,0],value:D
		key:[0,1],value:E
		key:[1,0],value:F
		key:[1,1],value:G
		 */

		Console.ReadKey();
	}
	/// <summary>単一のツリーをシリアライズ・デシリアライズ</summary>
	public static void Serialize_1() {
		string path = "Text1.xml";
		var tree = new List<NodePath<string>>() {
			new("A","B","C"),
			new("A","D","E"),
		}.AssembleForestByPath(x => new NamedNode() { Name = x.Last() }).Single();

		//Serialize
		var serializableLayer = tree.Convert(x => new NamedLayer() { Name = x.Name }, (p, c) => p.ChildLayers.Add(c));
		var serializer = new XmlSerializer(typeof(NamedLayer));
		using(var sw = new StreamWriter(path)) {
			serializer.Serialize(sw, serializableLayer);
		}
		/*
		<?xml version="1.0" encoding="utf-8"?>
		<NamedLayer xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
		  <Name>A</Name>
		  <ChildLayers>
			<NamedLayer>
			  <Name>B</Name>
			  <ChildLayers>
				<NamedLayer>
				  <Name>C</Name>
				  <ChildLayers />
				</NamedLayer>
			  </ChildLayers>
			</NamedLayer>
			<NamedLayer>
			  <Name>D</Name>
			  <ChildLayers>
				<NamedLayer>
				  <Name>E</Name>
				  <ChildLayers />
				</NamedLayer>
			  </ChildLayers>
			</NamedLayer>
		  </ChildLayers>
		</NamedLayer>
		 */

		//Deserialize
		NamedLayer layer;
		using(StreamReader sr = new StreamReader(path)) {
			layer = (NamedLayer)serializer.Deserialize(sr);
		}
		var tree2 = layer.AsValuedTreeNode(x => x.ChildLayers, x => x).Convert(x => new NamedNode() { Name = x.Value.Name});
		Console.WriteLine(tree2.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ └ C
		└ D
		  └ E
		 */

		Console.ReadKey();
	}

	/// <summary>単一のツリーを平坦化してシリアライズ・デシリアライズ</summary>
	public static void Serialize_2() {
		string path = "Text2.xml";
		var tree = new List<NodePath<string>>() {
			new("A", "B", "C"),
			new("A", "D", "E"),
			new("A", "D", "F", "G"),
		}.AssembleForestByPath(x => new NamedNode() { Name = x.Last() }).Single();

		//Serialize
		var serializable = tree.ToSerializableNodeMap(x => x.Name);
		var serializer = new XmlSerializer(typeof(SerializableNodeMap<string>));
		using (StreamWriter fs = new StreamWriter(path)) {
			serializer.Serialize(fs, serializable);
		}
		/*
		<?xml version="1.0" encoding="utf-8"?>
		<SerializableNodeMapOfString>
		  <KeyValueItemOfNodeIndexCodeOfStringString>
			<Key>
			  <NodeIndex>[]</NodeIndex>
			</Key>
			<Value>A</Value>
		  </KeyValueItemOfNodeIndexCodeOfStringString>
		  <KeyValueItemOfNodeIndexCodeOfStringString>
			<Key>
			  <NodeIndex>[0]</NodeIndex>
			</Key>
			<Value>B</Value>
		  </KeyValueItemOfNodeIndexCodeOfStringString>
		  <KeyValueItemOfNodeIndexCodeOfStringString>
			<Key>
			  <NodeIndex>[1]</NodeIndex>
			</Key>
			<Value>D</Value>
		  </KeyValueItemOfNodeIndexCodeOfStringString>
		  <KeyValueItemOfNodeIndexCodeOfStringString>
			<Key>
			  <NodeIndex>[0,0]</NodeIndex>
			</Key>
			<Value>C</Value>
		  </KeyValueItemOfNodeIndexCodeOfStringString>
		  <KeyValueItemOfNodeIndexCodeOfStringString>
			<Key>
			  <NodeIndex>[1,0]</NodeIndex>
			</Key>
			<Value>E</Value>
		  </KeyValueItemOfNodeIndexCodeOfStringString>
		  <KeyValueItemOfNodeIndexCodeOfStringString>
			<Key>
			  <NodeIndex>[1,1]</NodeIndex>
			</Key>
			<Value>F</Value>
		  </KeyValueItemOfNodeIndexCodeOfStringString>
		  <KeyValueItemOfNodeIndexCodeOfStringString>
			<Key>
			  <NodeIndex>[1,1,0]</NodeIndex>
			</Key>
			<Value>G</Value>
		  </KeyValueItemOfNodeIndexCodeOfStringString>
		</SerializableNodeMapOfString>
		 */

		//Deserialize
		SerializableNodeMap<string> snm;
		using (StreamReader sr = new StreamReader(path)) {
			snm =(SerializableNodeMap<string>)serializer.Deserialize(sr);
		}
		var tree2 = snm.AssembleTree(x=> new NamedNode() { Name = x});
		Console.WriteLine(tree2.ToTreeDiagram(x => x.Name));
		/*
		A
		├ B
		│ └ C
		└ D
		  ├ E
		  └ F
			└ G
		 */
		Console.ReadKey();
	}

	/// <summary>複数のツリーを平坦化してシリアライズ・デシリアライズ</summary>
	public static void Serialize_3() {
		string path = "Text3.xml";
		var forest1 = new List<NodePath<string>>() {
			new("A", "B", "C"),
			new("A", "B", "D"),
			new("E", "F", "G"),
		}.AssembleForestByPath(x => new NamedNode() { Name = x.Last() });

		//Assign ID and ParentID to each row.
		var rowss = forest1.Select(x => x.AsValuedTreeNode(a => new NamedRow() { Name = a.Name })).ToArray();
		foreach (var pair in rowss.SelectMany(x => x.LevelOrder()).Select((x, i) => (Row: x, Index: i))) {
			pair.Row.Value.Id = pair.Index;
			pair.Row.Value.ParentId = pair.Row.Parent?.Value.Id;
		}

		//Serialize
		var rows1 = rowss.SelectMany(x => x.LevelOrder()).Select(x => x.Value).ToList();
		var serializer = new XmlSerializer(typeof(List<NamedRow>));
		using (var sw = new StreamWriter(path)) {
			serializer.Serialize(sw, rows1);
		}
		/*
		<?xml version="1.0" encoding="utf-8"?>
		<ArrayOfNamedRow xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
		  <NamedRow>
			<Id>0</Id>
			<ParentId xsi:nil="true" />
			<Name>A</Name>
		  </NamedRow>
		  <NamedRow>
			<Id>1</Id>
			<ParentId>0</ParentId>
			<Name>B</Name>
		  </NamedRow>
		  <NamedRow>
			<Id>2</Id>
			<ParentId>1</ParentId>
			<Name>C</Name>
		  </NamedRow>
		  <NamedRow>
			<Id>3</Id>
			<ParentId>1</ParentId>
			<Name>D</Name>
		  </NamedRow>
		  <NamedRow>
			<Id>4</Id>
			<ParentId xsi:nil="true" />
			<Name>E</Name>
		  </NamedRow>
		  <NamedRow>
			<Id>5</Id>
			<ParentId>4</ParentId>
			<Name>F</Name>
		  </NamedRow>
		  <NamedRow>
			<Id>6</Id>
			<ParentId>5</ParentId>
			<Name>G</Name>
		  </NamedRow>
		</ArrayOfNamedRow>
		 */

		//Deserialize
		List<NamedRow> rows2;
		using (StreamReader sr = new StreamReader(path)) {
			rows2 = (List<NamedRow>)serializer.Deserialize(sr);
		}
		var forest2 = rows2.AssembleForestById(x => x.Id, x => x.ParentId, x => new NamedNode() { Name = x.Name });
		foreach (var t in forest2) Console.WriteLine(t.ToTreeDiagram(x => x.Name));
		/*
		A
		└ B
		  ├ C
		  └ D

		E
		└ F
		  └ G
		*/
	}


	static void PrintHierarchy(NamedLayer layer,string indent = "") {
		Console.WriteLine($"{indent}└ {layer.Name}");
		foreach (var child in layer.ChildLayers) {
			PrintHierarchy(child, indent+" 　");
		}
	}

}