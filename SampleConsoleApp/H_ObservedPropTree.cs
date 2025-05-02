using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Events;
using TreeStructures.Linq;
using TreeStructures.Tree;

using Reactive;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;

namespace SampleConsoleApp;

public class AdditionalObj : INotifyPropertyChanged {
	PropertyChangeProxy proxy;
	public AdditionalObj() {
		proxy = new PropertyChangeProxy(this);
	}
	string title = string.Empty;
	int number = 0;

	public event PropertyChangedEventHandler? PropertyChanged {
		add { proxy.Changed += value; }
		remove { proxy.Changed -= value; }
	}
	public string Title {
		get { return title; }
		set { proxy.SetWithNotify(ref title, value); }
	}
	public int Number {
		get { return number; } 
		set { proxy.SetWithNotify(ref number, value); }
	}
	public override string ToString() {
		return "Title:" + title + ", Number:" + number.ToString();
	}
}
public class ObservableItemNode : ObservableGeneralTreeNode<ObservableItemNode>{
	public ObservableItemNode() {
	}
	AdditionalObj _info;
	string name = string.Empty;
	public AdditionalObj AdditionalInfo {
		get => _info;
		set => this.SetProperty(ref _info, value);
	}
	public string Name {
		get { return name; }
		set {  this.SetProperty(ref name, value); }
	}
	public override string ToString() {
		return Name;
	}
}
public static partial class UseageSample{
	public static void MethodH(){
		var nodeList = "ABCDEFGH".ToCharArray().ToDictionary(x=>x,x=>new ObservableItemNode(){ Name = x.ToString() });
		var tree = nodeList.Values.AssembleAsNAryTree(2);
		var arrv = tree.DescendTraces(x => x.Name, new List<string> { "A", "B" }).FirstOrDefault();
		var observingTree = new ObservedPropertyTree<ObservableItemNode>(nodeList['H']);
		var listener = observingTree.Subscribe(
			x => x.Parent.Parent.AdditionalInfo.Title,
			ttl => Console.WriteLine($"Parent.Parent.AdditionalInfo.Title:{ttl}"));
		Console.WriteLine(tree.ToTreeDiagram(x => x.Name));
		Console.WriteLine(observingTree.Root.ToTreeDiagram(x => x.NamedProperty));

		Console.WriteLine("Set info");
		nodeList.Values.ForEach(x => x.AdditionalInfo = new AdditionalObj() { Title = x.Name.ToLower() });
		//Console.WriteLine(observingTree.Root.ToTreeDiagram(x => x.NamedProperty));

		Console.WriteLine("\nSet title");
		nodeList['B'].AdditionalInfo.Title = "bB" ;

		Console.WriteLine("\nmode node H to be a child node of node C");
		nodeList['C'].AddChild(nodeList['H']);

		Console.WriteLine("\nremove node H");
		nodeList['C'].RemoveChild(nodeList['H']);
		//listener.Dispose();

		Console.WriteLine("\nA sample using ReactiveProperty");
		var disposables = new CompositeDisposable();
		var rp = observingTree
			.ToNotifyObject(x => x.Parent.AdditionalInfo.Title)
			.ObserveProperty(x => x.Value)
			.ToReactiveProperty().AddTo(disposables);
		//Console.WriteLine(observingTree.Root.ToTreeDiagram(x => x.NamedProperty));

		nodeList['B'].AddChild(nodeList['H']);
		Console.WriteLine($"ReactivePropertyValue:{rp.Value}");
		//rp.Dispose();
		//listener.Dispose();
		//observingTree.Dispose();

		Console.WriteLine("sample tree");
		Console.WriteLine(tree.ToTreeDiagram(x => x.Name));
		Console.WriteLine("nodeH observed property tree");
		Console.WriteLine(observingTree.Root.ToTreeDiagram(x => x.NamedProperty));
	}

}
