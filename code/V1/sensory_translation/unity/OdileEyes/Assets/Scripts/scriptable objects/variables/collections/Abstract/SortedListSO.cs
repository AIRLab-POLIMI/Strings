

public abstract class SortedListSO<T> : ListSO<T> where T : IOrdered
{
    // the type must inherit from an "IntPrioritizable" interface, which has an "Order" value
    // Every time an element is added to the list, the list is sorted again, based on that sort value.

    public override void Add(T value)
    {
        base.Add(value);
        
        OrderHelper.SortByOrderDescending(list);
    }
}
