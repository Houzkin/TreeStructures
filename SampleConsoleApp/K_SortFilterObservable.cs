using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;
using TreeStructures.Events;
using System.Runtime.CompilerServices;
using TreeStructures.Collections;
using System.Reactive.Linq;

namespace SampleConsoleApp;
public class NotificationObj1 : INotifyPropertyChanged {
	protected readonly PropChangeProxy PcProxy;
	public NotificationObj1() {
		PcProxy = new PropChangeProxy(this, () => PropertyChanged);
	}
	public event PropertyChangedEventHandler? PropertyChanged;
}
public class NotificationObj2 : INotifyPropertyChanged {
	protected readonly PropChangeProxy PcProxy;
	public NotificationObj2() {
		PcProxy = new PropChangeProxy(RaisePropertyChanged);
	}
	protected virtual void RaisePropertyChanged(PropertyChangedEventArgs e) {
		PropertyChanged?.Invoke(this, e);
	}
	public event PropertyChangedEventHandler? PropertyChanged;
}
public class NotificationObje3 : INotifyPropertyChanged {
	private PropChangeProxy _proxy;
	public NotificationObje3() {
		_proxy = new PropChangeProxy(OnPropertyChanged);
	}
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		if (string.IsNullOrEmpty(propertyName)) return;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	protected bool SetProperty<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) {
		return _proxy.SetWithNotify(ref strage, value);
	}
	public event PropertyChangedEventHandler? PropertyChanged;
}

public abstract class NotificationObject : INotifyPropertyChanged {
	protected NotificationObject() {
		PropChangeProxy = new PropertyChangeProxy(this);
	}
	readonly PropertyChangeProxy PropChangeProxy;

	public event PropertyChangedEventHandler? PropertyChanged {
		add { this.PropChangeProxy.Changed += value; }
		remove { this.PropChangeProxy.Changed -= value; }
	}
	protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
		=> PropChangeProxy.SetWithNotify(ref storage, value, propertyName);

	protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropChangeProxy.Notify(propertyName);
}
public class HeadOfState : NotificationObject {
	string name;
	public string Name {
		get { return name; }
		set { this.SetProperty(ref name, value); }
	}
	string _state;
	public string State {
		get { return _state; }
		set { this.SetProperty(ref _state, value); }
	}
	DateTime _date;
	public DateTime Birthday{
		get{ return _date; }
		set{ this.SetProperty(ref _date, value);}
	}
	bool _result;
	public bool IsRepublic {
		get { return _result; }
		set { this.SetProperty(ref _result, value); }
	}
	bool _isEmpress;
	public bool IsEmpress {
		get { return _isEmpress; }
		set { this.SetProperty(ref _isEmpress, value); }
	}
}
public static partial class UseageSample{
	public static void MethodK() {
		var rmd = new RandomDateTime(new DateTime(2023, 1, 1), new DateTime(2024, 12, 31));
		var dtlist = new ObservableCollection<DateTime>() { rmd.Next(), rmd.Next(), rmd.Next(), };
		//var dtlist = new ObservableCollection<DateTime>() { new DateTime(2024, 9, 28), new DateTime(2024, 10, 6), new DateTime(2023, 5, 1) };//infinite loop

		//dtlist.AlignBy(dtlist.OrderBy(a => a));
		//Console.WriteLine(string.Join(", ", dtlist.Select(x => x.ToString("yyyy/MM/dd"))));
		//dtlist.Insert(3, rmd.Next());



		var sfo = dtlist.ToReadOnlyObservableFilterSort();
		//var sfo = new RooObsFilterSortCollection<DateTime>(dtlist);

		Console.WriteLine("initial state");
		Console.WriteLine(string.Join(", ", sfo.Select(x => x.ToString("yyyy/MM/dd"))));

		Console.WriteLine("sort");
		sfo.SortBy(x => x);
		Console.WriteLine(string.Join(", ", sfo.Select(x => x.ToString("yyyy/MM/dd"))));

		Console.WriteLine("add elements");
		dtlist.Add(rmd.Next());
		dtlist.Add(rmd.Next());
		dtlist.Add(rmd.Next());
		Console.WriteLine(string.Join(", ", sfo.Select(x => x.ToString("yyyy/MM/dd"))));



		var heads = new ObservableCollection<HeadOfState>() {
			new HeadOfState{ Name = "Naruhito",State ="Japan", Birthday = new DateTime(1960,2,23), IsRepublic = false,IsEmpress = false },
			new HeadOfState{Name = "Elizabeth2nd",State="UK",Birthday = new DateTime(1926,4,21),IsRepublic = false,IsEmpress = true },
			new HeadOfState{Name = "Charlse3rd",State="UK",Birthday=new DateTime(1948,11,14), IsRepublic=false,IsEmpress=false },
			new HeadOfState{Name = "BidenJr",State = "USA",Birthday = new DateTime(1942,11,20),IsRepublic=true,IsEmpress = false },
			new HeadOfState{Name = "Putin",State="Russia",Birthday = new DateTime(1952,10,7),IsRepublic = true,IsEmpress=false },
			new HeadOfState{Name ="Raisi",State="Iran",Birthday = new DateTime(1960,12,14),IsRepublic = true,IsEmpress=false },
			new HeadOfState{Name = "Margrethe2nd",State="Danmark",Birthday = new DateTime(1940,4,16),IsRepublic =false,IsEmpress = true },
			new HeadOfState{Name = "Erdogan",State = "Turkey",Birthday =new DateTime(1954,2,26),IsRepublic=true,IsEmpress=false },
			new HeadOfState{Name = "Charlse3rd",State="Australia",Birthday=new DateTime(1948,11,14),IsRepublic=false, IsEmpress=false },
			new HeadOfState{Name = "Charlse3rd",State="Canada",Birthday=new DateTime(1948,11,14), IsRepublic=false,IsEmpress=false },
		};

		Console.WriteLine("\ninitial state");
		var sfoheads = heads.ToReadOnlyObservableFilterSort();
		Console.WriteLine(string.Join("\n", sfoheads.Select(x => $"State:{x.State}, Name:{x.Name}, Birthday:{x.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{x.IsRepublic}, IsEmpress:{x.IsEmpress}")));

		Console.WriteLine("\nonly republic");
		sfoheads.FilterProperty(x => x.IsRepublic);
		Console.WriteLine(string.Join("\n", sfoheads.Select(x => $"State:{x.State}, Name:{x.Name}, Birthday:{x.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{x.IsRepublic}, IsEmpress:{x.IsEmpress}")));


		Console.WriteLine("\nsort birthday");
		sfoheads.SortProperty(x => x.Birthday);
		Console.WriteLine(string.Join("\n", sfoheads.Select(x => $"State:{x.State}, Name:{x.Name}, Birthday:{x.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{x.IsRepublic}, IsEmpress:{x.IsEmpress}")));

		Console.WriteLine("\nsort state");
		sfoheads.SortBy(x => x.State.Length,/**Comparer<int>.Default.Invert(),**/ x => x.State);
		Console.WriteLine(string.Join("\n", sfoheads.Select(x => $"State:{x.State}, Name:{x.Name}, Birthday:{x.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{x.IsRepublic}, IsEmpress:{x.IsEmpress}")));

		Console.WriteLine("\nClear filter");
		sfoheads.ClearFilter();
		Console.WriteLine(string.Join("\n", sfoheads.Select(x => $"State:{x.State}, Name:{x.Name}, Birthday:{x.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{x.IsRepublic}, IsEmpress:{x.IsEmpress}")));

		Console.WriteLine("\nEdit state: UK -> UnitedKingdom");
		foreach(var item in heads.Where(x=>x.State=="UK")){
			item.State = "UnitedKingdom";
		}
		Console.WriteLine(string.Join("\n", sfoheads.Select(x => $"State:{x.State}, Name:{x.Name}, Birthday:{x.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{x.IsRepublic}, IsEmpress:{x.IsEmpress}")));

		Console.WriteLine("changed hander");
		//var disp = sfoheads.Subscribe(x => x.Name, (s, e) => {
		//	Console.WriteLine($"State:{s.State}, Name:{s.Name}, Birthday:{s.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{s.IsRepublic}, IsEmpress:{s.IsEmpress}");
		//});
		foreach (var item in heads.Where(x => x.State == "USA")) { item.Name = "Trump"; }
		Console.WriteLine(string.Join("\n", sfoheads.Select(x => $"State:{x.State}, Name:{x.Name}, Birthday:{x.Birthday.ToString("yyyy/MM/dd")}, IsRepublic:{x.IsRepublic}, IsEmpress:{x.IsEmpress}")));
	}

}