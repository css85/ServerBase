using System;
using System.Collections.Generic;

namespace SampleGame
{
    public delegate bool CSVParserCallback(List<string> data, int length);
    public abstract class CSVLoaderBase
    {
        protected Type CsvMapperType { get; set; }
        protected string FilePath { get; set; }
        protected CSVParserCallback callback { get; set; }
        protected List<string> _lineList { get; set; }
        protected bool Break { get; set; }

        #region CollectLineData
        protected int CollectLineData(string line)
        {
            int startIndex = 0;
            char minValue = char.MinValue;

            for (int i = 0; i < line.Length; ++i)
            {
                if (line[i] == '"' || line[i] == '\'')
                {
                    if (startIndex == i)
                    {
                        minValue = line[i];
                    }
                    else if (i == line.Length - 1)
                    {
                        if (minValue == 0)
                        {
                            string str = line.Substring(startIndex, i - startIndex + 1);
                            _lineList.Add(str);
                        }
                        else if (line[i] == minValue)
                        {
                            string str = line.Substring(startIndex + 1, i - startIndex - 1);
                            _lineList.Add(str);
                            minValue = char.MinValue;
                        }
                        else
                            continue;
                        startIndex = i + 1;
                    }
                }
                else if (line[i] == ',')
                {
                    if (minValue == 0)
                    {
                        string str = line.Substring(startIndex, i - startIndex);
                        _lineList.Add(str);
                    }
                    else if (line[i - 1] == minValue)
                    {
                        string str = line.Substring(startIndex + 1, i - startIndex - 2);
                        _lineList.Add(str);
                        minValue = char.MinValue;
                    }
                    else
                        continue;
                    startIndex = i + 1;
                }
                else if (i == line.Length - 1)
                {
                    if (minValue == 0)
                    {
                        string str = line.Substring(startIndex, i - startIndex + 1);
                        _lineList.Add(str);
                    }
                    else if (line[i - 1] == minValue)
                    {
                        string str = line.Substring(startIndex + 1, i - startIndex);
                        _lineList.Add(str);
                        minValue = char.MinValue;
                    }
                    else
                        continue;
                    startIndex = i + 1;
                }
                /*
                else if (i == line.Length - 2)
                {
                    if (minValue == 0)
                    {
                        string str = line.Substring(startIndex, i + 1 - startIndex);
                        _lineList.Add(str);
                    }
                    else if (line[i - 1] == minValue)
                    {
                        string str = line.Substring(startIndex + 1, i + 1 - startIndex - 2);
                        _lineList.Add(str);
                        minValue = char.MinValue;
                    }
                    else
                        continue;
                    startIndex = i + 1;
                }
                */
            }

            // 마지막 값 체크.. 
            int lastIndex = line.Length - 1;
            if(0 <= lastIndex)
            {
                if(line[lastIndex] == ',')
                {
                    //UnityEngine.Debug.Log("마지막 데이터 없음!");
                    _lineList.Add("");
                }
            }

            return _lineList.Count;
        }
        #endregion

        #region Load
        public void Load(string text, System.Action loadedCallback)
        {
            if (text == null)
                return;

            _lineList = new List<string>(1024);

            string str = text;
            char[] chArray = new char[2] { '\r', '\n' };
            string[] data = str.Split(chArray);

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Length > 0)
                {
                    _lineList.Clear();

#if UNITY_EDITOR
                    //Debug.Log(i.ToString() + " : " +data[i]);
#endif

                    if (callback != null)
                        callback(_lineList, CollectLineData(data[i]));
                }
            }

            if (loadedCallback != null)
                loadedCallback();
        }
        #endregion
    }
}