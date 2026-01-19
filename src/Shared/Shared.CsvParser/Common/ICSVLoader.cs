using System;
using System.Collections;

/// <summary>
/// Author : 
/// </summary>
namespace SampleGame
{
    public interface ICSVLoader
    {
        void LoadPath(string path, string bundleName, bool async, bool resource, Action loadedCallback, bool isBundle = true);
        IEnumerator LoadPathAsync(string path, bool async, Action loadedCallback);
    }

    public class DataParserException : Exception
    {
        public DataParserException(string message)
            : base(message)
        {
        }
    }
}
