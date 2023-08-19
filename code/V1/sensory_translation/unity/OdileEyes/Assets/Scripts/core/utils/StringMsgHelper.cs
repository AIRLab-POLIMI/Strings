
using System;
using System.Collections.Generic;
using UnityEngine;


public static class StringMsgHelper
{
    
    #region UTILS
        
        public static List<int> FindCharInString(string msg, char c) 
        {
            var locations = new List<int>();

            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == c)
                {
                    locations.Add(i);
                }
            }

            return locations;
        }

    #endregion


    #region PARSE STRING MESSAGES
    
        public static List<string> GetMsgWithDelimiter(string msg, char delimiter = Constants.MsgDelimiter) 
        {
            var msgs = new List<string>();

            // Debug.Log($"    --- msg: {msg}");
            
            // CASES
            // 1. empty message: return empty list
            // 2. no delimiters: return only one element, the msg ()
            //                   if not empty but with no delimiters, there is only one msg, the msg itself
            // 3. N delimiters: there are N+1 messages. Separate the string and return one msg for each portion

            if (msg.Length <= 0)
                return msgs;

            var locations = FindCharInString(msg, delimiter);
            var numLocations = locations.Count;
            
            if (numLocations <= 0)
            {
                msgs.Add(msg);
                return msgs;
            }

            var startIndex = 0;
            // add the first N messages
            for (var i = 0; i < numLocations; i++)
            {
                // Debug.Log($"    --- for loop: {i} - start index: '{startIndex}' - cur loc: '{locations[i]}' - len: '{locations[i] - startIndex}'");
                msgs.Add(msg.Substring(startIndex, locations[i] - startIndex));
                startIndex = locations[i] + 1;
            }
            // add the last one
            msgs.Add(msg.Substring(startIndex));
            
            return msgs;
        }

    #endregion
    
    
    #region STRING TO NUMERIC CONVERSION

        
        public static float StringToFloat(string msg, out bool success) 
        {
            try
            {
                success = true;
                return float.Parse(msg);
            }
            catch (Exception e)
            {
                success = false;
                Debug.Log($"[STRING MSG HELPER][StringToFloat] - ERROR: {e}");
                return 0;
            }
        }

    #endregion
}
