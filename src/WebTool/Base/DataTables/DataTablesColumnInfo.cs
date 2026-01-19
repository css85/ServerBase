namespace WebTool.Base.DataTables
{
    public class DataTablesColumnInfo
    {
        public string Name;
        public string PropertyName;

        public DataTablesColumnInfo(string name)
        {
            Name = name;
            PropertyName = null;
        }

        public DataTablesColumnInfo(string name, string propertyName)
        {
            Name = name;
            PropertyName = propertyName;
        }
    }
}
