﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Tree;

namespace SampleConsoleApp;
public class ObservableSampleNode: ObservableTreeNodeCollection<ObservableSampleNode> {
    public string Name { get; set; }
    public override string ToString() {
        return this.Name;
    }
}

public static class SampleB {
    public static void Method() {
        Console.WriteLine("コレクションをN分木として組み立てる");

        var nodesDic = "ABCDEFGHI".ToCharArray().Select(x => x.ToString()).ToDictionary(x => x, x => new ObservableSampleNode() { Name = x });
        var root = nodesDic.Values.CreateAsNAryTree(2);
        //移動前のツリーを表示
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));

        EventHandler<StructureChangedEventArgs<ObservableSampleNode>> structreChangedHdlr = (s, e) => {
            Console.WriteLine($"sender:{s} \nTarget:{e.Target} TreeAction:{e.TreeAction} PreviousParentOfTarget:{e.PreviousParentOfTarget} OldIndex:{e.OldIndex} AncestorWasChanged:{e.AncestorWasChanged} DescendantWasChanged:{e.DescendantWasChanged}");
            if (e.AncestorWasChanged) {
                var info = e.AncestorInfo!;
                Console.WriteLine($"MovedTarget:{info.MovedTarget} OldIndex:{info.OldIndex} PreviousParentOfTarget:{info.PreviousParentOfTarget} RootWasChanged:{info.RootWasChanged}");
            } 
            if (e.DescendantWasChanged) {
                var info = e.DescendantInfo!;
                Console.WriteLine($"Target:{info.Target} NodeAction:{info.NodeAction} OldIndex:{info.OldIndex} PreviousParentOfTarget:{info.PreviousParentOfTarget}");
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
        Console.WriteLine("node D を削除\n");
        nodesDic["B"].RemoveChild(nodesDic["D"]);
        //削除後のツリーを表示
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));

        Console.WriteLine("node D を node E の子ノードに追加");
        nodesDic["E"].AddChild(nodesDic["D"]);
        //追加後のツリーを表示
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));

        Console.WriteLine("node E を node C の子ノードに移動\n");
        nodesDic["C"].AddChild(nodesDic["E"]);
        //移動後のツリーを表示
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));

        Console.WriteLine("node E を Dispose\n");
        nodesDic["E"].Dispose();
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));


        Console.ReadLine();
    }
}


