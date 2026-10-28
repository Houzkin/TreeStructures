using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Collections;
using TreeStructures.Events;
using TreeStructures.Linq;
using TreeStructures.Results;
using TreeStructures.Tree;
using TreeStructures.Utilities;

namespace SampleConsoleApp;

public class NotifyLevelChangeNode : ObservableGeneralTreeNode<NotifyLevelChangeNode> {
	UniqueOperationExecutor UniOperator = new UniqueOperationExecutor();
	IDisposable reporter;
	public NotifyLevelChangeNode() {
		//Register action with key
		UniOperator.Register(nameof(Level), () => Properties.Notify(nameof(Level)));
		//Set initial status
		reporter = CreateNewReporter();

		this.StructureChanged += (s, e) => {
			if (e.IsAncestorChanged) {
				//Evaluate whether level changed
				reporter.Dispose();
				//Reset status
				reporter = CreateNewReporter();
			}
		};
	}
	IDisposable CreateNewReporter() => UniOperator.LateEvaluate(nameof(Level), () => Level);
	public int Level => this.Depth();
	public string Name { get; set; }
}
public class NotifyObjA : INotifyPropertyChanged {
	public NotifyObjA() {
		Properties = new PropertyAccessor(this, () => PropertyChanged);
	}
	protected PropertyAccessor Properties { get; }
	public event PropertyChangedEventHandler? PropertyChanged;
}
public class NotifyObjAA : NotifyObjA {
	public string FirstName {
		get => Properties.GetValue<string>();
		set => Properties.TrySetValue(value)
			.When(o => o.Notify(nameof(FullName)));
	}
	public string LastName {
		get => Properties.GetValue<string>();
		set => Properties.TrySetValue(value)
			.When(o => o.Notify(nameof(FullName)));
	}
	public string FullName => FirstName + " " + LastName;
	public int Age {
		get => Properties.GetValue(0);
		set => Properties.TrySetValue(value);
	}
	string _job;
	public string Job {
		get => _job;
		set => Properties.TrySetField(ref _job, value);
	}
}
public class NotifyObjB : INotifyPropertyChanged {
	public NotifyObjB() {
		Properties = new PropertyAccessor(this, RaisePropertyChangedEvent);
	}
	protected PropertyAccessor Properties { get; }
	protected virtual void RaisePropertyChangedEvent(PropertyChangedEventArgs e) {
		PropertyChanged?.Invoke(this, e);
	}
	public virtual event PropertyChangedEventHandler? PropertyChanged;
}
public class NotifyObjBB : NotifyObjB {
	string _firstName = "";
	string _lastName = "";
	public NotifyObjBB(string firstname, string lastname) {
		_firstName = firstname;
		_lastName = lastname;
	}
	public string FirstName {
		get => _firstName;
		set => Properties.TrySetField(ref _firstName, value)
			.When(o => o.Notify(nameof(FullName)));
	}
	public string LastName {
		get => _lastName;
		set => Properties.TrySetField(ref _lastName, value)
			.When(o => o.Notify(nameof(FullName)));
	}
	public string FullName => FirstName + " " + LastName;
	protected override void RaisePropertyChangedEvent(PropertyChangedEventArgs e) {
		Console.WriteLine($"Raise Event: {e.PropertyName}");
		base.RaisePropertyChangedEvent(e);
	}
	public override event PropertyChangedEventHandler? PropertyChanged {
		add {
			base.PropertyChanged += value;
			Console.WriteLine("Handler added.");
		}
		remove {
			base.PropertyChanged -= value;
			Console.WriteLine("Handler removed.");
		}
	}
}
public class DisposableNamedObj : DisposableObject {
	public DisposableNamedObj(Action? action):base(action) { }
	public DisposableNamedObj() { }
	public string Name { get; set; }
	protected override void Dispose(bool disposing) {
		if (disposing) {
			Console.WriteLine($"{this.Name} Dispose");
		}
		base.Dispose(disposing);
	}
	public override string ToString() => this.Name;
}
public class SymbolNode : ObservableGeneralTreeNode<SymbolNode> {
	public SymbolNode() { }
	public char Symbol {
		get => Properties.GetValue<char>();
		set => Properties.TrySetValue(value);
	}
	public ItemData Data {
		get => Properties.GetValue<ItemData>();
		set => Properties.TrySetValue(value);
	}
	public override string ToString() {
		return $"Symbol[{Symbol}]";
	}

}
public static partial class Demo {
	public static void Unique_1() {

		var dic = "ABCDE".ToCharArray().Select(x => x.ToString()).ToDictionary(x => x, x => new NotifyLevelChangeNode() { Name = x });
		dic.Values.AssembleAsNAryTree(2);
		Console.WriteLine(dic["A"].ToTreeDiagram(x => $"{x.Name}, Level:{x.Level}"));

		foreach(var node in dic.Values) {
			node.PropertyChanged += (s, e) => {
				if(e.PropertyName == "Level" && s is NotifyLevelChangeNode nd) {
					Console.WriteLine($"{nd.Name}, Level:{nd.Level}");
				}
			};
		}
		dic["A"].AddChild(dic["D"]);
		dic["C"].AddChild(dic["E"]);

		Console.WriteLine(dic["A"].ToTreeDiagram(x => $"{x.Name}, Level:{x.Level}"));

	}
	public static void Listener_1() {
		var nodeA = new ObservableNamedNode() { Name ="A" };

		var listener1 = new EventListener<PropertyChangedEventHandler>(
			h => nodeA.PropertyChanged += h,
			h => nodeA.PropertyChanged -= h,
			(s, e) => Console.WriteLine($"sender:{s}, Property handler1: {e.PropertyName}"));

		var listener2 = new EventListener<EventHandler<StructureChangedEventArgs<ObservableNamedNode>>>(
			h => nodeA.StructureChanged += h,
			h => nodeA.StructureChanged -= h,
			(s, e) => Console.WriteLine($"sender:{s}, Structure handler: {e.TreeAction}"));

		var listener3 = new EventListener<EventHandler<StructureChangedEventArgs<ObservableNamedNode>>, PropertyChangedEventArgs>(
			h => nodeA.StructureChanged += h,
			h => nodeA.StructureChanged -= h,
			ph => (s, e) => {
				if (e.IsAncestorChanged) ph(s, new PropertyChangedEventArgs("Ancestor"));
				if (e.IsDescendantChanged) ph(s, new PropertyChangedEventArgs("Descendant"));
			},
			propHandler);

		void propHandler(object? sender, PropertyChangedEventArgs e) {
			Console.WriteLine($"sender:{sender}, Structure -> Property handler: {e.PropertyName}");
		}

		nodeA.AddChild(new ObservableNamedNode() { Name = "B" });

		var nodeS = new ObservableNamedNode() { Name = "S" }.AddChild(nodeA);

		nodeS.AddChild(new ObservableNamedNode() { Name = "C"});

		var disposables = new ReusableCompositeDisposable(listener1, listener2, listener3);
		disposables.ClearAndDispose();

		nodeS.Disassemble();

	}

