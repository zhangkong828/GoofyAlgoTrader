using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader
{
    public static class ObjectExtension
    {
        private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;
        private static readonly IFormatProvider FormatProvider = CultureInfo;
        private static readonly StringComparison StringComparison = StringComparison.InvariantCulture;

        /// <summary>
        /// Converts the provided <paramref name="value"/> as <typeparamref name="T"/>
        /// using <see cref="CultureInfo"/>
        /// </summary>
        public static T ConvertInvariant<T>(this object value)
        {
            return (T)value.ConvertInvariant(typeof(T));
        }

        /// <summary>
        /// Converts the provided <paramref name="value"/> as <paramref name="conversionType"/>
        /// using <see cref="CultureInfo"/>
        /// </summary>
        /// <remarks>
        /// This implementation uses the Convert.ToXXX methods. This causes null values to be converted to the default value
        /// for the provided <paramref name="conversionType"/>. This is in contrast to directly calling <see cref="IConvertible.ToType"/>
        /// which results in an <see cref="InvalidCastException"/> or a <see cref="FormatException"/>. Since existing code is
        /// dependent on this null -> default value conversion behavior, it has been preserved in this method.
        /// </remarks>
        public static object ConvertInvariant(this object value, Type conversionType)
        {
            switch (Type.GetTypeCode(conversionType))
            {
                // these cases are purposefully ordered to ensure the compiler can generate a jump table vs a binary tree
                case TypeCode.Empty:
                    throw new ArgumentException("StringExtensions.ConvertInvariant does not support converting to TypeCode.Empty");

                case TypeCode.Object:
                    var convertible = value as IConvertible;
                    if (convertible != null)
                    {
                        return convertible.ToType(conversionType, FormatProvider);
                    }

                    return Convert.ChangeType(value, conversionType, FormatProvider);

                case TypeCode.DBNull:
                    throw new ArgumentException("StringExtensions.ConvertInvariant does not support converting to TypeCode.DBNull");

                case TypeCode.Boolean:
                    return Convert.ToBoolean(value, FormatProvider);

                case TypeCode.Char:
                    return Convert.ToChar(value, FormatProvider);

                case TypeCode.SByte:
                    return Convert.ToSByte(value, FormatProvider);

                case TypeCode.Byte:
                    return Convert.ToByte(value, FormatProvider);

                case TypeCode.Int16:
                    return Convert.ToInt16(value, FormatProvider);

                case TypeCode.UInt16:
                    return Convert.ToUInt16(value, FormatProvider);

                case TypeCode.Int32:
                    return Convert.ToInt32(value, FormatProvider);

                case TypeCode.UInt32:
                    return Convert.ToUInt32(value, FormatProvider);

                case TypeCode.Int64:
                    return Convert.ToInt64(value, FormatProvider);

                case TypeCode.UInt64:
                    return Convert.ToUInt64(value, FormatProvider);

                case TypeCode.Single:
                    return Convert.ToSingle(value, FormatProvider);

                case TypeCode.Double:
                    return Convert.ToDouble(value, FormatProvider);

                case TypeCode.Decimal:
                    return Convert.ToDecimal(value, FormatProvider);

                case TypeCode.DateTime:
                    return Convert.ToDateTime(value, FormatProvider);

                case TypeCode.String:
                    return Convert.ToString(value, FormatProvider);

                default:
                    return Convert.ChangeType(value, conversionType, FormatProvider);
            }
        }

        /// <summary>
        /// Converts the provided value to a string using <see cref="CultureInfo"/>
        /// </summary>
        public static string ToStringInvariant(this IConvertible convertible)
        {
            if (convertible == null)
            {
                return string.Empty;
            }

            return convertible.ToString(FormatProvider);
        }
    }
}
