
using System.Collections.Generic;

public static class Extensions
{
  
        public static KeyValuePair<string, string> Value(
           this KeyValuePair<string, string> first,
          string newVal)
        {
            return new KeyValuePair<string, string>(
                first.Key,
                newVal);
        }
   public static KeyValuePair<string,string> KeyValuePair (string key,string Val)
    {
        return new KeyValuePair<string, string>(key, Val);
    }
   
}
