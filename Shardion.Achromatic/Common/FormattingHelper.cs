using System;
using System.Numerics;
using System.Text;
using Disqord;

namespace Shardion.Achromatic.Common
{
    public static class FormattingHelper
    {
        public static string FormatValue(object? value, bool wrap = false)
        {
            if (value is null)
            {
                return wrap ? "`<null>`" : "<null>";
            }

            if (value is Array array)
            {
                StringBuilder arrayBuilder = new("[");
                for (int index = 0; index < array.Length; index++)
                {
                    arrayBuilder.Append($"{FormatValue(array.GetValue(index), wrap)}");
                    if (index < array.GetUpperBound(0))
                    {
                        arrayBuilder.Append(',');
                    }
                }
                arrayBuilder.Append(']');
                return arrayBuilder.ToString();
            }

            if (value is ulong id)
            {
                if (wrap)
                {
                    return $"`{FormatId(id)}`";
                }
                else
                {
                    return FormatId(id);
                }
            }

            if (value is int intNum)
            {
                return FormatNumber(intNum, wrap);
            }
            if (value is uint uintNum)
            {
                return FormatNumber(uintNum, wrap);
            }
            if (value is long longNum)
            {
                return FormatNumber(longNum, wrap);
            }
            if (value is ulong ulongNum)
            {
                return FormatNumber(ulongNum, wrap);
            }

            if (value is byte byteValue)
            {
                if (wrap)
                {
                    return $"`{Convert.ToHexString([byteValue])}`";
                }
                else
                {
                    return Convert.ToHexString([byteValue]);
                }
            }

            if (value is IEmoji emoji)
            {
                return emoji.Name ?? (wrap ? "`<null>`" : "<null>");
            }

            if (value is string str)
            {
                return $"\"{str}\"";
            }

            if (value.ToString() is string valueString)
            {
                if (wrap)
                {
                    return $"`{valueString}`";
                }
                else
                {
                    return valueString;
                }
            }
            else
            {
                return wrap ? "`<null>`" : "<null>";
            }
        }

        public static string FormatId(ulong id)
        {
            return id.ToString();
        }

        public static string FormatNumber<TNumber>(TNumber number, bool wrap = false) where TNumber : INumber<TNumber>
        {
            return number.ToString() ?? (wrap ? "`<null>`" : "<null>");
        }

        public static string MakeSentenceParseable(string sentence)
        {
            return sentence.ToLowerInvariant().Replace(",", "").Replace("\"", "").Replace(".", "");
        }
    }
}
