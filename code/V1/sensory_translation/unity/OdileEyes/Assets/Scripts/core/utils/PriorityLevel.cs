
// needs to be in DESCENDING order so that the underlying integers are ascending
// and the SORT function works correctly

using System.Collections.Generic;

[System.Serializable]
public enum PriorityLevel
{
    High,
    Medium,
    Low
}

public interface IPrioritized
{
    public PriorityLevel Priority { get; }
}

public static class PriorityLevelHelper{
    
    // sort in DESCENDING priority level (higher priority elements are FIRSt in the resulting list)
    // NB the list is modified INPLACE
    public static void SortByPriorityLevelDescending<T>(List<T> prioritizedList) where T : IPrioritized =>
        prioritizedList.Sort((a, b) => 
            a.Priority.CompareTo(b.Priority));
}
