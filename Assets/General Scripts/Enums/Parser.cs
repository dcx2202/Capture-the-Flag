using System;
using System.Collections.Generic;
using System.Linq;

namespace General_Scripts.Enums
{
    /// <summary>
    /// Static support class for Enums
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// Parses any string into any enum. If the parses failts, returns the TEnum default.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TEnum StringToEnum<TEnum>(string value) where TEnum : struct
        {
            try
            {
                return (TEnum)Enum.Parse(typeof(TEnum), value);
            }
            catch (Exception)
            {
                
                return default(TEnum);
            }
        }

        /// <summary>
        /// Gets a enumerable with all the values of the T enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
