using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Linq;
using TreeStructures.Tree;
using TreeStructures.Events;

namespace SampleConsoleApp;

public static partial class UseageSample {
	public static void MethodO() {

		var obsList = new ObservableCollection<ObservableItemNode>("ABCD".Select(x => new ObservableItemNode() { Name = x.ToString() }));

		var observer = new ReadOnlyObservableTrackingCollection<ObservableItemNode>(obsList);

		
		var listener0 = new EventListener<Action<ObservableItemNode, ChainedPropertyChangedEventArgs<object>>>(
			h => observer.TrackingPropertyChanged += h,
			h => observer.TrackingPropertyChanged -= h,
			(s, e) => Console.WriteLine($"Handle All  NodeName:{s.Name}, Property:{e.ChainedName}, Value:{e.PropertyValue.ToString()}"));

		var listener00 = observer.TrackHandler(x => x.Parent.Parent, (s, e) => Console.WriteLine($"Handle GrandParent  NodeName:{s.Name}, Property:{e.ChainedName}, Value:{e.PropertyValue.ToString()}"));

		var obsListTree = obsList.AssembleAsNAryTree(2);
		Console.WriteLine(obsListTree.ToTreeDiagram(x=> x.Name));
		/* 
		Handle All  NodeName:D, Property:Parent.Parent, Value:A
		Handle GrandParent  NodeName:D, Property:Parent.Parent, Value:A
		A
		├ B
		│ └ D
		└ C
		*/

		var trkList1 = observer.CreateTrackingList();
		trkList1.Register(x => x.AdditionalInfo);
		trkList1.Register(x => x.AdditionalInfo);
		var listener1 = trkList1.AttachHandler((s, e) => Console.WriteLine($"Handle list1  NodeName:{s.Name}, Property:{e.ChainedName}, Value:{e.PropertyValue.ToString()}"));

		var trkList2 = observer.CreateTrackingList();
		trkList2.Register(x => x.AdditionalInfo.Title, x => x.AdditionalInfo.Number);
		var listener2 = trkList2.AttachHandler((s, e) => Console.WriteLine($"Handle list2  NodeName:{s.Name}, Property:{e.ChainedName}, Value:{e.PropertyValue.ToString()}"));

		obsList[3].AdditionalInfo = new AdditionalObj();
		/*
		Handle All  NodeName:D, Property:AdditionalInfo, Value:Title:, Number:0
		Handle list1  NodeName:D, Property:AdditionalInfo, Value:Title:, Number:0
		Handle All  NodeName:D, Property:AdditionalInfo.Title, Value:
		Handle list2  NodeName:D, Property:AdditionalInfo.Title, Value:
		Handle All  NodeName:D, Property:AdditionalInfo.Number, Value:0
		Handle list2  NodeName:D, Property:AdditionalInfo.Number, Value:0
		 * */

		obsList[3].AdditionalInfo.Title = "Other";
		/*
		Handle All  NodeName:D, Property:AdditionalInfo.Title, Value:Other
		Handle list2  NodeName:D, Property:AdditionalInfo.Title, Value:Other
		 * */

		var node = new ObservableItemNode() { Name = "E" };
		obsList.Add(node);
		obsListTree.LevelOrder().Last().AddChild(node);
		Console.WriteLine(obsListTree.ToTreeDiagram(x=> x.Name));
		/*
		Handle All  NodeName:E, Property:Parent.Parent, Value:B
		Handle GrandParent  NodeName:E, Property:Parent.Parent, Value:B
		A
		├ B
		│ └ D
		│   └ E
		└ C
		 * */

		trkList2.Dispose();
		obsList[3].AdditionalInfo.Number = 5;
		// (no handler)

		//after use
		observer.Dispose();

	}
}