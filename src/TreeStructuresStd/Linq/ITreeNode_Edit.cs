using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Utility;

namespace TreeStructures.Linq;

/// <summary>
/// Defines extension methods for objects that form a tree structure.
/// </summary>
public static partial class TreeNodeExtenstions {

    #region 編集

    /// <summary>Adds a child node.</summary>
    /// <returns>If the addition is successful, returns the current node; if the addition is unsuccessful or the node is already added, returns <paramref name="child"/>.</returns>
    public static ResultWithValue<T> TryAddChild<T>(this IMutableTreeNode<T> self, T child) where T : IMutableTreeNode<T> {
        if(self.Children.Contains(child)) return new ResultWithValue<T>(false, child);
        self.AddChild(child);
        var ncash = self.Children.ToArray();
        if (ncash.Contains(child)) return new ResultWithValue<T>((T)self);
        return new ResultWithValue<T>(false, child);
    }
    /// <summary>Inserts a child node at the specified index.</summary>
    /// <returns>If the insertion is successful, returns the current node; if the insertion is unsuccessful, the node is already added, or the specified index does not match, returns <paramref name="child"/>.</returns>
    public static ResultWithValue<T> TryInsertChild<T>(this IMutableTreeNode<T> self, int index,T child) where T : IMutableTreeNode<T> {
        if (self.Children.Contains(child)) return new ResultWithValue<T>(false, child);
        self.InsertChild(index, child);
        if (self.Children.Contains(child) && child.BranchIndex() == index) return new ResultWithValue<T>((T)self);
        return new ResultWithValue<T>(false, child);
    }

