using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Collections {
	/// <summary>
	/// Represents a class for combining collections.
	/// </summary>
	/// <remarks>
	/// To synchronize changes with each collection, it must implement <see cref="INotifyCollectionChanged"/>.
	/// If such implementation is not present, changes will be reflected during collection additions or removals.
	/// </remarks>
	/// <typeparam name="T">The type of elements.</typeparam>
	public class CombinableObservableCollection<T> : ReadOnlyObservableCollection<T>{
		/// <summary>Initialize a new instance.</summary>
		/// <param name="comparer"></param>
		public CombinableObservableCollection(IEqualityComparer<T>? comparer = null) : base(new ObservableCollection<T>()) {
			ListAligner = new ListAligner<T, ObservableCollection<T>>(_Items, move: (list, ord, to) => { list.Move(ord, to); });
			_combines = new List<IEnumerable<T>>();
			this.comparer = comparer ??= EqualityComparer<T>.Default;
		}
		/// <summary></summary>
		protected virtual ListAligner<T,ObservableCollection<T>> ListAligner{ get; }
		private List<IEnumerable<T>> _combines;
		readonly IEqualityComparer<T> comparer;
		private ObservableCollection<T> _Items => (this.Items as ObservableCollection<T>)!;
		/// <summary>Adds the collection to the end.</summary>
		public void AppendCollection(IEnumerable<T> collection){
			_combines.Add(collection);
			combine(collection);
		}
		/// <summary>
		/// Adds the collection at the specified position.
		/// </summary>
		/// <param name="idx">The index.</param>
		/// <param name="collection">The collection to add.</param>
		public void InsertCollection(int idx,IEnumerable<T> collection){
			_combines.Insert(idx,collection);
			combine(collection);
		}
		void combine(IEnumerable<T> collection){
			if(collection is INotifyCollectionChanged notify){
				notify.CollectionChanged += collectionChanged;
			}
			ListAligner.AlignBy(_combines.SelectMany(x => x), comparer);
		}
		/// <summary>Removes the collection.</summary>
		/// <param name="collection"></param>
		public void RemoveCollection(IEnumerable<T> collection){
			_combines.Remove(collection);
			if(collection is INotifyCollectionChanged notify){
				notify.CollectionChanged -= collectionChanged;
			}
			ListAligner.AlignBy(_combines.SelectMany(x => x), comparer);
		}
		/// <summary>Clears all combined collections.</summary>
		public void ClearCollection(){
			foreach(var collection in _combines.AsEnumerable().Reverse()){
				_combines.Remove(collection);
				if(collection is INotifyCollectionChanged notify){
					notify.CollectionChanged -= collectionChanged;
				}
			}
			ListAligner.AlignBy(_combines.SelectMany(x => x), comparer);
		}

		private void collectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
			ListAligner.AlignBy(_combines.SelectMany(x => x),comparer);
		}
		/// <summary>
		/// Occurs when the collection changes, either by adding or removing an item.
		/// </summary>
		public new event NotifyCollectionChangedEventHandler CollectionChanged {
			add{ base.CollectionChanged += value; }
			remove{ base.CollectionChanged -= value; }
		}
		ReadOnlyObservableCollection<T>? _readonly;
		/// <summary></summary>
		/// <returns></returns>
		public ReadOnlyObservableCollection<T> AsReadOnlyObservableCollection()
			=> _readonly ??= new ReadOnlyObservableCollection<T>(_Items);
	}
}
