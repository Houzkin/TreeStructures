using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Collections;
using TreeStructures.Events;
using TreeStructures.Linq;
using TreeStructures.Tree;

namespace SampleConsoleApp;

public class CategoryView : BindableTreeNodeWrapper<CategoryTree<DateTime, int>.Node, CategoryView>, IComparable<CategoryView> {
	public CategoryView(CategoryTree<DateTime, int>.Node sourceNode) : base(sourceNode) { }

	protected override CategoryView GenerateChild(CategoryTree<DateTime, int>.Node sourceChildNode) {
		return new CategoryView(sourceChildNode);
	}
	protected override IEnumerable<CategoryView> SetupPublicChildCollection(CombinableMirroredChildrenCollection<CategoryView> children) {
		var lst = children.ToObservableFilterSortMirror();
		lst.SortBy(x => x);
		return lst;
	}

	public int CompareTo(CategoryView? other) {
		if(HasItem) {
			return this.Item.CompareTo(other?.Item);
		} else {
			return this.Category.CompareTo(other?.Category);
		}
	}

	public bool HasItem => Source.HasItem;
	public int Category => Source.Category;
	public DateTime Item => Source.Item;
}

public class Player : INotifyPropertyChanged {
	public Player() {
		this.Properties = new PropertyAccessor(this, () => this.PropertyChanged);
	}
	protected PropertyAccessor Properties { get; }

	public event PropertyChangedEventHandler? PropertyChanged;

	public string Name { get; set; }
	public int Rank {
		get => Properties.GetValue(0);
		set => Properties.TrySetValue(value);
	}
	public TimeSpan ActivePeriod {
		get => Properties.GetValue<TimeSpan>();
		set => Properties.TrySetValue(value);
	}
	public override string ToString() {
		return $"Name:{Name}, Rank:{Rank}, Period:{ActivePeriod.Days}";
	}
}
public class PlayerView : BindableTreeNodeWrapper<CategoryTree<Player, int>.Node, PlayerView> {
	Comparer<PlayerView> comparer;
	public PlayerView(CategoryTree<Player, int>.Node sourceNode) : base(sourceNode) { 
		comparer = Comparer<PlayerView>.Create((a, b) => {
			if (a.HasItem && b.HasItem) 
				return a.Item.ActivePeriod.Days - b.Item.ActivePeriod.Days;
			else 
				return a.Category - b.Category;
		});
	}
	protected override PlayerView GenerateChild(CategoryTree<Player, int>.Node sourceChildNode) {
		return new PlayerView(sourceChildNode);
	}
	protected override IEnumerable<PlayerView> SetupPublicChildCollection(CombinableMirroredChildrenCollection<PlayerView> children) {
		var lst = children.ToObservableFilterSortMirror();
		if (this.Source.Depth() == 0) {
			lst.SortBy(x => x, comparer.Invert());
		} else {
			lst.SortBy(x => x, comparer);
		}
		return lst;
	}
	public override string ToString() {
		if(this.Source.HasItem) return this.Source.Item.ToString();
		var header = this.Source.Depth() switch {
			0 => "Ranker",
			1 => toRankLabel(this.Source.Category) + "-Rank",
			2 => $"{this.Source.Category} days",
			_ => "",
		};
		return header;
	}
	string toRankLabel(int rank) {
		string str = rank switch {
			1 => "E",
			2 => "D",
			3 => "C",
			4 => "B",
			5 => "A",
			_ => "_",
		};
		return str;
	}


	public bool HasItem => Source.HasItem;
	public int Category => Source.Category;
	public Player Item => Source.Item;
}

public class Onion : NotifyObjA {
	public Onion Inside {
		get => Properties.GetValue<Onion>();
		set => Properties.TrySetValue(value);
	}
	public KnownColor SurfaceColor {
		get => Properties.GetValue(KnownColor.White);
		set => Properties.TrySetValue(value);
	}
}
public static partial class Demo {
	public static void Category_1() {

		var lst = new List<DateTime>() {
			new(2026,8,23),
			new(2026,8,15),
			new(2026,2,5),
			new(2026,2,5),
		};

		var dateTree = new CategoryTree<DateTime, int>(x => x.Year, x => x.Month, x => x.Day);
		dateTree.Add(lst);

		Console.WriteLine(dateTree.Root.ToTreeDiagram(x => x.HasItem ? x.Item.ToString("yyyy/M/d") : x.Category.ToString()));

		dateTree.Remove(lst[1]);
		Console.WriteLine(dateTree.Root.ToTreeDiagram(x => x.HasItem ? x.Item.ToString("yyyy/M/d") : x.Category.ToString()));

		dateTree.Add(new DateTime(2027,1,17));
		Console.WriteLine(dateTree.Root.ToTreeDiagram(x => x.HasItem ? x.Item.ToString("yyyy/M/d") : x.Category.ToString()));
	}
	public static void Category_2() {

		var lst = new List<DateTime>() {
			new(2026,8,23),
			new(2026,8,15),
			new(2026,2,5),
			new(2026,2,5),
		};

		var dateTree = new CategoryTree<DateTime, int>(x => x.Year, x => x.Month, x => x.Day);
		var dateTreeView = new CategoryView(dateTree.Root);

		dateTree.Add(lst);
		Console.WriteLine(dateTreeView.ToTreeDiagram(x => x.HasItem ? x.Item.ToString("yyyy/M/d") : x.Category.ToString()));

		dateTree.Remove(lst[1]);
		Console.WriteLine(dateTreeView.ToTreeDiagram(x => x.HasItem ? x.Item.ToString("yyyy/M/d") : x.Category.ToString()));

		dateTree.Add(new DateTime(2027,1,17));
		Console.WriteLine(dateTreeView.ToTreeDiagram(x => x.HasItem ? x.Item.ToString("yyyy/M/d") : x.Category.ToString()));
	}