    /// <summary>Removes a child node.</summary>
    /// <returns>If the removal is successful, returns <paramref name="child"/>; if the removal is unsuccessful or the specified child node does not exist, returns the current node.</returns>
    public static ResultWithValue<T> TryRemoveChild<T>(this IMutableTreeNode<T> self, T child) where T : IMutableTreeNode<T> {
        if(!self.Children.Contains(child)) return new ResultWithValue<T>(false,(T)self);
        self.RemoveChild(child);
        if (!self.Children.Contains(child)) return new ResultWithValue<T>(child);
        return new ResultWithValue<T>(false, (T)self);
    }
    /// <summary>Removes a child node.</summary>
    /// <param name="child">The child node to remove.</param>
    /// <param name="removeAction">A function that takes the parent node as the first parameter and the child node as the second parameter, dissolving their relationship.</param>
    /// <returns>If the removal is successful, returns <paramref name="child"/>; if the removal is unsuccessful or the specified child node does not exist, returns the current node.</returns>
    public static ResultWithValue<T> TryRemoveChild<T>(this ITreeNode<T> self,T child, Action<T,T> removeAction)where T : ITreeNode<T> {
        if (!self.Children.Contains(child)) return new ResultWithValue<T>(false, (T)self);
        removeAction((T)self, child);
        if (!self.Children.Contains(child)) return new ResultWithValue<T>(child);
        return new ResultWithValue<T>(false, (T)self);
    }
    /// <summary>Removes the current node.</summary>
    /// <returns>If the removal is successful, returns true; if the removal is unsuccessful or the current node is the root, returns false. In either case, the current node is retained.</returns>
    public static ResultWithValue<T> TryRemoveOwn<T>(this IMutableTreeNode<T> self) where T : IMutableTreeNode<T> {
        if (self == null) throw new ArgumentNullException("self");
        if (self.Parent != null) {
            return self.Parent.TryRemoveChild((T)self).When(
                o => new ResultWithValue<T>(o),
                x => new ResultWithValue<T>(false, (T)self));
        }
        return new ResultWithValue<T>(false, (T)self);
    }
    /// <summary>Disassembles descendant nodes.</summary>
    /// <typeparam name="T">Type of the nodes.</typeparam>
    /// <param name="self">Current node.</param>
    /// <returns>Returns the detached nodes.</returns>
    public static IReadOnlyList<T> DisassembleDescendants<T>(this IMutableTreeNode<T> self) where T : IMutableTreeNode<T> {
        List<T> rmvs = new();
        foreach (var cld in self.Levelorder().Reverse()) rmvs.AddRange(cld.ClearChildren());
        rmvs.Reverse();
        return rmvs.AsReadOnly();

        //var lst = self.Levelorder().ToArray();
        //foreach (var cld in lst) {
        //    rmvs.AddRange(cld.ClearChildren().OfType<T>());
        //}
        //return rmvs.AsReadOnly();
    }
    /// <summary>Disassembles from the current node to the leaves.</summary>
    /// <typeparam name="T">Type of the nodes.</typeparam>
    /// <param name="self">Current node.</param>
    /// <returns>Returns the detached nodes.</returns>
    public static IReadOnlyList<T> Disassemble<T>(this IMutableTreeNode<T> self) where T : IMutableTreeNode<T> {
        List<T> rmvs=new();
        self.TryRemoveOwn().When(o => rmvs.Add(o));
        rmvs.AddRange(self.DisassembleDescendants());
        return rmvs.AsReadOnly();
    }
    /// <summary>Removes all child nodes that match the specified condition.</summary>
    /// <typeparam name="T">Type of the nodes.</typeparam>
    /// <param name="self">Current node.</param>
    /// <param name="predicate">Condition for removal (true if it should be removed).</param>
    /// <returns>The removed nodes.</returns>
    public static IReadOnlyList<T> RemoveAllChild<T>(this IMutableTreeNode<T> self, Predicate<T> predicate) where T : IMutableTreeNode<T> {
        return TreeNodeExtenstions.RemoveAllChild(self, predicate,(p,c)=>p.RemoveChild(c));
    }
    /// <summary>Removes all child nodes that match the specified condition.</summary>
    /// <typeparam name="T">Type of the nodes.</typeparam>
    /// <param name="self">Current node.</param>
    /// <param name="predicate">Condition for removal (true if it should be removed).</param>
    /// <param name="removeAction">Function to break the relationship between the parent and the child. Takes the parent node as the first argument and the child node as the second argument.</param>
    /// <returns>The removed nodes.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IReadOnlyList<T> RemoveAllChild<T>(this ITreeNode<T> self,Predicate<T> predicate,Action<T,T> removeAction) where T : ITreeNode<T> {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        var lst = new List<T>();
        foreach (var cld in self.Children.OfType<T>().ToArray()) {
            if (predicate(cld)) {
                self.TryRemoveChild(cld,removeAction).When(o => lst.Add(cld));
            }
        }
        return lst;
    }
    /// <summary>Removes all nodes that match the condition in level order from the current node.</summary>
    /// <typeparam name="T">Type of the node</typeparam>
    /// <param name="self">Current node</param>
    /// <param name="predicate">Condition for removal, returns true for nodes to be deleted</param>
    /// <returns>The removed nodes</returns>
    public static IReadOnlyList<T> RemoveAllDescendant<T>(this IMutableTreeNode<T> self, Predicate<T> predicate) where T : IMutableTreeNode<T> {
        return TreeNodeExtenstions.RemoveAllDescendant<T>(self, predicate, (p, c) => p.RemoveChild(c));
    }
    /// <summary>Removes all nodes that match the condition in level order from the current node.</summary>
    /// <typeparam name="T">Type of the node</typeparam>
    /// <param name="self">Current node</param>
    /// <param name="predicate">Condition for removal, returns true for nodes to be deleted</param>
    /// <param name="removeAction">Function to dissolve the relationship, taking the parent node as the first argument and the child node as the second.</param>
    /// <returns>The removed nodes</returns>
    public static IReadOnlyList<T> RemoveAllDescendant<T>(this ITreeNode<T> self,Predicate<T> predicate,Action<T,T> removeAction)where T : ITreeNode<T> {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if(removeAction==null) throw new ArgumentNullException(nameof(removeAction));
        var lst = new List<T>();
        var dm = self.Evolve(a => {
            lst.AddRange(a.RemoveAllChild(predicate, removeAction).OfType<T>());
            return a.Children;
        }, (a, b, c) => new T?[1] { a }.Concat(c).Concat(b))
            .LastOrDefault();
        return lst;
    }
    #endregion
}

