using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;
using TreeStructures.Tree;

namespace SampleConsoleApp;
public class ObservableNamedNode: ObservableGeneralTreeNode<ObservableNamedNode> {
    public string Name { get; set; }
    public override string ToString() {
        return this.Name;
    }
}

public static partial class UseageSample {
    public static void MethodB() {
        Console.WriteLine("Building a collection as an N-ary tree.");

        var nodesDic = "ABCDEFGHI".ToCharArray().Select(x => x.ToString()).ToDictionary(x => x, x => new ObservableNamedNode() { Name = x });
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

        foreach (var node in root.Preorder()) {
            node.StructureChanged += structreChangedHdlr;
            node.PropertyChanged += propertyChangedHdlr;
            node.Disposed += disposedHdlr;
        }
        Console.WriteLine("Remove node D\n");
        nodesDic["B"].RemoveChild(nodesDic["D"]);
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));

        Console.WriteLine("Add node D as a child node to node E.");
        nodesDic["E"].AddChild(nodesDic["D"]);
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));

        Console.WriteLine("Move node E to be a child node of node C.\n");
        nodesDic["C"].AddChild(nodesDic["E"]);
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));

        Console.WriteLine("Dispose node E \n");
        nodesDic["E"].Dispose();
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));


        Console.ReadLine();
    }
}



