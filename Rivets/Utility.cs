// 
// System.Web.HttpUtility
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

namespace Rivets
{

	public sealed class Utility
	{
		#region Constructors
		public Utility () 
		{
		}
		#endregion // Constructors

		#region Methods
		public static Dictionary<string, string> ParseQueryString(string query)
		{
			var queryDict = new Dictionary<string, string>();

			if (string.IsNullOrEmpty(query))
				return queryDict;

			foreach (string token in query.TrimStart(new char[] { '?' }).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] parts = token.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 2)
					queryDict[parts[0].Trim()] = Utility.UrlDecode(parts[1]).Trim();
				else
					queryDict[parts[0].Trim()] = "";
			}
			return queryDict;
		}

		public static string UrlDecode (string str) 
		{
			return UrlDecode(str, Encoding.UTF8);
		}

		static void WriteCharBytes (IList buf, char ch, Encoding e)
		{
			if (ch > 255) {
				foreach (byte b in e.GetBytes (new char[] { ch }))
					buf.Add (b);
			} else
				buf.Add ((byte)ch);
		}

		public static string UrlDecode (string s, Encoding e)
		{
			if (null == s) 
				return null;

			if (s.IndexOf ('%') == -1 && s.IndexOf ('+') == -1)
				return s;

			if (e == null)
				e = Encoding.UTF8;

			long len = s.Length;
			var bytes = new List <byte> ();
			int xchar;
			char ch;

			for (int i = 0; i < len; i++) {
				ch = s [i];
				if (ch == '%' && i + 2 < len && s [i + 1] != '%') {
					if (s [i + 1] == 'u' && i + 5 < len) {
						// unicode hex sequence
						xchar = GetChar (s, i + 2, 4);
						if (xchar != -1) {
							WriteCharBytes (bytes, (char)xchar, e);
							i += 5;
						} else
							WriteCharBytes (bytes, '%', e);
					} else if ((xchar = GetChar (s, i + 1, 2)) != -1) {
						WriteCharBytes (bytes, (char)xchar, e);
						i += 2;
					} else {
						WriteCharBytes (bytes, '%', e);
					}
					continue;
				}

				if (ch == '+')
					WriteCharBytes (bytes, ' ', e);
				else
					WriteCharBytes (bytes, ch, e);
			}

			byte[] buf = bytes.ToArray ();
			bytes = null;
			return e.GetString (buf, 0, buf.Length);

		}

		static int GetInt (byte b)
		{
			char c = (char) b;
			if (c >= '0' && c <= '9')
				return c - '0';

			if (c >= 'a' && c <= 'f')
				return c - 'a' + 10;

			if (c >= 'A' && c <= 'F')
				return c - 'A' + 10;

			return -1;
		}

		static int GetChar (byte [] bytes, int offset, int length)
		{
			int value = 0;
			int end = length + offset;
			for (int i = offset; i < end; i++) {
				int current = GetInt (bytes [i]);
				if (current == -1)
					return -1;
				value = (value << 4) + current;
			}

			return value;
		}

		static int GetChar (string str, int offset, int length)
		{
			int val = 0;
			int end = length + offset;
			for (int i = offset; i < end; i++) {
				char c = str [i];
				if (c > 127)
					return -1;

				int current = GetInt ((byte) c);
				if (current == -1)
					return -1;
				val = (val << 4) + current;
			}

			return val;
		}

		internal static bool NotEncoded (char c)
		{
			return (c == '!' || c == '(' || c == ')' || c == '*' || c == '-' || c == '.' || c == '_'
				#if !NET_4_0
				|| c == '\''
				#endif
			);
		}

		public static string UrlEncode(string str) 
		{
			return UrlEncode(str, Encoding.UTF8);
		}

		public static string UrlEncode (string s, Encoding Enc) 
		{
			if (s == null)
				return null;

			if (s == String.Empty)
				return String.Empty;

			bool needEncode = false;
			int len = s.Length;
			for (int i = 0; i < len; i++) {
				char c = s [i];
				if ((c < '0') || (c < 'A' && c > '9') || (c > 'Z' && c < 'a') || (c > 'z')) {
					if (NotEncoded (c))
						continue;

					needEncode = true;
					break;
				}
			}

			if (!needEncode)
				return s;

			// avoided GetByteCount call
			byte [] bytes = new byte[Enc.GetMaxByteCount(s.Length)];
			int realLen = Enc.GetBytes (s, 0, s.Length, bytes, 0);

			#if PORTABLE
			var strData = UrlEncodeToBytes (bytes, 0, realLen);
			return Encoding.UTF8.GetString (strData, 0, strData.Length);
			#else
			return Encoding.ASCII.GetString (UrlEncodeToBytes (bytes, 0, realLen));
			#endif
		}

		public static byte [] UrlEncodeToBytes (string str)
		{
			return UrlEncodeToBytes (str, Encoding.UTF8);
		}

		public static byte [] UrlEncodeToBytes (string str, Encoding e)
		{
			if (str == null)
				return null;

			if (str.Length == 0)
				return new byte [0];

			byte [] bytes = e.GetBytes (str);
			return UrlEncodeToBytes (bytes, 0, bytes.Length);
		}

		public static byte [] UrlEncodeToBytes (byte [] bytes)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return new byte [0];

			return UrlEncodeToBytes (bytes, 0, bytes.Length);
		}
			
		internal static byte[] UrlEncodeToBytes (byte[] bytes, int offset, int count)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");

			int blen = bytes.Length;
			if (blen == 0)
				return new byte [0];

			if (offset < 0 || offset >= blen)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0 || count > blen - offset)
				throw new ArgumentOutOfRangeException("count");

			MemoryStream result = new MemoryStream (count);
			int end = offset + count;
			for (int i = offset; i < end; i++)
				UrlEncodeChar ((char)bytes [i], result, false);

			return result.ToArray();
		}

		static char [] hexChars = "0123456789abcdef".ToCharArray ();
		internal static void UrlEncodeChar (char c, Stream result, bool isUnicode) {
			if (c > 255) {
				//FIXME: what happens when there is an internal error?
				//if (!isUnicode)
				//	throw new ArgumentOutOfRangeException ("c", c, "c must be less than 256");
				int idx;
				int i = (int) c;

				result.WriteByte ((byte)'%');
				result.WriteByte ((byte)'u');
				idx = i >> 12;
				result.WriteByte ((byte)hexChars [idx]);
				idx = (i >> 8) & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
				idx = (i >> 4) & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
				idx = i & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
				return;
			}

			if (c > ' ' && NotEncoded (c)) {
				result.WriteByte ((byte)c);
				return;
			}
			if (c==' ') {
				result.WriteByte ((byte)'+');
				return;
			}
			if (	(c < '0') ||
				(c < 'A' && c > '9') ||
				(c > 'Z' && c < 'a') ||
				(c > 'z')) {
				if (isUnicode && c > 127) {
					result.WriteByte ((byte)'%');
					result.WriteByte ((byte)'u');
					result.WriteByte ((byte)'0');
					result.WriteByte ((byte)'0');
				}
				else
					result.WriteByte ((byte)'%');

				int idx = ((int) c) >> 4;
				result.WriteByte ((byte)hexChars [idx]);
				idx = ((int) c) & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
			}
			else
				result.WriteByte ((byte)c);
		}

	#endregion // Methods
	}
}
