// ReSharper disable InconsistentNaming

namespace WebTool.Base.DataTables
{
    public class DataTablesOutput
    {
        public int draw;
        public int recordsTotal;
        public int recordsFiltered;
        public string[][] data;
        public string error;

        public DataTablesOutput(int draw)
        {
            this.draw = draw;
        }
    }
}