	public static void Category_3() {

		var lst = new List<Player>() {
			new(){Name = "Ada", Rank = 2, ActivePeriod = new(32,0,0,0)},
			new(){Name = "Bill", Rank = 5, ActivePeriod = new(75,0,0,0)},
			new(){Name = "Curt", Rank = 2, ActivePeriod = new(41,0,0,0)},
			new(){Name = "Dan", Rank = 4, ActivePeriod = new(87,0,0,0)},
			new(){Name = "Eden", Rank = 1, ActivePeriod = new(13,0,0,0)},
			new(){Name = "Fleur", Rank = 2, ActivePeriod = new(14,0,0,0)},
		}.ToDictionary(x => x.Name, x => x);

		var ctgTree = new ReactiveCategoryTree<Player, int>(new(x => x.Rank, x => x.ActivePeriod), x =>x.Rank , x => x.ActivePeriod.Days);
		ctgTree.Add(lst.Values);

		var ctgTreeView = new PlayerView(ctgTree.Root);

		Console.WriteLine(ctgTreeView.ToTreeDiagram(x => x.ToString()));

		lst["Curt"].Rank = 3;
		lst["Fleur"].ActivePeriod = new(32, 0, 0, 0);

		Console.WriteLine(ctgTreeView.ToTreeDiagram(x => x.ToString()));


	}
	public static void ObsPropTree_1() {

		var colors = new KnownColor[] { KnownColor.Black, KnownColor.Brown, KnownColor.DarkKhaki, KnownColor.White };

		var root = colors.AssembleAsNAryTree(1,
			clr => new Onion() { SurfaceColor = clr },
			onion => onion.Inside is null ? Array.Empty<Onion>() : new Onion[] { onion.Inside },
			(p, c) => { p.Inside = c; });

		PrintHierarchy(root, x => new Onion[] { x.Inside }, x => x.SurfaceColor.ToString());

		var observingTree = new ObservedPropertyTree<Onion>(root);

		var listner1 = observingTree.Subscribe(x => x.Inside.Inside.SurfaceColor, clr => Console.WriteLine($"3rd layer color changed: {clr}"));

		root.Inside.Inside.SurfaceColor = KnownColor.Magenta;
		PrintHierarchy(root, x => new Onion[] { x.Inside }, x => x.SurfaceColor.ToString());


		root.Inside = null;

		var redOnion = new KnownColor[] { KnownColor.Red, KnownColor.Green, KnownColor.LightPink }
			.AssembleAsNAryTree(1,
			clr => new Onion() { SurfaceColor = clr },
			onion => onion.Inside is null ? Array.Empty<Onion>() : new Onion[] { onion.Inside },
			(p, c) => { p.Inside = c; });

		root.Inside = redOnion;

		PrintHierarchy(root, x => new Onion[] { x.Inside }, x => x.SurfaceColor.ToString());

		var blueOnion = new KnownColor[] { KnownColor.Blue, KnownColor.Green, KnownColor.Gray }
			.AssembleAsNAryTree(1,
			clr => new Onion() { SurfaceColor = clr },
			onion => onion.Inside is null ? Array.Empty<Onion>() : new Onion[] { onion.Inside },
			(p, c) => { p.Inside = c; });

		root.Inside = blueOnion;

		PrintHierarchy(root, x => new Onion[] { x.Inside }, x => x.SurfaceColor.ToString());

		root.Inside.Inside = redOnion;
		PrintHierarchy(root, x => new Onion[] { x.Inside }, x => x.SurfaceColor.ToString());


		Console.WriteLine(observingTree.Root.ToTreeDiagram(x => x.PropertyName));

		observingTree.Dispose();

		//root.Inside.Inside.SurfaceColor = KnownColor.Magenta;
	}
}
