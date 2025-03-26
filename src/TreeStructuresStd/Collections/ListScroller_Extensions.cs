using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using TreeStructures.Results;
using TreeStructures.Utilities;

namespace TreeStructures.Collections {
	public static class ListScrollerExtensions {
		static IEnumerable<T> nextSequence<T, TList>(this IListScroller<T, TList> list) where TList : IListScroller<T, TList> {
			return list.Items.Skip(list.CurrentIndex + 1);
		}
		static IEnumerable<T> previousSequence<T, TList>(this IListScroller<T, TList> list) where TList : IListScroller<T, TList> {
			return list.Items.Take(list.CurrentIndex).Reverse();
		}
		static TList asTList<T, TList>(this IListScroller<T, TList> list) where TList: IListScroller<T,TList>
			=> list.MoveTo(list.CurrentIndex);
		static int Tail<T, TList>(this IListScroller<T, TList> list) where TList : IListScroller<T, TList> {
			if (list.Items.Any()) return list.Items.Count - 1;
			return 0;
		}

		public static TList RestoreAfter<T,TList>(this IListScroller<T,TList> list,Action<TList> action) where TList: IListScroller<T, TList> {
			var b = list.CurrentIndex;
			action(list.asTList());
			return list.MoveTo(b);
		}
		#region Move
		public static TList Move<T,TList>(this IListScroller<T,TList> list, int moveCount)where TList:IListScroller<T,TList> {
			return list.MoveTo(list.CurrentIndex + moveCount);
		}
		public static bool CanMove<T,TList>(this IListScroller<T,TList> list,int moveCount)where TList : IListScroller<T, TList> {
			return list.CanMoveTo(list.CurrentIndex + moveCount);
		}
		public static ResultWithValue<TList> TryMove<T, TList>(this IListScroller<T, TList> list, int moveCount) where TList : IListScroller<T, TList>
			=> list.CanMove(moveCount)
				? new ResultWithValue<TList>(list.Move(moveCount))
				: new ResultWithValue<TList>(false, list.asTList());
		public static ResultWithValue<TList> TryMoveTo<T, TList>(this IListScroller<T, TList> list, int index) where TList : IListScroller<T, TList>
			=> list.CanMoveTo(index)
				? new ResultWithValue<TList>(list.MoveTo(index))
				: new ResultWithValue<TList>(false, list.asTList());
		#endregion

		#region Has (Next|Previous)
		public static bool HasNext<T, TList>(this IListScroller<T, TList> list, int count = 1) where TList : IListScroller<T, TList>
			=> list.CanMove(count);
		public static bool HasNext<T, TList>(this IListScroller<T, TList> list, Predicate<T> predicate) where TList : IListScroller<T, TList>
			=> list.nextSequence().Any(x => predicate(x));

		public static bool HasPrevious<T, TList>(this IListScroller<T, TList> list, int count = 1) where TList : IListScroller<T, TList>
			=> list.CanMove(count * -1);
		public static bool HasPrevious<T, TList>(this IListScroller<T, TList> list, Predicate<T> predicate) where TList : IListScroller<T, TList>
			=> list.previousSequence().Any(x => predicate(x));
		#endregion

		#region Next
		public static TList Next<T, TList>(this IListScroller<T, TList> list, int count = 1) where TList : IListScroller<T, TList>
			=> list.Move(count);
		public static TList Next<T, TList>(this IListScroller<T, TList> list,Predicate<T> predicate) where TList : IListScroller<T, TList> {
			var cnt = list.nextSequence()
				.Select((v, i) => new { Value = v, Index = i })
				.First(x => predicate(x.Value))
				.Index + 1;
			return list.Move(cnt);
		}
		public static ResultWithValue<TList> TryNext<T, TList>(this IListScroller<T, TList> list, int count = 1) where TList : IListScroller<T, TList>
			=>list.CanMove(count) 
				? new ResultWithValue<TList> (list.Move(count)) 
				: new ResultWithValue<TList>(false,list.asTList());
		public static ResultWithValue<TList> TryNext<T, TList>(this IListScroller<T, TList> list, Predicate<T> predicate) where TList : IListScroller<T, TList>
			=> list.HasNext(predicate)
				? new ResultWithValue<TList>(list.Next(predicate))
				: new ResultWithValue<TList>(false, list.asTList());
		#endregion

		#region Preivous
		public static TList Previous<T, TList>(this IListScroller<T, TList> list, int count = 1) where TList : IListScroller<T, TList>
			=> list.Move(count * -1);
		public static TList Previous<T, TList>(this IListScroller<T, TList> list, Predicate<T> predicate) where TList : IListScroller<T, TList> {
			var cnt = list.previousSequence()
				.Select((v, i) => new { Value = v, Index = i })
				.First(x => predicate(x.Value))
				.Index + 1;
			return list.Move(cnt * -1);
		}
		public static ResultWithValue<TList> TryPrevious<T, TList>(this IListScroller<T, TList> list, int count = 1) where TList : IListScroller<T, TList> {
			count *= -1;
			return list.CanMove(count) 
				? new ResultWithValue<TList>(list.Move(count))
				: new ResultWithValue<TList>(false, list.asTList());
		}
		public static ResultWithValue<TList> TryPrevious<T, TList>(this IListScroller<T, TList> list, Predicate<T> predicate) where TList : IListScroller<T, TList>
			=> list.HasPrevious(predicate)
				? new ResultWithValue<TList>(list.Previous(predicate))
				: new ResultWithValue<TList>(false, list.asTList());

		#endregion

