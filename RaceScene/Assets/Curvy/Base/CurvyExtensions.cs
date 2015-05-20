using UnityEngine;
using System.Collections;
using System;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Enum extensions
    /// </summary>
    public static class EnumExt
    {
        /// <summary>
        /// Checks if at least one of the provided flags is set in variable
        /// </summary>
        public static bool HasFlag(this Enum variable, params Enum[] flags)
        {
            if (flags.Length == 0)
                throw new ArgumentNullException("flags");

            Type T = variable.GetType();
            for (int i = 0; i < flags.Length; i++)
            {
                if (!Enum.IsDefined(T, flags[i]))
                {
                    throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    flags[i].GetType(), T));
                }
                ulong num = Convert.ToUInt64(flags[i]);
                if ((Convert.ToUInt64(variable) & num) == num)
                    return true;
            }
            return false;
        }

        public static T Set<T>(this Enum value, T append) { return Set(value,append,true);}
        public static T Set<T>(this Enum value, T append, bool OnOff)
        {
            if (append == null)
                throw new ArgumentNullException("append");

            Type type = value.GetType();
            //return the final value
            if (OnOff)
                return (T) Enum.Parse(type,(Convert.ToUInt64(value) | Convert.ToUInt64(append)).ToString());
            else
                return (T)Enum.Parse(type, (Convert.ToUInt64(value) & ~Convert.ToUInt64(append)).ToString());
        }

        public static T SetAll<T>(this Enum value)
        {
            Type type = value.GetType();
            object result = value;
            string[] names = Enum.GetNames(type);
            foreach (var name in names)
            {
                ((Enum)result).Set(Enum.Parse(type, name));
            }

            return (T)result;
        }

    }
}