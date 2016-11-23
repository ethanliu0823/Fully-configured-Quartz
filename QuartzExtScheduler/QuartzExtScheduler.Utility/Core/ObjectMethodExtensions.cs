using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QuartzExtScheduler.Utility.Core
{
    /// <summary>
    /// 对象方法扩展
    /// </summary>
    public static class ObjectMethodExtensions
    {
        #region Object
        /// <summary>
        /// 获取对象字符串。
        /// </summary>
        /// <param name="o">要测试的对象。</param>
        /// <param name="trim">是否去掉空格。</param>
        /// <returns>如果 o 参数为 null，则为 System.String.Empty；否则为 o参数对象 ToString方法的值。</returns>
        public static string GetString(this object o, bool trim)
        {
            return GetString(Convert.ToString(o), trim);
        }
        /// <summary>
        /// 获取对象字符串。(默认去掉前后空白)
        /// </summary>
        /// <param name="o">要测试的对象。</param>
        /// <returns>如果 o 参数为 null，则为 System.String.Empty；否则为 o参数对象 ToString方法的值。</returns>
        public static string GetString(this object o)
        {
            return GetString(o, true);
        }
        /// <summary>
        /// 获取 32 位有符号整数。
        /// </summary>
        /// <param name="value">要测试的字符串</param>
        /// <param name="trim">是否去掉前后空白</param>
        /// <param name="initial">初始值（默认值）</param>
        /// <returns>如果转换成功，返回32 位有符号整数,否则返回初始值。</returns>
        public static int GetInt(this object o, bool trim, int initial)
        {
            return GetInt(Convert.ToString(o), trim, initial);
        }
        /// <summary>
        /// 获取 32 位有符号整数。(默认去掉前后空白)
        /// </summary>
        /// <param name="value">要测试的字符串</param>
        /// <param name="initial">初始值（默认值）</param>
        /// <returns>如果转换成功，返回32 位有符号整数,否则返回初始值。</returns>
        public static int GetInt(this object o, int initial)
        {
            return GetInt(o, true, initial);
        }
        /// <summary>
        /// 获取 32 位有符号整数。(默认去掉前后空白)
        /// </summary>
        /// <param name="value">要测试的字符串</param>
        /// <returns>如果转换成功，返回32 位有符号整数,否则返回零。</returns>
        public static int GetInt(this object o)
        {
            return GetInt(o, 0);
        }
        #endregion

        #region String
        /// <summary>
        /// 获取字符串。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="trim">是否去掉前后空白。</param>
        /// <returns>如果 value 参数为 null，则为 System.String.Empty；否则为value。</returns>
        public static string GetString(this string value, bool trim)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return trim ? value.Trim() : value;
        }
        /// <summary>
        /// 获取字符串。(默认去掉前后空白)
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value 参数为 null，则为 System.String.Empty；否则为value。</returns>
        public static string GetString(this string value)
        {
            return GetString(value, true);
        }
        /// <summary>
        /// 获取 32 位有符号整数。
        /// </summary>
        /// <param name="value">要测试的字符串</param>
        /// <param name="trim">是否去掉前后空白</param>
        /// <param name="initial">初始值（默认值）</param>
        /// <returns>如果转换成功，返回32 位有符号整数,否则返回初始值。</returns>
        public static int GetInt(this string value, bool trim, int initial)
        {
            int result;
            if (int.TryParse(GetString(value, trim), out result))
                return result;
            return initial;
        }
        /// <summary>
        /// 获取 32 位有符号整数。(默认去掉前后空白)
        /// </summary>
        /// <param name="value">要测试的字符串</param>
        /// <param name="initial">初始值（默认值）</param>
        /// <returns>如果转换成功，返回32 位有符号整数,否则返回初始值。</returns>
        public static int GetInt(this string value, int initial)
        {
            return GetInt(value, true, initial);
        }
        /// <summary>
        /// 获取 32 位有符号整数。(默认去掉前后空白)
        /// </summary>
        /// <param name="value">要测试的字符串</param>
        /// <returns>如果转换成功，返回32 位有符号整数,否则返回零。</returns>
        public static int GetInt(this string value)
        {
            return GetInt(value, true, 0);
        }
        /// <summary>
        /// 指示指定的字符串是不是有符号整数。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value是整数，则为 true；否则为 false。</returns>
        public static bool IsInteger(this string value)
        {
            return IsMatch(value, @"^-?\d+$");
        }
        /// <summary>
        /// 指示指定的字符串是不是正整数(大于0的整数)。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value是正整数(大于0的整数)，则为 true；否则为 false。</returns>
        public static bool IsPositiveInteger(this string value)
        {
            return IsMatch(value, @"^[0-9]*[1-9][0-9]*$");
        }
        /// <summary>
        /// 指示指定的字符串是不是无符号整数(正整数+0)。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value是非负整数(正整数+0)，则为 true；否则为 false。</returns>
        public static bool IsUInteger(this string value)
        {
            return IsMatch(value, @"^\d+$");
        }
        /// <summary>
        /// 指示指定的字符串是不是只包含字母和数字,不区分大小写。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value只包含字母和数字(不区分大小写)，则为 true；否则为 false。</returns>
        public static bool IsOnlyLetterAndDigit(this string value)
        {
            return IsMatch(value, @"^[a-zA-Z0-9-]*$");
        }
        /// <summary>
        /// 指示指定的字符串是不是电子邮箱。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value是电子邮箱，则为 true；否则为 false。</returns>
        public static bool IsEmail(this string value)
        {
            return IsMatch(value, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }
        /// <summary>
        /// 指示指定的字符串是不是中文。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value是中文，则为 true；否则为 false。</returns>
        public static bool IsChinese(this string value)
        {
            return IsMatch(value, @"^[\u4e00-\u9fa5]{0,}$");
        }
        /// <summary>
        /// 指示指定的字符串是不是互联网网址。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value是互联网网址，则为 true；否则为 false。</returns>
        public static bool IsInternetURL(this string value)
        {
            return IsMatch(value, @"^(https|http)://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$");
        }
        /// <summary>
        /// 指示指定的字符串是不是DateTime(日期和时间)。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="trim">是否去掉前后空白</param>
        /// <returns>如果 value是DateTime，则为 true；否则为 false。</returns>
        public static bool IsDateTime(this string value)
        {
            try
            {
                Convert.ToDateTime(GetString(value));
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 获取匹配的正则表达式的正则对象。
        /// </summary>
        /// <param name="pattern">要匹配的正则表达式模式。</param>
        /// <returns>匹配的正则对象。</returns>
        public static Regex GetRegex(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            return new Regex(pattern);
        }

        /// <summary>
        /// 获取匹配的正则表达式的正则对象。
        /// </summary>
        /// <param name="pattern">要匹配的正则表达式模式。</param>
        /// <param name="options">提供用于设置正则表达式选项的枚举值。</param>
        /// <returns>匹配的正则对象。</returns>
        public static Regex GetRegex(string pattern, RegexOptions options)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            return new Regex(pattern, options);
        }

        /// <summary>
        /// 指示 System.Text.RegularExpressions.Regex 构造函数中指定的正则表达式在指定的输入字符串中是否找到了匹配项。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="regex">正则对象。</param>
        /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false。</returns>
        public static bool IsMatch(this string value, Regex regex)
        {
            if (regex == null)
                throw new ArgumentNullException("regex");
            return value != null && regex.IsMatch(value);
        }

        /// <summary>
        /// 指示 System.Text.RegularExpressions.Regex 构造函数中指定的正则表达式在指定的输入字符串中是否找到了匹配项。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="regex">正则对象。</param>
        /// <param name="startat">开始搜索的字符位置。</param>
        /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false。</returns>
        public static bool IsMatch(this string value, Regex regex, int startat)
        {
            if (regex == null)
                throw new ArgumentNullException("regex");
            if (startat < 0 || startat > value.Length)
                throw new ArgumentOutOfRangeException("startat");
            return value != null && regex.IsMatch(value, startat);
        }

        /// <summary>
        /// 指示 System.Text.RegularExpressions.Regex 构造函数中指定的正则表达式在指定的输入字符串中是否找到了匹配项。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="pattern">匹配的正则表达式。</param>
        /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false。</returns>
        public static bool IsMatch(this string value, string pattern)
        {
            return IsMatch(value, GetRegex(pattern));
        }

        /// <summary>
        /// 指示 System.Text.RegularExpressions.Regex 构造函数中指定的正则表达式在指定的输入字符串中是否找到了匹配项。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="pattern">匹配的正则表达式。</param>
        /// <param name="startat">开始搜索的字符位置。</param>
        /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false。</returns>
        public static bool IsMatch(this string value, string pattern, int startat)
        {
            return IsMatch(value, GetRegex(pattern), startat);
        }

        /// <summary>
        /// 指示 System.Text.RegularExpressions.Regex 构造函数中指定的正则表达式在指定的输入字符串中是否找到了匹配项。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="pattern">匹配的正则表达式。</param>
        /// <param name="options">提供用于设置正则表达式选项的枚举值。</param>
        /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false。</returns>
        public static bool IsMatch(this string value, string pattern, RegexOptions options)
        {
            return IsMatch(value, GetRegex(pattern, options));
        }

        /// <summary>
        /// 指示 System.Text.RegularExpressions.Regex 构造函数中指定的正则表达式在指定的输入字符串中是否找到了匹配项。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="pattern">匹配的正则表达式。</param>
        /// <param name="options">提供用于设置正则表达式选项的枚举值。</param>
        /// <param name="startat">开始搜索的字符位置。</param>
        /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false。</returns>
        public static bool IsMatch(this string value, string pattern, RegexOptions options, int startat)
        {
            return IsMatch(value, GetRegex(pattern, options), startat);
        }

        /// <summary>
        /// 获取指定字符串的字节长度。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="trim">是否去掉前后空白。</param>
        /// <returns>如果 value 参数为 null，则为0；否则，为字符串的字节长度。</returns>
        public static int GetCharLength(this string value, bool trim)
        {
            return !string.IsNullOrEmpty(value) ? System.Text.Encoding.Default.GetByteCount(GetString(value, trim)) : 0;
        }

        /// <summary>
        /// 获取指定字符串的字节长度。(默认去掉前后空白)
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value 参数为 null，则为0；否则，为字符串的字节长度。</returns>
        public static int GetCharLength(this string value)
        {
            return GetCharLength(value, true);
        }

        /// <summary>
        /// 指定字符串是否匹配字节范围。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="trim">是否去掉前后空白。</param>
        /// <param name="minLength">最小长度。</param>
        /// <param name="maxLength">最大长度。</param>
        /// <returns>如果 value参数为 null，则为false；如果 value匹配字节范围，则为 true；否则，为 false。</returns>
        public static bool IsMatchRangeChar(this string value, bool trim, int minLength, int maxLength)
        {
            int range = GetCharLength(value, trim);
            return minLength <= range && range <= maxLength;
        }

        /// <summary>
        /// 指定字符串是否匹配字节范围。(默认去掉前后空白)
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="minLength">最小长度。</param>
        /// <param name="maxLength">最大长度。</param>
        /// <returns>如果 value参数为 null，则为false；如果 value匹配字节范围，则为 true；否则，为 false。</returns>
        public static bool IsMatchRangeChar(this string value, int minLength, int maxLength)
        {
            return IsMatchRangeChar(value, true, minLength, maxLength);
        }

        /// <summary>
        /// 获取指定字符串的字符数。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="trim">是否去掉前后空白。</param>
        /// <returns>如果 value 参数为 null，则为0；否则，为字符串的字符数。</returns>
        public static int GetLength(this string value, bool trim)
        {
            return GetString(value, trim).Length;
        }

        /// <summary>
        /// 获取指定字符串的字符数。(默认去掉前后空白)
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value 参数为 null，则为0；否则，为字符串的字符数。</returns>
        public static int GetLength(this string value)
        {
            return GetLength(value, true);
        }

        /// <summary>
        /// 指定字符串是否匹配字符数范围。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="trim">是否去掉前后空白。</param>
        /// <param name="minLength">最小长度。</param>
        /// <param name="maxLength">最大长度。</param>
        /// <returns>如果 value参数为 null，则为false；如果 value匹配字符数范围，则为 true；否则，为 false。</returns>
        public static bool IsMatchRangeLength(this string value, bool trim, int minLength, int maxLength)
        {
            int range = GetLength(value, trim);
            return minLength <= range && range <= maxLength;
        }

        /// <summary>
        /// 指定字符串是否匹配字符数范围。(默认去掉前后空白)
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <param name="minLength">最小长度。</param>
        /// <param name="maxLength">最大长度。</param>
        /// <returns>如果 value参数为 null，则为false；如果 value匹配字符数范围，则为 true；否则，为 false。</returns>
        public static bool IsMatchRangeLength(this string value, int minLength, int maxLength)
        {
            return IsMatchRangeLength(value, true, minLength, maxLength);
        }
        #endregion
    }
}
