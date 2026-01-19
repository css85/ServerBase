using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.CsvParser;

namespace SampleGame
{
    public static class CSVUtility
    {
        public static List<T> Load<T>(string filePath, Action<T> after = null)
            where T : BaseCSVData, new()
        {
            var list = LoadList<T>(filePath, "", false, false, null);
            foreach (var item in list)
            {
                after?.Invoke(item);
            }
            return list;
        }

        public static Dictionary<TK, T> Load<TK, T>(string filePath, Action<T> after = null)
        where T : BaseCSVData, new()
        {
            var map = LoadDictionary<TK, T>(filePath, "", false, false, null);
            foreach (var item in map.Values)
            {
                after?.Invoke(item);
            }

            return map;
        }

        private static Dictionary<TK, T> LoadDictionary<TK, T>(string filePath, string bundleName, bool async, bool resource, Action loadedCallback = null, bool isBundle = true)
            where T : BaseCSVData, new()
        {
            Dictionary<TK, T> dic = new Dictionary<TK, T>();

            CSVMapperBase<T> mapper = new CSVBasicMapper<T>(filePath);

            mapper.Callback = (obj) =>
            {
                if (obj == null)
                    return;

                TK key = default(TK);
                if (mapper.PrimaryKey._fieldInfo != null)
                    key = (TK)mapper.PrimaryKey._fieldInfo.GetValue(obj);
                else if (mapper.PrimaryKey._propertyInfo != null)
                    key = (TK)mapper.PrimaryKey._propertyInfo.GetValue(obj, null);

                obj.SetKeyString(key.ToString());

                dic.Add(key, obj);
            };

            mapper.LoadPath(filePath, bundleName, async, resource, loadedCallback, isBundle);

            if (mapper.ElementCount != dic.Count)
                throw new Exception("Couldn't read all csv data");

            return dic;
        }

        private static List<T> LoadList<T>(string filePath, string bundleName, bool async, bool resource, Action loadedCallback = null, bool isBundle = true)
            where T : BaseCSVData, new()
        {
            List<T> list = new List<T>();

            var mapper = new CSVBasicMapper<T>(filePath);

            mapper.Callback = (obj) =>
            {
                if (obj == null)
                    return;

                list.Add(obj);
            };
            mapper.LoadPath(filePath, bundleName, async, resource, loadedCallback, isBundle);

            if (mapper.ElementCount != list.Count)
                throw new Exception("Couldn't read all csv data");

            return list;
        }

        public static Dictionary<TK, T> LoadDictionary<TK, T>(string fileName, string dataString)
            where T : BaseCSVData, new()
        {
            try
            {
                Dictionary<TK, T> dic = new Dictionary<TK, T>();
                var mapper = new CSVBasicMapper<T>(fileName);

                mapper.Callback = (obj) =>
                {
                    if (obj == null)
                        return;

                    TK key = default(TK);
                    if (mapper.PrimaryKey._fieldInfo != null)
                        key = (TK)mapper.PrimaryKey._fieldInfo.GetValue(obj);
                    else if (mapper.PrimaryKey._propertyInfo != null)
                        key = (TK)mapper.PrimaryKey._propertyInfo.GetValue(obj, null);

                    obj.SetKeyString(key.ToString());

                    dic.Add(key, obj);
                };

                mapper.Load(dataString, null);
                if (mapper.ElementCount != dic.Count)
                    throw new Exception("Couldn't read all csv data");

                return dic;
            }
            catch
            {
                return null;
            }
        }

        public static List<T> LoadList<T>(string fileName, string dataString)
            where T : BaseCSVData, new()
        {
            List<T> list = new List<T>();
            var mapper = new CSVBasicMapper<T>(fileName);

            mapper.Callback = (obj) =>
            {
                if (obj == null)
                    return;

                list.Add(obj);
            };

            mapper.Load(dataString, null);

            if (mapper.ElementCount != list.Count)
                throw new Exception("Couldn't read all csv data");

            return list;
        }
    }
}

