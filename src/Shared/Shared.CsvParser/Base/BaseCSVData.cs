using Shared.ServerApp.Services;

namespace Shared.CsvParser
{
    public abstract class BaseCSVData
    {
        public int LineNumber { get; private set; }
        public string KeyString { get; private set; }

        public abstract string GetFileName();

        internal void SetLineNumber(int lineNumber)
        {
            LineNumber = lineNumber;
        }

        internal void SetKeyString(string keyString)
        {
            KeyString = keyString;
        }

        public virtual void Init()
        {
        }

        public virtual void InitAfter(CsvStoreContextData csvData)
        {
        }

        public virtual bool CheckValidationAfter(CsvStoreContextData csvData, out string message)
        {
            message = "";
            return true;
        }
    }
}