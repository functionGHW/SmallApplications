﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SimpleCmdLineParser
{
    /// <summary>
    /// 参数设置特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentAttribute : Attribute
    {
        public ArgumentAttribute()
        {
        }

        public ArgumentAttribute(string tagName) : this()
        {
            TagName = tagName;
        }

        /// <summary>
        /// 参数标签名
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 表示该参数是可选的
        /// </summary>
        public bool Optional { get; set; }
    }
    

    /// <summary>
    /// 参数解析异常类，包含出错信息
    /// </summary>
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
    }


    /// <summary>
    /// 参数解析类
    /// </summary>
    public class SimpleCmdLineParser
    {
        private class ArgumentInfo
        {
            public string TagName { get; set; }

            public bool Optional { get; set; }

            public PropertyInfo Property { get; set; }

            public bool IsSet { get; set; }
        }

        /// <summary>
        /// 解析命令行参数
        /// </summary>
        /// <typeparam name="T">参数定义类型</typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ParserException">解析参数过程出错，Message包含错误说明</exception>
        public static T Parse<T>(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var result = Activator.CreateInstance<T>();
            return (T)Parse(args, result, typeof(T));
        }

        /// <summary>
        /// 解析命令行参数到指定的model对象中
        /// </summary>
        /// <typeparam name="T">参数定义类型</typeparam>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <param name="resultType"></param>
        /// <returns></returns>
        /// <exception cref="ParserException">解析参数过程出错，Message包含错误说明</exception>
        public static object ParseToObject(string[] args, object result, Type resultType = null)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            return Parse(args, result, resultType ?? result.GetType());
        }

        private static object Parse(string[] args, object result, Type resultType)
        {
            var mapper = new Dictionary<string, ArgumentInfo>();
            foreach (var p in resultType.GetProperties())
            {
                var argAttr = FxExtensions.FirstOrDefault(p.GetCustomAttributes(typeof(ArgumentAttribute), true)) as ArgumentAttribute;
                if (argAttr == null)
                    continue;

                // 未指定TagName，默认使用"--{PropertyName}"作为TagName
                string tagName = string.IsNullOrEmpty(argAttr.TagName) ? "--" + p.Name : argAttr.TagName;
                tagName = tagName.ToLowerInvariant();
                var argInfo = new ArgumentInfo()
                {
                    TagName = tagName,
                    Optional = argAttr.Optional,
                    Property = p
                };
                mapper.Add(tagName, argInfo);
            }
            int index = 0;
            while (index < args.Length)
            {
                string tmp = args[index];
                if (mapper.TryGetValue(tmp.Trim().ToLowerInvariant(), out var argInfo))
                {
                    if (argInfo.IsSet)
                        throw new ParserException($"指定了重复参数：参数名={tmp}");

                    var prop = argInfo.Property;
                    // 布尔参数不需要读取值
                    if (prop.PropertyType == typeof(bool)
                        || prop.PropertyType == typeof(bool?))
                    {
                        prop.SetValue(result, true, null);
                    }
                    else
                    {
                        if (++index >= args.Length)
                            throw new ParserException($"缺少参数值: 参数名={tmp}");
                        try
                        {
                            object value = ConvertData(args[index], prop.PropertyType);
                            prop.SetValue(result, value, null);
                        }
                        catch (Exception)
                        {
                            throw new ParserException($"参数值转换错误: 参数名={tmp}");
                        }
                    }
                    argInfo.IsSet = true;
                }
                index++;
            }
            foreach (var item in mapper.Values)
            {
                if (!item.Optional && !item.IsSet)
                    throw new ParserException($"缺少必须的参数：参数名={item.TagName}");
            }
            
            return result;
        }

        private static object ConvertData(object value, Type targetType)
        {
            var underType = Nullable.GetUnderlyingType(targetType);
            if (underType != null && value == null)
            {
                return null;
            }
            var typeCode = Type.GetTypeCode(underType ?? targetType);

            switch (typeCode)
            {
                case TypeCode.Int16:
                    return Convert.ToInt16(value);
                case TypeCode.Int32:
                    return Convert.ToInt32(value);
                case TypeCode.Int64:
                    return Convert.ToInt64(value);
                case TypeCode.Byte:
                    return Convert.ToByte(value);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(value);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(value);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value);
                case TypeCode.SByte:
                    return Convert.ToSByte(value);
                case TypeCode.Single:
                    return Convert.ToSingle(value);
                case TypeCode.Double:
                    return Convert.ToDouble(value);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(value);
                case TypeCode.Boolean:
                    return Convert.ToBoolean(value);
                case TypeCode.DateTime:
                    return Convert.ToDateTime(value);
                case TypeCode.String:
                    return Convert.ToString(value);
                case TypeCode.Char:
                    return Convert.ToChar(value);
            }
            return value;
        }
    }

    public static class FxExtensions
    {
        public static T FirstOrDefault<T>(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Length == 0)
                return default(T);

            return array[0];
        }
    }
}
