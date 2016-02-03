using System;

namespace LumiSoft.MailServer.Filters
{
	/// <summary>
	/// Convert methods.
	/// </summary>
	public class ConvertEx
	{
		#region method ToString

		/// <summary>
		/// Converts object to string. If value == null, returns "".
		/// </summary>
		/// <param name="value">Value to  be converted.</param>
		/// <returns></returns>
		public static string ToString(object value)
		{
			if(value == null){
				return "";
			}
			else{
				return value.ToString();
			}
		}

		#endregion

		#region method ToBoolean

		/// <summary>
		/// Convert object to bool. If value == null or object can't be converted to bool, returns false.
		/// </summary>
		/// <param name="value">Value to  be converted.</param>
		/// <returns></returns>
		public static bool ToBoolean(object value)
		{
			if(value == null){
				return false;
			}
			else{
				try{
					return Convert.ToBoolean(value);
				}
				catch{
					return false;
				}
			}
		}

		#endregion

		#region method ToInt32

		/// <summary>
		/// Convert object to int. If value == null or object can't be converted to int, returns 0.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int ToInt32(object value)
		{
			return ToInt32(value,0);
		}

		/// <summary>
		/// Convert object to int. If value == null or object can't be converted to int, returns 0.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="defaultValue">Default value if value == null.</param>
		/// <returns></returns>
		public static int ToInt32(object value,int defaultValue)
		{
			if(value == null){
				return defaultValue;
			}
			else{
				try{
					return Convert.ToInt32(value);
				}
				catch{
					return defaultValue;
				}
			}
		}

		#endregion
	}
}
