﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;

namespace SampleConsoleApp; 

public class ClassA {
	public ClassA() {
		PropA = "classA";
		Console.WriteLine(PropA ?? "null in ClassA ctor");
		PropB = PropA + "add";
		Console.WriteLine($"Method return : {Method()}");
		this.EventHandler += (s, e) => this.Raised();
	}
	public virtual string PropA { get; }
	public string PropB { get; }
	public virtual string Method() => "A";
	public event EventHandler EventHandler;
	public virtual void Raised() { Console.WriteLine("event rise of A"); }
	public void OnRise() { this.EventHandler?.Invoke(this, EventArgs.Empty); }
}
public class ClassB : ClassA {
	public ClassB() : base() {
		PropA = "ClassB";
		Console.WriteLine(PropA ?? "null in classB ctor");
	}
	public override string PropA { get; }
	public override string Method() => "B";
	public override void Raised() { Console.WriteLine("event rise of B"); }
}
public class ClassC : ClassB {
	public ClassC() : base() {
		PropA = "ClassC";
		Console.WriteLine(PropA ?? "null in classC ctor");
	}
	public override string PropA { get; }
}
public class BaseClass {
	public BaseClass(string propA) {
		_propA = propA;
		Console.WriteLine(this.PropValue ?? "null in baseClass");
	}
	string _propA;
	public virtual string PropValue => _propA;
}
public class SubClass : BaseClass {
	public SubClass(string propB) : base("dummy") {
		_propB = propB;
		Console.WriteLine(this.PropValue ?? "null in subClass");
	}
	string _propB;
	public override string PropValue => _propB;
}
public static partial class UseageSample {
	public static void MethodN() {
		var tt = new SubClass("subclass");
		var test = new ClassC();
		test.OnRise();

		var obs1 = new ObservableCollection<string>();
		var obs2 = new ObservableCollection<string>("abc".Select(x=>x.ToString()));
		var lst3 = new List<string>("123".Select(x=>x.ToString()));
		var obs4 = new ObservableCollection<string>("qwerty".Select(x => x.ToString()));

		var cmb = new ObservableCombinableCollection<string>();
		(cmb as INotifyCollectionChanged).CollectionChanged += (s, e) => {
			Console.WriteLine($"action:{e.Action}, StartIndex:{e.NewStartingIndex}, NewItems:{string.Join(" ",e.NewItems?.OfType<string>() ?? Array.Empty<string>())}, OldIndex:{e.OldStartingIndex}, OldItems:{string.Join(" ",e.OldItems?.OfType<string>() ?? Array.Empty<string>())}");
		};
		cmb.AppendCollection(obs1);
		cmb.AppendCollection(obs2);
		cmb.AppendCollection(lst3);
		cmb.AppendCollection(obs4);
		Console.WriteLine(string.Join(", ", cmb));

		obs1.Add("A");
		Console.WriteLine(string.Join(", ", cmb));
		obs2.Remove("b");
		Console.WriteLine(string.Join(", ", cmb));
		lst3.Add("100");
		Console.WriteLine(string.Join(", ", cmb));
		obs4[1] = "D";
		Console.WriteLine(string.Join(", ", cmb));
		obs4.Move(4, 0);
		Console.WriteLine(string.Join(", ", cmb));

		cmb.RemoveCollection(obs4);
		Console.WriteLine(string.Join(", ", cmb));
		cmb.InsertCollection(1, obs4);
		Console.WriteLine(string.Join(", ", cmb));

		cmb.RemoveCollection(lst3);
		Console.WriteLine(string.Join(", ", cmb));
		cmb.InsertCollection(0, lst3);
		Console.WriteLine(string.Join(", ", cmb));

		cmb.Dispose();
		Console.WriteLine(string.Join(", ", cmb));

	}
}