	public static void Notifier_1() {
		var objA = new NotifyObjAA() { FirstName = "Leonardo", LastName = "da Vinci" };
		Console.WriteLine(objA.FullName);

		objA.PropertyChanged += (s, e) => {
			if(e.PropertyName == "FullName")
				Console.WriteLine($"{(s as NotifyObjAA)?.FullName}");
		};
		objA.LastName = "DiCaprio";


		var objB = new NotifyObjBB("Matt", "Reeves");
		objB.PropertyChanged += (s, e) => {
			if (e.PropertyName == "FullName")
				Console.WriteLine($"{(s as NotifyObjBB)?.FullName}");
		};
		objB.FirstName = "Keanu";
	}

	public static void Comparer_1() {
		var lst = new List<string> { "Alvin", "Ben", "Carl", "Daniel", };
		var cmpr = Comparer<string>.Create((x, y) => x.Length - y.Length);

		var pxyLst = lst.ToReadOnlyObservableFilterSort();

		pxyLst.SortBy(x => x, cmpr);
		Console.WriteLine(string.Join (", ", pxyLst));
		////Ben, Carl, Alvin, Daniel

		pxyLst.SortBy(x => x, cmpr.Invert());
		Console.WriteLine(string.Join (", ", pxyLst));
		////Daniel, Alvin, Carl, Ben
	}
	public static void CmbDisp_1() {
		var srcList = new ObservableCollection<char>("ABC".ToCharArray());

		var proxy3 = srcList.ToReadOnlyObservable(
			chr => new DisposableNamedObj() { Name = chr.ToString() },
			(chr, obj) => chr.ToString() == obj.Name,
			obj => obj.Dispose());

		srcList.Remove('A');
		//A Dispose

		proxy3.CombineDisposablesAndSelf().Dispose();
		//B Dispose
		//C Dispose

		srcList.Add('F');
		//
		
	}
}
