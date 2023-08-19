
using System;
using UnityEngine;


public static class TimeHelper
{
    public static string TimeIntInSecToTimeInMinSecString(int timeInSec)
    {
        var mins = timeInSec / 60;
        var secs = timeInSec % 60;

        // if there are no minutes, DO NOT write 0 min; 
        var timeString = mins > 0 ? $"{mins} min " : "";
        
        // if there are some minutes and no seconds, DO NOT write 0 s
        timeString += mins > 0 && secs == 0 ? "" : $"{secs} s";

        return timeString;
    }

    public static TimeSpan SubtractTime(DateTime prev, DateTime now)
    {
        try
        {
            return now.Subtract(prev);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[TimeHelper][SubtractTime] - could not complete subtraction of datetime now: '{now}' and prev: '{prev}'.\nError: '{e}'");
            return TimeSpan.Zero;
        }
    }

    public static int TimeSubtractionInMinutes(DateTime prev, DateTime now)
    {
        var temp = SubtractTime(prev, now);
        return temp.Hours * 60 + temp.Minutes;
    }
}