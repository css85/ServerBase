using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Shared.CsvParser;

namespace SampleGame
{
    public abstract class CSVMapperBase<T> : CSVParser where T : BaseCSVData, new()
    {
        private int _realDataLineCount = 3;
        public delegate void DataMappingCallback(T a);
        public DataMappingCallback Callback { get; set; }

        public CSVColumnAttribute PrimaryKey { get; protected set; }
        private CSVColumnAttribute[] _column;

        public int LineCount { get; protected set; } = 0;

        public int ElementCount { get { return this.LineCount - _realDataLineCount; } }

        private static CultureInfo _cultureInfo = new CultureInfo("en-US");

        protected CSVMapperBase(string csvFilePath) : base(typeof(T),csvFilePath)
        {
            LineCount = 0;
            callback = Update;
        }

        bool Update(List<string> data, int length)
        {

            if( LineCount == 2 )
            {
                if (PrimaryKey == null && _column == null)
                {
                    InitializeColumn(data);
                }
            }
            else
            {
                if (LineCount >= _realDataLineCount)
                    Parsing(LineCount, data, length);
            }
            //// lineCount = 0, ColumnName Setting
            //if (PrimaryKey == null && _column == null)
            //{
            //    InitializeColumn(data);
            //}
            //// lineCount = 1, ColumnTypeB
            //else if (LineCount == 1)
            //{
            //    ColumnType(data);
            //}
            //// Column Data
            //else
            //{
            //    if (LineCount >= _realDataLineCount)
            //        Parsing(LineCount, data, length);
            //}
            LineCount++;
            return true;
        }

        private void InitializeColumn(List<string> data)
        {
            _column = new CSVColumnAttribute[data.Count];

            foreach (FieldInfo field in
                CsvMapperType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.IsDefined(typeof(CSVColumnAttribute), true)))
            {
                CSVColumnAttribute attr = field.GetCustomAttributes(typeof(CSVColumnAttribute), true)[0] as CSVColumnAttribute;
                attr._fieldInfo = field;

                attr._Parser = GetParser(attr, attr._fieldInfo.FieldType);

                if (attr._primaryKey)
                {
                    if (PrimaryKey == null)
                    {
                        PrimaryKey = attr;
                    }
                    else
                    {
                        // Error 처리
                    }
                }

                bool isNotEqual = false;
                for (int i = 0; i < data.Count; i++)
                {
                    if (string.Compare(attr._name, data[i]) == 0)
                    {
                        _column[i] = attr;
                        isNotEqual = true;
                        break;
                    }
                }

                if (isNotEqual == false)
                {
                    throw new DataParserException(ExceptionMessage($"Not Match FieldInfo | {field.ReflectedType.Name}:{attr._name}"));
                }
            }

