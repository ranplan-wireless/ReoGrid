using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MoreLinq;
using unvell.ReoGrid.CellTypes;
using unvell.ReoGrid.Data;
using unvell.ReoGrid.DataFormat;

namespace unvell.ReoGrid.Demo
{
    public partial class ReoGridWrapperForm : Form
    {
        private IEnumerable<DataItem> _data = new List<DataItem>
        {
            new DataItem {Name = "Name A", Value = 0.62f, IsSelected = true, Type = Types.A},
            new DataItem {Name = "Name B", Value = 0.75f, IsSelected = true, Type = Types.B},
            new DataItem {Name = "Name B", Value = 0.85f, IsSelected = false, Type = Types.B},
            new DataItem {Name = "Name B", Value = 0.65f, IsSelected = false, Type = Types.B},
            new DataItem {Name = "Name B", Value = 0.55f, IsSelected = true, Type = Types.B},
            new DataItem {Name = "Name C", Value = 12.56f, IsSelected = true, Type = Types.C},
            new DataItem {Name = "Name D", Value = -10.37f, IsSelected = true, Type = Types.D},
            new DataItem {Name = "Name E", Value = 10.32f, IsSelected = true, Type = Types.E},
            new DataItem {Name = "Name F", Value = 6.71f, IsSelected = true, Type = Types.F},
            new DataItem {Name = "Name G", Value = -7.58f, IsSelected = true, Type = Types.G},
        };

        private AutoColumnFilter _selectionFilter;
        private ReoGridControl _table;

        public ReoGridWrapperForm()
        {
            InitializeComponent();

            _table = new ReoGridControl {Dock = DockStyle.Fill};
            _table.SetSettings(WorkbookSettings.View_ShowSheetTabControl, false);
            _table.CurrentWorksheet.DisableSettings(WorksheetSettings.View_AllowCellTextOverflow);

            panel2.Controls.Add(_table);
            _table.Dock = DockStyle.Fill;

            LoadData(_table.Worksheets.First());
            _selectionFilter = _table.Worksheets.First().CreateColumnFilter(RangePosition.EntireRange);
        }

        private void LoadData(Worksheet worksheet)
        {
            var j = 0;
            worksheet.DeleteRows(0, worksheet.ColumnCount);

            _data.ForEach((item, i) =>
            {
                worksheet.AppendRows(1);
                worksheet[i, 0] = item.Name;
                worksheet[i, 1] = item.IsSelected;
                //worksheet[i, 1] = new CheckBoxCell();
                worksheet[i, 2] = item.Type;

                if (i == 2)
                    worksheet.Cells[i, 2].IsReadOnly = true;

                //if (i % 2 == 0)
                    worksheet[i, 2] = new DropdownListCell(Enum.GetNames(typeof(Types)).Except(new []{item.Type.ToString()}));
                worksheet[i, 3] = item.Value;
                j = i;
            });
            //worksheet[3, 0] = new string[worksheet.ColumnCount];
            worksheet[j++, 0] = Enumerable.Repeat(string.Empty, worksheet.ColumnCount).ToArray();
            // worksheet[3, 1] = string.Empty;
            // worksheet[3, 1] = new CheckBoxCell();
            // worksheet[3, 2] = string.Empty;
            // worksheet[3, 2] = new DropdownListCell(Enum.GetNames(typeof(Types)));
            // worksheet[3, 3] = string.Empty;

            worksheet.SetCols(4);
            worksheet.SetRows(j);

            var numberFormatArgs = new NumberDataFormatter.NumberFormatArgs {DecimalPlaces = 2};
            worksheet.SetRangeDataFormat(new RangePosition(0, 3, -1, 1), CellDataFormatFlag.Number, numberFormatArgs);
        }

        private class DataItem
        {
            public string Name { get; set; }
            public bool IsSelected { get; set; }
            public float Value { get; set; }
            public Types Type { get; set; }
        }

        private enum Types
        {
            A,
            B,
            C,
            D,
            E,
            F,
            G
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            var worksheet = _table.Worksheets.First();
            var elderCount = worksheet.RowCount;
            worksheet.AppendRows(1);
            worksheet[elderCount, 0] = Enumerable.Repeat(string.Empty, worksheet.ColumnCount).ToArray();
            _selectionFilter.RefreshApplyRange(RangePosition.EntireRange);
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            _table.Worksheets.First().Resize(0, 0);
            LoadData(_table.Worksheets.First());
        }

        /*private void cb_Selected_CheckedChanged(object sender, System.EventArgs e)
        {
            var isSelected = sender as CheckBox;

            if (isSelected.Checked)
            {
                _selectionFilter.Columns[1].SelectedTextItems.Clear();
                _selectionFilter.Columns[1].SelectedTextItems.Add("True");
                var body = _selectionFilter.Columns[1];
                _selectionFilter.Apply();
            }
            else
            {
                _selectionFilter.Columns[1].IsSelectAll = true;
                _selectionFilter.Apply();
            }
        }

        private void llb_ClearFilter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _selectionFilter.Columns.ForEach(filter =>
            {
                filter.SelectedTextItems.Clear();
                filter.SelectedTextItems.AddRange(filter.GetDistinctItems());
                filter.IsSelectAll = true;
                filter.OnDataChange(0, 0);
            });
            _selectionFilter.Apply();
        }*/
    }
}