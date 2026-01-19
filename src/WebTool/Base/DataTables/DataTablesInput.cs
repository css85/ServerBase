using System.Linq;
using Microsoft.AspNetCore.Http;
using WebTool.Extensions;

// ReSharper disable InconsistentNaming

namespace WebTool.Base.DataTables
{
    public class DataTablesInputOrder
    {
        public int index;
        public DataTablesOrderType dir;
    }

    public class DataTablesInputColumn
    {
        public int index;
        public string data;
        public string name;
        public bool searchable;
        public bool orderable;
        public string searchValue;
        public string searchRegex;
    }

    public class DataTablesInput
    {
        public DataTablesColumnInfo[] columnInfos;

        public int draw;
        public int start;
        public int length;
        public string[] searchValues;
        public string searchRegex;
        public DataTablesInputOrder[] orders;
        public DataTablesInputColumn[] columns;

        public DataTablesInput(IFormCollection form, DataTablesColumnInfo[] columnInfos)
        {
            this.columnInfos = columnInfos;

            draw = form.GetFormValue<int>("draw");
            start = form.GetFormValue<int>("start");
            length = form.GetFormValue<int>("length");
            searchValues =
                form.GetFormValue<string>("search[value]")?.Split(",").Where(p => string.IsNullOrEmpty(p) == false)
                    .ToArray() ?? new string[0];
            searchRegex = form.GetFormValue<string>("search[regex]");

            orders = new DataTablesInputOrder[this.columnInfos.Length];
            columns = new DataTablesInputColumn[this.columnInfos.Length];
            for (var i = 0; i < columns.Length; i++)
            {
                columns[i] = new DataTablesInputColumn
                {
                    index = i,
                };

                columns[i].data = form.GetFormValue<string>($"columns[{i}][data]");
                columns[i].name = form.GetFormValue<string>($"columns[{i}][name]");
                columns[i].searchable = form.GetFormValue<bool>($"columns[{i}][searchable]");
                if (columns[i].searchable)
                {
                    columns[i].searchValue = form.GetFormValue<string>($"columns[{i}][search][value]");
                    columns[i].searchRegex = form.GetFormValue<string>($"columns[{i}][search][regex]");
                }

                columns[i].orderable = form.GetFormValue<bool>($"columns[{i}][orderable]");
            }

            for (var i = 0; i < orders.Length; i++)
            {
                if (columns[i].orderable)
                {
                    if (form.TryGetFormValue($"order[{i}][column]", out int orderColumn) == false)
                        continue;
                    if (form.TryGetFormValue($"order[{i}][dir]", out string orderDirection) == false)
                        continue;

                    orders[i] = new DataTablesInputOrder
                    {
                        index = orderColumn,
                        dir = orderDirection switch
                        {
                            "asc" => DataTablesOrderType.Asc,
                            "desc" => DataTablesOrderType.Desc,
                            _ => DataTablesOrderType.None
                        }
                    };
                }
            }

            orders = orders.Where(p => p != null).ToArray();
        }
    }
}
