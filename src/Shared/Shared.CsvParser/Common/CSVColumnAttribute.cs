using System;
using System.Reflection;
using System.Collections.Generic;

namespace Shared.CsvParser
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CSVColumnAttribute : Attribute
    {
        public string _name;
        public FieldInfo _fieldInfo;
        public PropertyInfo _propertyInfo;
        public Func<string, CSVColumnAttribute, object> _Parser;
        private string _variableType;
        public bool _primaryKey { get; private set; }

        public bool _bitEnum { get; private set; }
        public string _columnType;
        public Dictionary<string, int> _bitEnumNameList = null;
        
        public string _excludeBracket;
        public bool IsAllowToEmpty { get; private set; }

        public double OffsetToUtc { get; private set; }

        public string VariableType
        {
            get
            {
                if (_variableType == string.Empty)
                    return _fieldInfo.FieldType.Name;
                return _variableType;
            }
        }

        public CSVColumnAttribute(string name, bool primaryKey = false, bool bitEnum = false, string variableType = "",
            string excludeBracket = null, bool isAllowToEmpty = false, int offsetToUtc = 0)
        {
            _name = name;
            _primaryKey = primaryKey;
            _variableType = variableType;
            _bitEnum = bitEnum;
            _excludeBracket = excludeBracket;
            IsAllowToEmpty = isAllowToEmpty;
            OffsetToUtc = offsetToUtc;
        }
    }
}