            foreach (PropertyInfo property in
                CsvMapperType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.IsDefined(typeof(CSVColumnAttribute), true)))
            {
                CSVColumnAttribute attr = property.GetCustomAttributes(typeof(CSVColumnAttribute), true)[0] as CSVColumnAttribute;
                attr._propertyInfo = property;
                attr._Parser = GetParser(attr, attr._propertyInfo.PropertyType);

                if (attr._primaryKey)
                {
                    if (PrimaryKey == null)
                    {
                        PrimaryKey = attr;
                    }
                    else
                    {
                        // Error 처리
                    }
                }

                bool isNotEqual = false;
                for (int i = 0; i < data.Count; i++)
                {
                    if (string.Compare(attr._name, data[i]) == 0)
                    {
                        _column[i] = attr;
                        isNotEqual = true;
                        break;
                    }
                }
                
                if(isNotEqual == false)
                {
                    throw new DataParserException(ExceptionMessage($"Not Math PropertyInfo | {property.ReflectedType.Name}:{attr._name}"));
                }
            }
        }
        
        
        protected void ColumnType(List<string> columnTypeData)
        {
            if (_column.Length != columnTypeData.Count)
            {
                // Error 
                throw new DataParserException(ExceptionMessage("Not Math CSVColumn Count"));
            }

            for (int i = 0; i < _column.Length; ++i)
            {
                if (_column[i] == null) continue;

                _column[i]._columnType = columnTypeData[i];
                if (_column[i]._bitEnum == true)
                {
                    string enumRemoveData = columnTypeData[i].Replace("Enum(", "");
                    enumRemoveData = enumRemoveData.Replace(")", "");

                    string[] keyDataList = enumRemoveData.Split(',');

                    _column[i]._bitEnumNameList = new Dictionary<string, int>();
                    for (int keyIndex = 0; keyIndex < keyDataList.Length; ++keyIndex)
                    {
                        string key = keyDataList[keyIndex];
                        if (_column[i]._bitEnumNameList.ContainsKey(key) == true)
                        {
                            // Error
                            throw new DataParserException(ExceptionMessage($"Duplicate Key Enum | {key}"));
                        }
                        _column[i]._bitEnumNameList.Add(key, 1 << keyIndex);
                    }

                }
            }
        }
        protected virtual void Parsing(int lineNumber, List<string> list, int length)
        {
            var obj = new T();

            for (int i = 0, len = _column.Length; i < len; i++)
            {
                CSVColumnAttribute col = _column[i];
                if (col != null && i < list.Count)
                {
                    if(col.IsAllowToEmpty == false)
                    {
                        if(string.IsNullOrEmpty(list[i]) == true)
                            throw new DataParserException(ExceptionMessage($"Empty Column | {lineNumber}:{col._name}"));
                    }

                    try
                    {
                        if (col._fieldInfo != null)
                            col._fieldInfo.SetValue(obj, col._Parser(list[i], col));
                        else if (col._propertyInfo != null)
                            col._propertyInfo.SetValue(obj, col._Parser(list[i], col), null);
                    }
                    catch (Exception)
                    {
                        throw new DataParserException(ExceptionMessage($"Inner exception | {lineNumber}:{col._name}"));
                    }
                }
            }

            obj.SetLineNumber(lineNumber);

            if (Callback == null) return;

            Callback(obj);
        }
        protected virtual Func<string, CSVColumnAttribute, object> GetParser(CSVColumnAttribute attr, System.Type fieldType = null, bool isCustomParser = true)
        {
            if (isCustomParser == true && attr != null && attr._bitEnum == true)
                return ParserBitEnum;

            if (fieldType == typeof(bool))
                return ParserBool;
            else if (fieldType == typeof(byte))
                return ParserByte;
            else if (fieldType == typeof(short))
                return ParserShort;
            else if (fieldType == typeof(ushort))
                return ParserUShort;
            else if (fieldType == typeof(int))
                return ParserInt;
            else if (fieldType == typeof(uint))
                return ParserUInt;
            else if (fieldType == typeof(long))
                return ParserLong;
            else if (fieldType == typeof(ulong))
                return ParserULong;
            else if (fieldType == typeof(float))
                return ParserFloat;
            else if (fieldType == typeof(double))
                return ParserDouble;
            else if (fieldType == typeof(string))
                return ParserString;
            else if (fieldType == typeof(decimal))
                return ParserDecimal;
            else if (fieldType.IsEnum)
                return ParserEnum;
            else if (fieldType == typeof(string[]))
                return ParserStringArray;
            else if (fieldType == typeof(int[]))
                return ParserIntArray;
            else if (fieldType == typeof(uint[]))
                return ParserUIntArray;
            else if (fieldType == typeof(float[]))
                return ParserFloatArray;
            else if (fieldType == typeof(long[]))
                return ParserLongArray;
            else if (fieldType == typeof(TimeSpan))
                return ParserTimeSpan;
            else if (fieldType == typeof(DateTime))
                return ParserDateTime;
            else if (fieldType == typeof(BigInteger))
                return ParserBigInteger;
            else if (fieldType.IsArray && fieldType.GetElementType()?.IsEnum == true)
                return ParserEnumArray;
            return null;
        }

        #region ParsingMethods

        private object ParserBool(string value, CSVColumnAttribute attr)
        {
            return value.ToUpper(_cultureInfo) == "TRUE" || value == "1";
        }

        private object ParserByte(string value, CSVColumnAttribute attr)
        {
            byte returnValue = default(byte);
            bool tryResult = byte.TryParse(value, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserShort(string value, CSVColumnAttribute attr)
        {
            short returnValue = default(short);
            bool tryResult = short.TryParse(value, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserUShort(string value, CSVColumnAttribute attr)
        {
            ushort returnValue = default(ushort);
            bool tryResult = ushort.TryParse(value, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserInt(string value, CSVColumnAttribute attr)
        {
            int returnValue = default(int);
            bool tryResult = int.TryParse(value, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserUInt(string value, CSVColumnAttribute attr)
        {
            uint returnValue = default(uint);
            bool tryResult = uint.TryParse(value, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserLong(string value, CSVColumnAttribute attr)
        {
            long returnValue = default(long);
            bool tryResult = long.TryParse(value, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserULong(string value, CSVColumnAttribute attr)
        {
            ulong returnValue = default(ulong);
            bool tryResult = ulong.TryParse(value, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserFloat(string value, CSVColumnAttribute attr)
        {
            float returnValue = default(float);
            bool tryResult = float.TryParse(value, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserDouble(string value, CSVColumnAttribute attr)
        {
            double returnValue = default(double);
            bool tryResult = double.TryParse(value, System.Globalization.NumberStyles.Float, null, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserDecimal(string value, CSVColumnAttribute attr)
        {
            decimal returnValue = default(decimal);
            bool tryResult = decimal.TryParse(value, System.Globalization.NumberStyles.Float, null, out returnValue);
#if ENABLE_LOG
            // throw
            if (!tryResult)
            {
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            return returnValue;
        }

        private object ParserString(string value, CSVColumnAttribute attr)
        {
            return value;
        }

        private object ParserEnum(string value, CSVColumnAttribute attr)
        {
            Type enumType = null;
            if (attr._fieldInfo != null)
                enumType = attr._fieldInfo.FieldType;
            else if (attr._propertyInfo != null)
                enumType = attr._propertyInfo.PropertyType;
            else 
                return value;

            if (Enum.TryParse(enumType, value, out var result))
            {
                return result;
            }

            return Enum.Parse(enumType, "0");
        }

        private object ParserEnumArray(string value, CSVColumnAttribute attr)
        {
            Type enumType = null;
            if (attr._fieldInfo != null)
                enumType = attr._fieldInfo.FieldType.GetElementType();
            else if (attr._propertyInfo != null)
                enumType = attr._propertyInfo.PropertyType.GetElementType();
            else 
                return null;

            if (enumType == null)
                return null;

            if (string.IsNullOrEmpty(value))
                return Array.CreateInstance(enumType, 0);

            if (attr._excludeBracket != null)
                value = value.Trim(attr._excludeBracket.ToCharArray());


            string[] temps = value.Split(',');
            var values = Array.CreateInstance(enumType, temps.Length);
            for (int i = 0; i < temps.Length; i++)
            {
                if (!Enum.IsDefined(enumType, temps[i]) && !attr._bitEnum)
                {
#if ENABLE_LOG
                // throw
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
//#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
//#else
//                throw new Exception(errorMsg);
//#endif
#endif
                    // 기본값 0으로 픽스
                    temps[i] = "0";
                
                    // 만약 enum의 첫번째 타입으로 파싱하고싶다면.. (0이 아닐경우)
                    //var enumValues = enumType.GetEnumValues();
                    //if (enumValues.Length > 0)
                    //    temps[i] = enumValues.GetValue(0).ToString();
                }

                values.SetValue(Enum.Parse(enumType, temps[i]), i);
            }
            return values;
        }

        private object ParserStringArray(string value, CSVColumnAttribute attr)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<string>();

            if (attr._excludeBracket != null)
                value = value.Trim(attr._excludeBracket.ToCharArray());

            return value.Split(',');
        }

        private object ParserFloatArray(string value, CSVColumnAttribute attr)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<float>();

            if (attr._excludeBracket !=null)
                value = value.Trim(attr._excludeBracket.ToCharArray());

            string[] temps = value.Split(',');
            float[] values = new float[temps.Length];
            for (int i = 0; i < temps.Length; i++)
            {
                float resultValue = default(float);
                bool tryResult = float.TryParse(temps[i], out resultValue);
#if ENABLE_LOG
                if (tryResult == false)
                {
                    // throw
                    string errorMsg = "Parser Error! " + attr._name + " : " + value + " / error Value : " + temps[i] + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                    CommonDebug.LogError(errorMsg);
#else
                    throw new Exception(errorMsg);
#endif
                }
#endif
                values[i] = resultValue;
                //values[i] = float.Parse(temps[i]);
            }
            return values;
        }

        private object ParserLongArray(string value, CSVColumnAttribute attr)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<long>();

            if (attr._excludeBracket != null)
                value = value.Trim(attr._excludeBracket.ToCharArray());

            string[] temps = value.Split(',');
            long[] values = new long[temps.Length];
            for (int i = 0; i < temps.Length; i++)
            {
                long resultValue = default(long);
                bool tryResult = long.TryParse(temps[i], out resultValue);
#if ENABLE_LOG
                if (tryResult == false)
                {
                    // throw
                    string errorMsg = "Parser Error! " + attr._name + " : " + value + " / error Value : " + temps[i] + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                    CommonDebug.LogError(errorMsg);
#else
                    throw new Exception(errorMsg);
#endif
                }
#endif
                values[i] = resultValue;
                //values[i] = float.Parse(temps[i]);
            }
            return values;
        }

        private object ParserIntArray(string value, CSVColumnAttribute attr)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<int>();

            if (attr._excludeBracket != null)
                value = value.Trim(attr._excludeBracket.ToCharArray());

            string[] temps = value.Split(',');
            int[] values = new int[temps.Length];
            for (int i = 0; i < temps.Length; i++)
            {
                int resultValue = default(int);
                bool tryResult = int.TryParse(temps[i], out resultValue);
#if ENABLE_LOG
                if (tryResult == false)
                {
                    // throw
                    string errorMsg = "Parser Error! " + attr._name + " : " + value + " / error Value : " + temps[i] + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                    CommonDebug.LogError(errorMsg);
#else
                    throw new Exception(errorMsg);
#endif
                }
#endif
                values[i] = resultValue;
                //values[i] = int.Parse(temps[i]);
            }
            return values;
        }

        private object ParserUIntArray(string value, CSVColumnAttribute attr)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<uint>();

            if (attr._excludeBracket != null)
                value = value.Trim(attr._excludeBracket.ToCharArray());

            string[] temps = value.Split(',');
            uint[] values = new uint[temps.Length];
            for (int i = 0; i < temps.Length; i++)
            {
                uint resultValue = default(uint);
                bool tryResult = uint.TryParse(temps[i], out resultValue);
#if ENABLE_LOG
                if (tryResult == false)
                {
                    // throw
                    string errorMsg =
 "Parser Error! " + attr._name + " : " + value + " / error Value : " + temps[i] + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                    throw new Exception(errorMsg);
#endif
                }
#endif
                values[i] = resultValue;
                //values[i] = uint.Parse(temps[i]);
            }

            return values;
        }

        private object ParserTimeSpan(string value, CSVColumnAttribute attr)
        {
            TimeSpan returnValue = default(TimeSpan);

            bool tryResult = TimeSpan.TryParse(value, out returnValue);
            return returnValue;

#if ENABLE_LOG
            if (tryResult == false)
            {
                // throw
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            //return returnValue;
        }

        private object ParserBigInteger(string value, CSVColumnAttribute attr)
        {
            BigInteger returnValue = default(BigInteger);

            //            bool tryResult = BigInteger.TryParse(value, out returnValue);
            bool tryResult = BigInteger.TryParse(value, System.Globalization.NumberStyles.Number | NumberStyles.AllowExponent, null, out returnValue);

            return returnValue;

#if ENABLE_LOG
            if (tryResult == false)
            {
                // throw
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
            //return returnValue;
        }

        private object ParserDateTime(string value, CSVColumnAttribute attr)
        {
            DateTime.TryParse(value, _cultureInfo, DateTimeStyles.AdjustToUniversal, out var returnValue);
            return DateTime.SpecifyKind(returnValue.AddHours(attr.OffsetToUtc), DateTimeKind.Utc);

#if ENABLE_LOG
            if (tryResult == false)
            {
                // throw
                string errorMsg = "Parser Error! " + attr._name + " : " + value + "\nPath : " + FilePath;
#if ENABLE_LOG_CSVPARSER
                CommonDebug.LogError(errorMsg);
#else
                throw new Exception(errorMsg);
#endif
            }
#endif
                //return returnValue;
        }

        private object ParserBitEnum(string value, CSVColumnAttribute attr)
        {
            Func<string, CSVColumnAttribute, object> parser = null;
            if (attr._fieldInfo != null)
                parser = GetParser(attr, attr._fieldInfo.FieldType, false);
            else if (attr._propertyInfo != null)
                parser = GetParser(attr, attr._propertyInfo.PropertyType, false);

            if (parser == null)
            {
                // Error!
                throw new DataParserException(ExceptionMessage($"Not Found Parser | {attr._name}"));
            }

            int resultBit = 0;
            string[] keyList = value.Split(',');
            for (int i = 0; i < keyList.Length; ++i)
            {
                string enumKey = keyList[i];
                if (attr._bitEnumNameList.ContainsKey(enumKey) != true)
                {
                    throw new DataParserException(ExceptionMessage($"Not Register Enum Key | {enumKey}"));
                }

                int intBit = attr._bitEnumNameList[enumKey];
                resultBit = resultBit | intBit;
            }

            return parser(resultBit.ToString(), attr);
        }
        #endregion
    }
}
