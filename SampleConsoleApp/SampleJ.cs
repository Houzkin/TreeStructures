using Reactive.Bindings.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class UseageSample{
    public class DisposingObservableNamedNode : ObservableNamedNode{
		protected override IEnumerable<ObservableNamedNode> SetupPublicChildCollection(IEnumerable<ObservableNamedNode> innerCollection) {
            return (innerCollection as ObservableCollection<ObservableNamedNode>).ToFilteredReadOnlyObservableCollection(x => x.Name != "H");
			return base.SetupPublicChildCollection(innerCollection);
		}
		protected override void Dispose(bool disposing) {
            Console.WriteLine($"sender : {this} Disposing");
			base.Dispose(disposing);
		}
	}
    public class NamedNodeWrapper : BindableTreeNodeWrapper<ObservableNamedNode,NamedNodeWrapper>{
        ObservableCollection<NamedNodeWrapper> artificals = new();
        IFilteredReadOnlyObservableCollection<NamedNodeWrapper> filted;
        public NamedNodeWrapper(ObservableNamedNode node) : base(node) { }

		protected override IEnumerable<NamedNodeWrapper> SetupPublicChildCollection(CombinableChildWrapperCollection<NamedNodeWrapper> children) {
            children.AppendCollection(artificals);
            filted = children.AsReadOnlyObservableCollection().ToFilteredReadOnlyObservableCollection(x => x.Name != "N");
            return filted;
		}
		protected override NamedNodeWrapper GenerateChild(ObservableNamedNode sourceChildNode) {
            return new NamedNodeWrapper(sourceChildNode);
		}
        public virtual string Name => Source.Name;
		protected override void HandleRemovedChild(NamedNodeWrapper removedNode) {
            Console.WriteLine($"{removedNode.Name} : Removed");
			base.HandleRemovedChild(removedNode);
		}
		protected override void Dispose(bool disposing) {
            Console.WriteLine($"{this.Name} : Disposing");
			base.Dispose(disposing);
		}
		public void AddArtificalChild(string name){
            this.artificals.Add(new AnnonymousNamedWrapper(name));
        }
        public void RemoveArtificalChild(string name){
            var tgt = artificals.FirstOrDefault(x => x.Name == name);
            if (tgt != null) { artificals.Remove(tgt); }
        }
	}
    public class AnnonymousNamedWrapper : NamedNodeWrapper{
        public AnnonymousNamedWrapper(string name) : base(null) { 
            this.Name = name;
        }
        public override string Name { get; }
    }
	public static void MethodJ(){
        Console.WriteLine("Building a collection as an N-ary tree.");

        var nodesDic = "ABCDEFGHI".ToCharArray().Select(x => x.ToString()).ToDictionary(x => x, x => new DisposingObservableNamedNode() { Name = x } as ObservableNamedNode);
        var root = nodesDic.Values.AssembleAsNAryTree(2);
        //移動前のツリーを表示
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));

        EventHandler<StructureChangedEventArgs<ObservableNamedNode>> structreChangedHdlr = (s, e) => {
            Console.WriteLine($"sender:{s} \nTarget:{e.Target} TreeAction:{e.TreeAction} PreviousParentOfTarget:{e.PreviousParentOfTarget} OldIndex:{e.OldIndex} IsAncestorChanged:{e.IsAncestorChanged} IsDescendantChanged:{e.IsDescendantChanged}");
            if (e.IsAncestorChanged) {
                var info = e.AncestorInfo!;
                Console.WriteLine($"MovedTarget:{info.MovedTarget} OldIndex:{info.OldIndex} PreviousParentOfTarget:{info.PreviousParentOfTarget} IsRootChanged:{info.IsRootChanged}");
            } 
            if (e.IsDescendantChanged) {
                var info = e.DescendantInfo!;
                Console.WriteLine($"Target:{info.Target} SubtreeAction:{info.SubTreeAction} OldIndex:{info.OldIndex} PreviousParentOfTarget:{info.PreviousParentOfTarget}");
            }
            Console.Write("\n");
        };
        PropertyChangedEventHandler propertyChangedHdlr = (s, e) => { Console.WriteLine($"sender:{s} Parent Changed.\n"); };

        EventHandler disposedHdlr = (s, e) => { Console.WriteLine($"sender:{s} Disposed.\n"); };

        foreach (var node in new ObservableNamedNode[]{nodesDic["A"],nodesDic["B"],nodesDic["D"],nodesDic["H"],nodesDic["I"],nodesDic["E"] }){
            node.StructureChanged += structreChangedHdlr;
            node.PropertyChanged += propertyChangedHdlr;
            node.Disposed += disposedHdlr;
        }
        nodesDic["B"].Dispose();
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));

	}
    public static void MethodJJ(){
        var sources = "ABCDEFGHIJKLMN".ToCharArray().Select(x => x.ToString()).ToDictionary(x => x, x => new ObservableNamedNode() { Name = x });
        //assemble source tree.
        var SrcRoot = sources.Values.AssembleAsNAryTree(2);
        sources["H"].AddChild(sources["F"]);

        //wrapping source tree.
        var WrprRoot = new NamedNodeWrapper(SrcRoot);
        Console.WriteLine(WrprRoot.ToTreeDiagram(x => $"{x.Name}, IsDisposed : {x.IsDisposed}"));


        //dispose node D wrapper.
        Console.WriteLine("Dispose node D Wrapper.");
        WrprRoot.Preorder().First(x => x.Name == "D").Dispose();
        Console.WriteLine(WrprRoot.ToTreeDiagram(x => $"{x.Name}, IsDisposed : {x.IsDisposed}"));

        //remove node D.
        Console.WriteLine("Remove node D.");
        sources["D"].TryRemoveOwn();
        Console.WriteLine(WrprRoot.ToTreeDiagram(x => $"{x.Name}, IsDisposed : {x.IsDisposed}"));

        //remove node B.
        Console.WriteLine("Remove node B");
        sources["B"].TryRemoveOwn();
        Console.WriteLine(WrprRoot.ToTreeDiagram(x => $"{x.Name}, IsDisposed : {x.IsDisposed}"));

        //add dummy wrapper child.
        var nodeG = WrprRoot.Preorder().First(x => x.Name == "G");
        nodeG.AddArtificalChild("V");
        nodeG.AddArtificalChild("W");

        Console.WriteLine(WrprRoot.ToTreeDiagram(x=> x.Name));
    }
}
