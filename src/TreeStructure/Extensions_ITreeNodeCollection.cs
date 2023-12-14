using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructure.Utility;

namespace TreeStructure;

/// <summary>
/// ツリー構造を成すオブジェクトに対する拡張メソッドを定義する。
/// </summary>
public static partial class TreeNodeExtenstions {

    #region 編集

    /// <summary>子ノードを追加する。</summary>
    /// <returns>正常に追加できた場合は現在のノード、追加できなかった、或いは既に追加済みの場合は<paramref name="child"/>を返す。</returns>
    public static ResultWithValue<T> TryAddChild<T>(this ITreeNodeCollection<T> self, T child) where T : ITreeNodeCollection<T> {
        if(self.Children.Contains(child)) return new ResultWithValue<T>(false, child);
        self.AddChild(child);
        var ncash = self.Children.ToArray();
        if (ncash.Contains(child)) return new ResultWithValue<T>((T)self);
        return new ResultWithValue<T>(false, child);
    }
    /// <summary>指定したインデックスに子ノードを挿入する。</summary>
    /// <returns>挿入できた場合は現在のノード、挿入できなかった、既に追加済み、追加はできたが指定したインデックスと一致しなかった場合は<paramref name="child"/>を返す。</returns>
    public static ResultWithValue<T> TryInsertChild<T>(this ITreeNodeCollection<T> self, int index,T child) where T : ITreeNodeCollection<T> {
        if (self.Children.Contains(child)) return new ResultWithValue<T>(false, child);
        self.InsertChild(index, child);
        if (self.Children.Contains(child) && child.BranchIndex() == index) return new ResultWithValue<T>((T)self);
        return new ResultWithValue<T>(false, child);
    }

    /// <summary>子ノードを削除する。</summary>
    /// <returns>正常に削除できた場合は<paramref name="child"/>、削除できなかった或いは子ノードに該当しなかった場合は現在のノードを返す。</returns>
    public static ResultWithValue<T> TryRemoveChild<T>(this ITreeNodeCollection<T> self, T child) where T : ITreeNodeCollection<T> {
        if(!self.Children.Contains(child)) return new ResultWithValue<T>(false,(T)self);
        self.RemoveChild(child);
        if (!self.Children.Contains(child)) return new ResultWithValue<T>(child);
        return new ResultWithValue<T>(false, (T)self);
    }
    /// <summary>現在のノードを削除する。</summary>
    /// <returns>正常に削除できた場合はture、削除できなかった或いは現在のノードがルートだった場合はfalse。何れも現在のノードが付与される。</returns>
    public static ResultWithValue<T> TryRemoveOwn<T>(this ITreeNodeCollection<T> self) where T : ITreeNodeCollection<T> {
        if (self == null) throw new ArgumentNullException("self");
        if (self.Parent != null) {
            return self.Parent.TryRemoveChild((T)self).When(
                o => new ResultWithValue<T>(o),
                x => new ResultWithValue<T>(false, (T)self));
        }
        return new ResultWithValue<T>(false, (T)self);
    }
    /// <summary>子孫ノードを分解する。</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns>切り離されたノードを返す</returns>
    public static IReadOnlyList<T> DismantleDescendants<T>(this ITreeNodeCollection<T> self) where T : ITreeNodeCollection<T> {
        List<T> rmvs = new();
        var lst = self.Levelorder().ToArray();
        foreach (var cld in lst) {
            rmvs.AddRange(cld.ClearChildren().OfType<T>());
        }
        return rmvs.AsReadOnly();
    }
    /// <summary>対象から末端にかけて分解する。</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns>切り離されたノードを返す</returns>
    public static IReadOnlyList<T> Dismantle<T>(this ITreeNodeCollection<T> self) where T : ITreeNodeCollection<T> {
        List<T> rmvs=new();
        self.TryRemoveOwn().When(o => rmvs.Add(o));
        rmvs.AddRange(self.DismantleDescendants());
        return rmvs.AsReadOnly();
    }
    /// <summary>条件に一致する子ノードを全て削除する。</summary>
    /// <typeparam name="T">ノードの型</typeparam>
    /// <param name="self">現在のノード</param>
    /// <param name="predicate">削除対象であればtrue</param>
    /// <returns>削除したノード</returns>
    public static IReadOnlyList<T> RemoveChild<T>(this ITreeNodeCollection<T> self, Predicate<T> predicate) where T : ITreeNodeCollection<T> {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        var lst = new List<T>();
        foreach (var cld in self.Children.OfType<T>().ToArray()) {
            if (predicate(cld)) {
                self.TryRemoveChild(cld).When(o => lst.Add(cld));
            }
        }
        return lst;
    }
    /// <summary>対象ノードからレベル順に、条件に一致したノードを全て削除する。</summary>
    /// <typeparam name="T">ノードの型</typeparam>
    /// <param name="self">現在のノード</param>
    /// <param name="predicate">削除対象であればtrue</param>
    /// <returns>削除したノード</returns>
    public static IReadOnlyList<T> RemoveDescendant<T>(this ITreeNodeCollection<T> self, Predicate<T> predicate) where T : ITreeNodeCollection<T> {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        var lst = new List<T>();
        self.Evolve(a => {
            lst.AddRange(a.RemoveChild(predicate).OfType<T>());
            return a.Children;
        },(a, b, c) => new T?[1] { a }.Concat(c).Concat(b));
        return lst;
    }
    
    #endregion
}

