using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;

namespace SampleConsoleApp;

[Serializable]
public class NamedNode : GeneralTreeNode<NamedNode> {
    public NamedNode() { }
    public string Name { get; set; }
    public override string ToString() => this.Name;
}
public class NamedLayer {
    public string Name { get; set; }
    public List<NamedLayer> ChildLayers { get; } = new List<NamedLayer>();
	public override string ToString() => this.Name;
}
public class NamedRow {
	public int Id { get; set; }
	public int? ParentId { get; set; }
	public string Name { get; set; }
}	
public class ObservableNamedNode: ObservableGeneralTreeNode<ObservableNamedNode> {
    public string Name { get; set; }
    public override string ToString() {
        return this.Name;
    }
}
public class CustomNaryTreeNode : NAryTreeNode<CustomNaryTreeNode> {
	public CustomNaryTreeNode(int nary) : base(nary) { }
	public CustomNaryTreeNode() : this(2) { }
	public string Name { get; set; }
}
public class DspObsNamedNode : ObservableNamedNode,IDisposable {
	private bool disposedValue;
	protected override void Dispose(bool disposing) {
		if (disposedValue) return;
		if (disposing) {
			Console.WriteLine($"Start disposing {this.Name}.");
		}
		base.Dispose(disposing);
		if (disposing) {
			Console.WriteLine($"Finished disposing {this.Name}.");
		}
		disposedValue = true;
	}
	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
public class DisposableNode : ObservableGeneralTreeNode<DisposableNode>, IDisposable {
	public string Name { get; set; }

	public override string ToString() => this.Name;

	private bool disposedValue;
	protected override void Dispose(bool disposing) {
		if (disposedValue) return;
		if (disposing) {
			Console.WriteLine($"Start disposing {this.Name}.");
		}
		base.Dispose(disposing);
		if (disposing) {
			Console.WriteLine($"Finished disposing {this.Name}.");
		}
		disposedValue = true;
	}
	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
