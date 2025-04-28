using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeStructures.Collections;

namespace TreeStructures.Tree {
    /// <summary>
    /// Builds a tree structure of date collections.
    /// </summary>
    //public class DateTimeTree : DateTimeTree<DateTime> {

    //    /// <summary>
    //    /// Initializes a new instance.
    //    /// </summary>
    //    /// <param name="collection">Collection of <see cref="DateTime"/>.</param>
    //    /// <param name="Lv1Classes">Classification at level 1.</param>
    //    /// <param name="nextsLvClasses">Each classifications at levels 2 and above.</param>
    //    public DateTimeTree(IEnumerable<DateTime> collection, Func<DateTime, int> Lv1Classes, params Func<DateTime, int>[] nextsLvClasses)
    //        : base(collection, x => x, Lv1Classes, nextsLvClasses) { }
    //}
    /// <summary>
    /// Builds a tree structure from a collection of objects holding a date as a property.
    /// </summary>
    /// <typeparam name="T">Type of objects holding a date as a property.</typeparam>
    //public class DateTimeTree<T> : PartitionedTree<T, DateTime, int> {
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="DateTimeTree{T}"/> class.
    //    /// </summary>
    //    /// <param name="collection">A collection of objects that hold a date as a property.</param>
    //    /// <param name="valueSelector">Function to extract the date from each object.</param>
    //    /// <param name="Lv1Classes">Classification at level 1.</param>
    //    /// <param name="nextsLvClasses">Each classification at levels 2 and above.</param>
    //    public DateTimeTree(IEnumerable<T> collection, Func<T, DateTime> valueSelector, Func<DateTime, int> Lv1Classes, params Func<DateTime, int>[] nextsLvClasses)
    //    : base(collection, valueSelector, Lv1Classes, nextsLvClasses) { }

    //    //protected override Node InitializeRoot(InnerNodeBase innerNode) {
    //    //    return new DateTimeNode(innerNode);
    //    //}

    //    //public class DateTimeNode : Node {
    //    //    protected internal DateTimeNode(InnerNodeBase innerNode) : base(innerNode) { }
    //    //    static readonly IComparer<Node> comp = Comparer<Node>.Create((a, b) => {
    //    //        if (a.HasItemAndValue && b.HasItemAndValue) {
    //    //            return Comparer<DateTime>.Default.Compare(a.Value, b.Value);
    //    //        } else {
    //    //            return Comparer.Default.Compare(a.NodeClass, b.NodeClass);
    //    //        }
    //    //    });
    //    //    protected override IEnumerable<Node> SetupPublicChildCollection(CombinableChildWrapperCollection<Node> children) {
    //    //        var list = new ReadOnlySortFilterObservableCollection<Node>(children);
    //    //        list.SortBy(x => x, comp);
    //    //        return list.AsReadOnlyObservableCollection();
    //    //    }
    //    //    protected override Node GenerateChild(InnerNodeBase sourceChildNode) {
    //    //        return new DateTimeNode(sourceChildNode);
    //    //    }
    //    //}
    //}

}
