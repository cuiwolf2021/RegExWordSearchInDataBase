using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class StringHelper
    {
        /**/
        /// <summary>
        /// 判断字符是否英文半角字符或标点
        /// </summary>
        /// <remarks>
        /// 32    空格
        /// 33-47    标点
        /// 48-57    0~9
        /// 58-64    标点
        /// 65-90    A~Z
        /// 91-96    标点
        /// 97-122    a~z
        /// 123-126  标点
        /// </remarks>
        private static bool IsSBC(char c)
        {
            int i = (int)c;
            return i >= 32 && i <= 126;
        }

        /**/
        /// <summary>
        /// 判断字符是否全角字符或标点
        /// </summary>
        /// <remarks>
        /// <para>全角字符 - 65248 = 半角字符</para>
        /// <para>全角空格例外</para>
        /// </remarks>
        private static bool IsDoubleBytesChar(char c)
        {
            if (c == '\u3000') return true;

            int i = (int)c - 65248;
            if (i < 32) return false;
            return IsSBC((char)i);
        }

        /// <summary>
        /// 将字符串中的全角字符转换为半角
        /// </summary>
        public static string ToSBC(string s)
        {
            if (s == null || s.Trim() == string.Empty) return s;

            StringBuilder sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\u3000')
                    sb.Append('\u0020');
                else if (IsDoubleBytesChar(s[i]))
                    sb.Append((char)((int)s[i] - 65248));
                else
                    sb.Append(s[i]);
            }

            return sb.ToString();
        }


        /* http://www.cnblogs.com/roucheng/ */
        /// <summary>
        /// 将字符串中的全角字符转换为半角
        /// </summary>
        private static string ToSBCNotUsed(string type, string s)
        {
            if (s == null || s.Trim() == string.Empty) return s;

            StringBuilder sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\u3000')
                    sb.Append('\u0020');
                else if (IsDoubleBytesChar(s[i]))
                    sb.Append((char)((int)s[i] - 65248));
                else
                    sb.Append(s[i]);
            }

            //如果是int类型的，则只能输入int类型，否则自动设为0

            if (type.ToLower() == "int")
            {
                try
                {
                    Convert.ToInt32(sb.ToString());
                }
                catch
                {
                    return "0";
                }

            }



            //如果是float或double类型的，则只能输入这两种类型，否则自动设为0
            if (type.ToLower() == "float" || type.ToLower() == "double")
            {
                try
                {
                    Convert.ToDouble(sb.ToString());
                }
                catch
                {
                    return "0";
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 去除标点符号、转化为单字节、转化为大写、去掉任何空格、xx路yy号中的xx变为汉字且yy变为数字
        /// “丈8” 变成“丈八”
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Format(string str)
        {
            string symbol = "[ \\[ \\] \\^ \\-_*×――(^)（^）$%~!@#$…&%￥—+=<>《》!！??？:：●•`·．、。，；,.;\"‘’“”-]";
            var v = Regex.Replace(str, symbol, "", RegexOptions.Compiled);
            v = SpecificReplace(v);
            return ConvertDigitRoadNumHao(ToSBC(v).ToUpper());
        }

        /// <summary>
        /// 将丈8、2环、3环、替换为丈八、二环、三环
        /// 路、街后面的十字、什子等替换为“什字”，不是路、街后面的不替换
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static string SpecificReplace(string v)
        {
            string s1 = v.Replace("丈8", "丈八");
            string s2 = s1.Replace("2环", "二环").Replace("3环", "三环");
            string s3 = Regex.Replace(s2, "(路|街)[十什][子字]", "$1什字");
            return s3;
        }

        /// <summary>
        /// 去除标点符号、转化为单字节、转化为大写、xx路yy号中的xx变为汉字且yy变为数字
        /// “丈8” 变成“丈八”
        /// <para>注意：1.保留各类横线，并转化为-；2.其他标点符号等特殊字符转化为空格；3.去除头尾的空格</para>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FormatRetainDash(string str)
        {
            string ss = Regex.Replace(ToSBC(str).ToUpper(), "[^-_—―–ˉ‐¯﹍￣＿a-zA-Z0-9\u4e00-\u9fa5]", " ", RegexOptions.Compiled);
            ss = Regex.Replace(ss.Trim(), "[-_—―–ˉ‐¯﹍￣＿]+", "-", RegexOptions.Compiled);
            return ConvertDigitRoadNumHao(SpecificReplace(ss));
        }

        /// <summary>
        /// 将xx路yy号中的阿拉伯数字xx变成汉字，汉字数字yy变成阿拉伯数字
        /// </summary>
        /// <param name="str">可能含有“xx路yy号”的字符串</param>
        /// <returns>转换后的整个字符串</returns>
        private static string ConvertDigitRoadNumHao(string str)
        {
            string result = str;
            //替换“3路”为“三路”。(?=[路街])为环视之 lookahead,右侧跟的是路或街
            //“101街坊”等不替换
            // 注意：正则表达式中想表示“非”，需要用字符类，即中括号[]括起来。
            Match match = Regex.Match(str, @"(\d+)(?=[路街][^坊])", RegexOptions.Compiled);//捕获数字
            string m1 = match.Value;
            if (m1 != "")
            {
                result = result.Substring(0, match.Index) + Digit2Chinese(m1) + result.Substring(match.Index + match.Length);
            }

            //替换“三路四号”为“三路4号”
            //(?<=[路街](.段)?[副付负]?) 为环视之lookbehind，前边有路或街，后边可能跟东段等，可能跟副付负。
            match = Regex.Match(str, @"(?<=[路街][东西南北]?[侧段]?[副付负]?)([零一二三四五六七八九十百千万亿]+)(?=号)", RegexOptions.Compiled);//捕获汉字数字
            m1 = match.Value;

            if (m1 != "")
            {
                result = result.Substring(0, match.Index) + Chinese2Digits(m1) + result.Substring(match.Index + match.Length);
            }

            return result;
        }


        #region 复制自网络,已测试.https://www.cnblogs.com/yellow3gold/archive/2019/08/12/11338374.html 
        /// <summary>
        /// 阿拉伯数字转换成中文数字
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string Digit2Chinese(string x)
        {
            string[] pArrayNum = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            //为数字位数建立一个位数组  
            string[] pArrayDigit = { "", "十", "百", "千" };
            //为数字单位建立一个单位数组  
            string[] pArrayUnits = { "", "万", "亿", "万亿" };
            var pStrReturnValue = ""; //返回值  
            var finger = 0; //字符位置指针  
            var pIntM = x.Length % 4; //取模  
            int pIntK;
            if (pIntM > 0)
                pIntK = x.Length / 4 + 1;
            else
                pIntK = x.Length / 4;
            //外层循环,四位一组,每组最后加上单位: ",万亿,",",亿,",",万,"  
            for (var i = pIntK; i > 0; i--)
            {
                var pIntL = 4;
                if (i == pIntK && pIntM != 0)
                    pIntL = pIntM;
                //得到一组四位数  
                var four = x.Substring(finger, pIntL);
                var P_int_l = four.Length;
                //内层循环在该组中的每一位数上循环  
                for (int j = 0; j < P_int_l; j++)
                {
                    //处理组中的每一位数加上所在的位  
                    int n = Convert.ToInt32(four.Substring(j, 1));
                    if (n == 0)
                    {
                        if (j < P_int_l - 1 && Convert.ToInt32(four.Substring(j + 1, 1)) > 0 && !pStrReturnValue.EndsWith(pArrayNum[n]))
                            pStrReturnValue += pArrayNum[n];
                    }
                    else
                    {
                        if (!(n == 1 && (pStrReturnValue.EndsWith(pArrayNum[0]) | pStrReturnValue.Length == 0) && j == P_int_l - 2))
                            pStrReturnValue += pArrayNum[n];
                        pStrReturnValue += pArrayDigit[P_int_l - j - 1];
                    }
                }
                finger += pIntL;
                //每组最后加上一个单位:",万,",",亿," 等  
                if (i < pIntK) //如果不是最高位的一组  
                {
                    if (Convert.ToInt32(four) != 0)
                        //如果所有4位不全是0则加上单位",万,",",亿,"等  
                        pStrReturnValue += pArrayUnits[i - 1];
                }
                else
                {
                    //处理最高位的一组,最后必须加上单位  
                    pStrReturnValue += pArrayUnits[i - 1];
                }
            }
            return pStrReturnValue;
        }

        /// <summary>
        /// 转换数字
        /// </summary>
        private static long CharToNumber(char c)
        {
            switch (c)
            {
                case '一': return 1;
                case '二': return 2;
                case '三': return 3;
                case '四': return 4;
                case '五': return 5;
                case '六': return 6;
                case '七': return 7;
                case '八': return 8;
                case '九': return 9;
                case '零': return 0;
                default: return -1;
            }
        }

        /// <summary>
        /// 转换单位
        /// </summary>
        private static long CharToUnit(char c)
        {
            switch (c)
            {
                case '十': return 10;
                case '百': return 100;
                case '千': return 1000;
                case '万': return 10000;
                case '亿': return 100000000;
                default: return 1;
            }
        }
        /// <summary>
        /// 将中文数字转换阿拉伯数字
        /// </summary>
        /// <param name="cnum">汉字数字</param>
        /// <returns>长整型阿拉伯数字字符串</returns>
        public static string Chinese2Digits(string cnum)
        {
            cnum = Regex.Replace(cnum, "\\s+", "");
            long firstUnit = 1;//一级单位                
            long secondUnit = 1;//二级单位 
            long result = 0;//结果
            for (var i = cnum.Length - 1; i > -1; --i)//从低到高位依次处理
            {
                var tmpUnit = CharToUnit(cnum[i]);//临时单位变量
                if (tmpUnit > firstUnit)//判断此位是数字还是单位
                {
                    firstUnit = tmpUnit;//是的话就赋值,以备下次循环使用
                    secondUnit = 1;
                    if (i == 0)//处理如果是"十","十一"这样的开头的
                    {
                        result += firstUnit * secondUnit;
                    }
                    continue;//结束本次循环
                }
                if (tmpUnit > secondUnit)
                {
                    secondUnit = tmpUnit;
                    continue;
                }
                result += firstUnit * secondUnit * CharToNumber(cnum[i]);//如果是数字,则和单位想乘然后存到结果里
            }
            return result.ToString();
        }
        #endregion

        #region
        public static bool StartsWithIgnoreCase(string str, char ch)
        {
            Regex re = new Regex($"^{ch}", RegexOptions.IgnoreCase);
            return re.IsMatch(str);
        }
        #endregion
    }

}