		#region Is(First|Last)
		public static bool IsFirst<T, TList>(this IListScroller<T, TList> list) where TList : IListScroller<T, TList>
			=> list.CurrentIndex == 0;
		public static bool IsFirst<T,TList>(this IListScroller<T,TList> list, Predicate<T> predicate)where TList : IListScroller<T, TList> {
			bool result = false;
			list.RestoreAfter(self => {
				int idx = self.CurrentIndex;
				Action<int> check = (cruIdx) => { result = idx == cruIdx; };
				self.Reset();
				if (predicate(self.Current)) {
					check(self.CurrentIndex);
				} else {
					self.TryNext(predicate).When(o => check(o.CurrentIndex));
				}
			});
			return result;
		}
		public static bool IsLast<T, TList>(this IListScroller<T, TList> list) where TList : IListScroller<T, TList> {
			if (list.CurrentIndex < 0 || list.HasNext()) return false;
			else return true;
		}
		public static bool IsLast<T,TList>(this IListScroller<T,TList> list,Predicate<T> predicate) where TList : IListScroller<T, TList> {
			bool result = false;
			list.RestoreAfter(self => {
				int idx = self.CurrentIndex;
				Action<int> check = (cruIdx) => { result = idx == cruIdx; };
				self.MoveTo(self.Tail());
				if (predicate(self.Current)) {
					check(self.CurrentIndex);
				} else {
					self.TryPrevious(predicate).When(o=>check(o.CurrentIndex));
				}
			});
			return result;
		}
		#endregion is

		#region First | Last
		public static TList First<T, TList>(this IListScroller<T, TList> list) where TList : IListScroller<T, TList>
			=> list.MoveTo(0);
		public static TList First<T, TList>(this IListScroller<T, TList> list,Predicate<T> predicate) where TList : IListScroller<T, TList> {
			list.Reset();
			return list.While(lst => !predicate(lst.Current), lst => lst.Next());
		}
		public static ResultWithValue<TList> TryFirst<T,TList>(this IListScroller<T,TList> list,Predicate<T>? predicate = null)where TList : IListScroller<T, TList> {
			predicate ??= x => true;
			int i = list.CurrentIndex;
			if (i < 0) return new ResultWithValue<TList>(false, list.asTList());
			
			list.Reset();
			if (predicate(list.Current)) 
				return new ResultWithValue<TList>(list.asTList());
			var result = list.TryNext(predicate);
			return result ? result : new ResultWithValue<TList>(false, list.MoveTo(i));
		}

		public static TList Last<T,TList>(this IListScroller<T,TList> list) where TList : IListScroller<T, TList> {
			if (list.CurrentIndex < 0) throw new InvalidOperationException();
			return list.MoveTo(list.Tail());
		}
		public static TList Last<T, TList>(this IListScroller<T, TList> list, Predicate<T> predicate) where TList : IListScroller<T, TList> {
			list.MoveTo(list.Tail());
			return list.While(lst => !predicate(lst.Current), lst => lst.Previous());
		}
		public static ResultWithValue<TList> TryLast<T,TList>(this IListScroller<T,TList> list, Predicate<T>? predicate = null) where TList : IListScroller<T, TList> {
			predicate ??= x => true;
			int i = list.CurrentIndex;
			if (i < 0) return new ResultWithValue<TList>(false, list.asTList());

			list.MoveTo(list.Tail());
			if(predicate(list.Current)) 
				return new ResultWithValue<TList>(true, list.asTList());
			var result = list.TryPrevious(predicate);
			return result ? result : new ResultWithValue<TList>(false, list.MoveTo(i));
		}
		#endregion

		#region loop
		/// <summary>
		/// Iterates through all elements by moving the scroller and applies the specified action to each step.
		/// </summary>
		/// <param name="list">The scroller to iterate over.</param>
		/// <param name="action">The action to apply at each step.</param>
		/// <typeparam name="T">The type of elements in the scroller.</typeparam>
		/// <typeparam name="TList">The scroller type.</typeparam>
		public static TList MoveForEach<T,TList>(this IListScroller<T,TList> list,Action<T> action) where TList: IListScroller<T, TList> {
			list.Reset();
			if (list.CurrentIndex < 0) return list.asTList();
			return list.DoWhile(
				lst => { 
					return lst.TryNext();
				}, 
				lst => {
					lst.RestoreAfter(x => action(x.Current));
				});
		}
		public static TList MoveForEachReverse<T, TList>(this IListScroller<T, TList> list, Action<T> action) where TList : IListScroller<T, TList> {
			list.Reset();
			if (list.CurrentIndex < 0) return list.asTList();
			list.MoveTo(list.Tail());
			return list.DoWhile(lst => lst.TryPrevious(), lst=>lst.RestoreAfter(x=>action(x.Current)));
		}
		public static TList Repeat<T,TList>(this IListScroller<T,TList> list, int count,Action<int,TList> action) where TList : IListScroller<T, TList> {
			for (int i = 1; i <= count; i++) { action(i, list.asTList()); }
			return list.asTList();
		}
		public static TList DoWhile<T, TList>(this IListScroller<T, TList> list, Predicate<TList> toContinue, Action<TList> action) where TList : IListScroller<T, TList> {
			do {
				action(list.asTList());
			} 
			while (toContinue(list.asTList()));
			return list.asTList();
		}
		public static TList While<T, TList>(this IListScroller<T, TList> list, Predicate<TList> toContinue, Action<TList> action) where TList : IListScroller<T, TList> {
			while (toContinue(list.asTList())) {
				action(list.asTList());
			}
			return list.asTList();
		}
		#endregion loop

	}
}
