using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;

namespace SampleConsoleApp;

public class NamedLayerWrapper : HierarchyWrapper<NamedLayer, NamedLayerWrapper> {
	public NamedLayerWrapper(NamedLayer source) : base(source) {
		this.Name = "(" + source.Name + ")";
		Console.WriteLine($"Create new {this.Name}");
	}
	protected override IEnumerable<NamedLayer>? SourceChildren => this.Source.ChildLayers;

	protected override NamedLayerWrapper GenerateChild(NamedLayer sourceChildNode) {
		return new NamedLayerWrapper(sourceChildNode);
	}
	protected override void HandleRemovedChild(NamedLayerWrapper removedNode) {
		Console.WriteLine($"Removed {removedNode.Name}");
	}
	public string Name { get; }
}

public class ReadOnlyLinkedNamedNode : BindableTreeNodeWrapper<ObservableNamedNode, ReadOnlyLinkedNamedNode> {
	public ReadOnlyLinkedNamedNode(ObservableNamedNode sourceNode) : base(sourceNode) {
		this.Name = "{" + sourceNode.Name + "}";
		Console.WriteLine($"Create new {this.Name}");
	}

	protected override ReadOnlyLinkedNamedNode GenerateChild(ObservableNamedNode sourceChildNode) {
		return new ReadOnlyLinkedNamedNode(sourceChildNode);
	}
	protected override void HandleRemovedChild(ReadOnlyLinkedNamedNode removedNode) {
		Console.WriteLine($"Removed {removedNode.Name}");
		base.HandleRemovedChild(removedNode);
	}
	protected override void Dispose(bool disposing) {
		//Console.WriteLine($"{this.Name} is disposing");
		Console.WriteLine($"Start disposing {this.Name}.");
		base.Dispose(disposing);
		Console.WriteLine($"Finished disposing {this.Name}.");
	}
	public string Name { get; }
}
//public class ReadOnlyDisposableNamedNode : BindableTreeNodeWrapper<ObservableNamedNode, ReadOnlyDisposableNamedNode> {
//	public ReadOnlyDisposableNamedNode(ObservableNamedNode sourceNode) : base(sourceNode) {
//		this.Name = "{" + this.Name + "}";
//	}

//	public string Name { get; }
//}

