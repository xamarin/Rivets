using System;

using System.Collections.Generic;

namespace System.Collections.Generic
{
	/// Error	42	
	///		'System.Collections.Generic.Dictionary<string,object>' does not contain a 
	///		definition for 'ToArray' and no extension method 'ToArray' accepting a first 
	///		argument of type 'System.Collections.Generic.Dictionary<string,object>' could 
	///		be found (are you missing a using directive or an assembly reference?)	
	///		System.Json\System.Json.DLL_01_WP\Serialization\JavaScriptReader.cs	
	///		82	16	System.Json.DLL_01_WP

	/// <summary>
	/// 
	/// </summary>
	public static class DictionaryExtensions
	{
		public static KeyValuePair<TKey, TValue>[] ToArray<TKey, TValue>(this Dictionary<TKey, TValue> d)
		{
			List<KeyValuePair<TKey, TValue>> l = new List<KeyValuePair<TKey, TValue>>();
			foreach (KeyValuePair<TKey, TValue> kvp in d)
			{
				l.Add(kvp);
			}

			return l.ToArray();
		}
	}
}