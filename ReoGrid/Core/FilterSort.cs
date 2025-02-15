﻿/*****************************************************************************
 * 
 * ReoGrid - .NET Spreadsheet Control
 * 
 * https://reogrid.net/
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 *
 * Author: Jing Lu <jingwood at unvell.com>
 *
 * Copyright (c) 2012-2021 Jing Lu <jingwood at unvell.com>
 * Copyright (c) 2012-2016 unvell.com, all rights reserved.
 * 
 ****************************************************************************/

using System;
using System.Linq;
using unvell.ReoGrid.Core;
#if DEBUG
using System.Diagnostics;
#endif // DEBUG

using unvell.ReoGrid.Data;
using unvell.ReoGrid.Interaction;

namespace unvell.ReoGrid
{
	partial class Worksheet
	{
		#region Filter

		///// <summary>
		///// Create filter on specified column.
		///// </summary>
		///// <param name="column">Column code that locates a column to create filter.</param>
		///// <param name="titleRows">Indicates how many title rows exist at the top of worksheet,
		///// title rows will not be included in filter range.</param>
		///// <returns>Instance of column filter.</returns>
		//public AutoColumnFilter CreateColumnFilter(string column, int titleRows = 0,
		//	AutoColumnFilterUI columnFilterUI = AutoColumnFilterUI.DropdownButtonAndPane)
		//{
		//	return CreateColumnFilter(column, column, titleRows, columnFilterUI);
		//}

		/// <summary>
		/// Create column filter.
		/// </summary>
		/// <param name="startColumn">First column specified by an address to create filter.</param>
		/// <param name="endColumn">Last column specified by an address to the filter.</param>
		/// <param name="titleRows">Indicates that how many title rows exist at the top of spreadsheet,
		/// title rows will not be included in filter apply range.</param>
		/// <param name="columnFilterUI">Indicates whether allow to create graphics user interface (GUI), 
		/// by default the dropdown-button on the column and candidates dropdown-panel will be created.
		/// Set this argument as NoGUI to create filter without GUI.</param>
		/// <returns>Instance of column filter.</returns>
		public AutoColumnFilter CreateColumnFilter(string startColumn, string endColumn, int titleRows = 0,
			AutoColumnFilterUI columnFilterUI = AutoColumnFilterUI.DropdownButtonAndPanel)
		{
			int startIndex = RGUtility.GetNumberOfChar(startColumn);
			int endIndex = RGUtility.GetNumberOfChar(endColumn);

			return CreateColumnFilter(startIndex, endIndex, titleRows, columnFilterUI);
		}

		/// <summary>
		/// Create column filter.
		/// </summary>
		/// <param name="column">Column to create filter.</param>
		/// <param name="titleRows">indicates that how many title rows exist at the top of spreadsheet,
		/// title rows will not be included in filter apply range.</param>
		/// <param name="columnFilterUI">Indicates whether allow to create graphics user interface (GUI), 
		/// by default the dropdown-button on the column and candidates dropdown-panel will be created.
		/// Set this argument as NoGUI to create filter without GUI.</param>
		/// <returns>Instance of column filter.</returns>
		public AutoColumnFilter CreateColumnFilter(int column, int titleRows, AutoColumnFilterUI columnFilterUI)
		{
			return this.CreateColumnFilter(column, column, titleRows, columnFilterUI);
		}

		/// <summary>
		/// Create column filter.
		/// </summary>
		/// <param name="startColumn">first column specified by a zero-based number of column to create filter</param>
		/// <param name="endColumn">last column specified by a zero-based number of column to create filter</param>
		/// <param name="titleRows">indicates that how many title rows exist at the top of spreadsheet,
		/// title rows will not be included in filter apply range.</param>
		/// <param name="columnFilterUI">Indicates whether or not to show GUI for filter, 
		/// by default the drop-down button displayed on column header and a candidates list popuped up when dropdown-panel opened.
		/// Set this argument as NoGUI to create filter without GUI.</param>
		/// <returns>Instance of column filter.</returns>
		public AutoColumnFilter CreateColumnFilter(int startColumn, int endColumn, int titleRows = 0,
			AutoColumnFilterUI columnFilterUI = AutoColumnFilterUI.DropdownButtonAndPanel)
		{
			if (startColumn < 0 || startColumn >= this.ColumnCount)
			{
				throw new ArgumentOutOfRangeException("startColumn", "number of column start to filter out of valid spreadsheet range");
			}

			if (endColumn < startColumn)
			{
				throw new ArgumentOutOfRangeException("endColumn", "end column must be greater than start column");
			}

			if (endColumn >= this.ColumnCount)
			{
				throw new ArgumentOutOfRangeException("endColumn", "end column out of valid spreadsheet range");
			}

			return CreateColumnFilter(new RangePosition(titleRows, startColumn,
				this.MaxContentRow - titleRows + 1, endColumn - startColumn + 1), columnFilterUI);
		}

		/// <summary>
		/// Create automatic column filter and display on specified headers of worksheet.
		/// </summary>
		/// <param name="range">Range to filter data.</param>
		/// <param name="columnFilterUI">Indicates whether or not to show GUI for filter, 
		/// by default the drop-down button displayed on column header and a candidates list popuped up when dropdown-panel opened.
		/// Set this argument as NoGUI to create filter without GUI.</param>
		/// <returns>Instance of column filter.</returns>
		public AutoColumnFilter CreateColumnFilter(RangePosition range, 
			AutoColumnFilterUI columnFilterUI = AutoColumnFilterUI.DropdownButtonAndPanel)
		{
			var filter = new AutoColumnFilter(this, this.FixRange(range));

			filter.Attach(this, columnFilterUI);

			return filter;
		}

		/// <summary>
		/// Do a filter on specified rows. Determines whether or not to show or hide a row.
		/// </summary>
		/// <param name="startRow">Number of row start to check.</param>
		/// <param name="rows">Number of rows to be checked.</param>
		/// <param name="filter">A callback filter function to check specified row from worksheet.</param>
		public void DoFilter(RangePosition range, Func<int, bool> filter)
		{
			try
			{
				this.controlAdapter.ChangeCursor(CursorStyle.Busy);

				this.SetRowsHeight(range.Row, range.Rows, r =>
				{
					bool showRow = filter(r);

					if (showRow)
					{
						var rowhead = this.RetrieveRowHeader(r);

						// don't hide row, show the row
						// if row is hidden, return lastHeight to show the row
						return rowhead.InnerHeight == 0 ? rowhead.LastHeight : rowhead.InnerHeight;
					}
					else
					{
						return 0;
					}
				}, true);
			}
			finally
			{
				if (this.controlAdapter != null)
				{
					this.ControlAdapter.ChangeCursor(Interaction.CursorStyle.PlatformDefault);
				}
			}

			this.RowsFiltered?.Invoke(this, null);
		}

		/// <summary>
		/// Event raised when rows filtered on this worksheet.
		/// </summary>
		public event EventHandler RowsFiltered;

		#endregion // Filter

		#region Sort

        /// <summary>
        /// Sort data on specified column.
        /// </summary>
        /// <param name="columnAddress">Base column specified by an address to sort data.</param>
        /// <param name="order">Order of data sort.</param>
        /// <param name="cellDataComparer">Custom cell data comparer, compares two cells and returns an integer. 
        /// Set this value to null to use default built-in comparer.</param>
        /// <returns>Data changed range</returns>
        public RangePosition SortColumn(string columnAddress, SortOrder order = SortOrder.Ascending,
            CellElementFlag moveElementFlag = CellElementFlag.Data,
            Func<object, object, int> cellDataComparer = null)
        {
            return SortColumn(RGUtility.GetNumberOfChar(columnAddress), order, moveElementFlag, cellDataComparer);
        }

        /// <summary>
		/// Sort data on specified column.
		/// </summary>
		/// <param name="columnIndex">Zero-based number of column to sort data.</param>
		/// <param name="order">Order of data sort.</param>
		/// <param name="cellDataComparer">custom cell data comparer, compares two cells and returns an integer. 
		/// Set this value to null to use default built-in comparer.</param>
		/// <returns>Data changed range</returns>
		public RangePosition SortColumn(int columnIndex, SortOrder order = SortOrder.Ascending,
			CellElementFlag moveElementFlag = CellElementFlag.Data,
			Func<object, object, int> cellDataComparer = null)
		{
			return SortColumn(columnIndex, 0, MaxContentRow, 0, MaxContentCol, order, moveElementFlag, cellDataComparer);
		}

		/// <summary>
		/// Sort data on specified column.
		/// </summary>
		/// <param name="columnIndex">Zero-based number of column to sort data</param>
		/// <param name="titleRows">Indicates that how many title rows exist at the top of worksheet, 
		/// Title rows will not be included in sort apply range.</param>
		/// <param name="order">Order of data sort.</param>
		/// <param name="cellDataComparer">Custom cell data comparer, compares two cells and returns an integer.  
		/// Set this value to null to use default built-in comparer.</param>
		/// <returns>Data changed range.</returns>
		public RangePosition SortColumn(int columnIndex, int titleRows, SortOrder order = SortOrder.Ascending,
				CellElementFlag moveElementFlag = CellElementFlag.Data,
		Func<object, object, int> cellDataComparer = null)
		{
			return SortColumn(columnIndex, titleRows, MaxContentRow, 0, MaxContentCol, order, moveElementFlag, cellDataComparer);
		}

		/// <summary>
		/// Sort data on specified column.
		/// </summary>
		/// <param name="columnIndex">Zero-based number of column to sort data.</param>
		/// <param name="startRow">First number of row to allow sort data.</param>
		/// <param name="endRow">Last number of number of row to allow sort data.</param>
		/// <param name="startColumn">First number of column to allow sort data.</param>
		/// <param name="endColumn">Last number of column to allow sort data.</param>
		/// <param name="order">Order of data sort.</param>
		/// <param name="cellDataComparer">Custom cell data comparer, compares two cells and returns an integer. 
		/// Set this value to null to use default built-in comparer.</param>
		/// <returns>Data changed range.</returns>
		public RangePosition SortColumn(int columnIndex, int startRow, int endRow, int startColumn, int endColumn, 
			SortOrder order = SortOrder.Ascending,
			CellElementFlag moveElementFlag = CellElementFlag.Data,
			Func<object, object, int> cellDataComparer = null)
		{
			return SortColumn(columnIndex, new RangePosition(startRow, startColumn, MaxContentRow - startRow + 1,
				endColumn - startColumn + 1), order, moveElementFlag, cellDataComparer);
		}

		/// <summary>
		/// Sort data inside specified range.
		/// </summary>
		/// <param name="columnIndex">Data will be sorted based on this column.</param>
		/// <param name="applyRange">Affect range.</param>
		/// <param name="order">Order of data sort.</param>
		/// <param name="cellDataComparer"></param>
		/// <returns></returns>
		public RangePosition SortColumn(int columnIndex, string applyRange, SortOrder order = SortOrder.Ascending,
			CellElementFlag moveElementFlag = CellElementFlag.Data,
			Func<object, object, int> cellDataComparer = null)
		{
			if (RangePosition.IsValidAddress(applyRange))
			{
				return this.SortColumn(columnIndex, new RangePosition(applyRange), order, moveElementFlag, cellDataComparer);
			}
			else if (this.TryGetNamedRangePosition(applyRange, out var range))
			{
				return this.SortColumn(columnIndex, range, order, moveElementFlag, cellDataComparer);
			}
			else
				throw new InvalidAddressException(applyRange);
		}

		/// <summary>
		/// Sort data on specified column.
		/// </summary>
		/// <param name="columnIndex">Zero-based number of column to sort data.</param>
		/// <param name="applyRange">Data only be changed in this range during sort.</param>
		/// <param name="order">Order of data sort.</param>
		/// <param name="cellDataComparer">Custom cell data comparer, compares two cells and returns an integer. 
		/// Set this value to null to use default built-in comparer.</param>
		/// <returns>Data changed range.</returns>
		public RangePosition SortColumn(int columnIndex, RangePosition applyRange,
			SortOrder order = SortOrder.Ascending,
			CellElementFlag moveElementFlag = CellElementFlag.Data,
			Func<object, object, int> cellDataComparer = null)
		{
			var range = FixRange(applyRange);

			RangePosition affectRange = RangePosition.Empty;

			if (cellDataComparer != null)
			{
				this.cellDataComparer = cellDataComparer;
			}

#if DEBUG
			Stopwatch sw = Stopwatch.StartNew();
#endif // DEBUG

            // stop fire events
			this.SuspendDataChangedEvents();

			try
			{
				this.controlAdapter.ChangeCursor(CursorStyle.Busy);

				if (!this.CheckQuickSortRange(columnIndex, range.Row, range.EndRow, range.Col, range.EndCol, order, ref affectRange))
				{
					throw new InvalidOperationException("Cannot change a part of range, all cells should be having same colspan on column.");
				}

                int Compare(ISortedEntity data, ISortedEntity @base)
                {
                    return CompareCell(data, @base, order);
                }

				this.QuickSortColumn(columnIndex, range.Row, range.EndRow, range.Col, range.EndCol, order, ref affectRange,
					cellDataComparer == null ? (Func<ISortedEntity, ISortedEntity, int>)Compare : UserCellDataComparerAdapter);

                if (RetrieveColumnHeader(columnIndex).Body is AutoColumnFilter.AutoColumnFilterBody columnFilterBody)
                    columnFilterBody.autoFilter.Apply();
#if DEBUG
				sw.Stop();

				if (sw.ElapsedMilliseconds > 10)
				{
					Debug.WriteLine(string.Format("sort column by {0} on [{1}-{2}]: {3} ms", columnIndex,
						range.Col, range.EndCol, sw.ElapsedMilliseconds));
				}
#endif // DEBUG
			}
			finally
			{
				// resume to fire events
				this.ResumeDataChangedEvents();
			}

			this.RequestInvalidate();

			this.controlAdapter.ChangeCursor(CursorStyle.PlatformDefault);

			if (!affectRange.IsEmpty)
			{
				for (int c = affectRange.Col; c <= affectRange.EndCol; c++)
				{
					var header = this.cols[c];

					if (header.Body != null)
					{
						header.Body.OnDataChange(affectRange.Row, affectRange.EndRow);
					}
				}

				this.RaiseRangeDataChangedEvent(affectRange);

				this.RowsSorted?.Invoke(this, new Events.RangeEventArgs(affectRange));
			}

			return affectRange;
		}

		private bool CheckQuickSortRange(int columnIndex, int row, int endRow, int col, int endCol,
			SortOrder order, ref RangePosition affectRange)
		{
			for (int c = col; c <= endCol; c++)
			{
				var cell1 = this.cells[row, c];

				for (int r = row + 1; r <= endRow; r++)
				{
					var cell2 = this.cells[r, c];

					if (cell1 != null && cell2 != null
						&& ((cell1.IsValidCell && !cell2.IsValidCell)
						|| (!cell1.IsValidCell && cell2.IsValidCell)))
					{
						return false;
					}
				}
			}

			return true;
		}

		private void QuickSortRelocateRow(int row, int[] rowIndexes, int startColumn, int endColumn, int excludeColumn)
		{
			int targetIndex = rowIndexes[row];
			//int targetTargetIndex = rowIndexes[targetIndex];

			QuickSortSwapRow(row, targetIndex, rowIndexes, startColumn, endColumn, excludeColumn);

			int index = rowIndexes[row];
			rowIndexes[row] = rowIndexes[targetIndex];
			rowIndexes[targetIndex] = index;
		}

		private void QuickSortSwapRow(int row1, int row2, int[] rowIndexes, int startColumn, int endColumn, int excludeColumn)
		{
			for (int col = startColumn; col <= endColumn; col++)
			{
				if (col == excludeColumn) continue;

				var cell1 = this.cells[row1, col];
				var cell2 = this.cells[row2, col];

				if ((cell1.IsValidCell && !cell2.IsValidCell)
					|| (!cell1.IsValidCell && cell2.IsValidCell))
				{
					throw new InvalidOperationException("Cannot change a part of range, all cells should be having same colspans during sort.");
				}

				if (cell1.InnerData != null || cell2.InnerData != null)
				{
					var v = cell1.InnerData;
					SetSingleCellData(cell1, cell2.InnerData);
					SetSingleCellData(cell2, v);
				}
			}
		}

        private int UserCellDataComparerAdapter(ISortedEntity data, ISortedEntity @base)
        {
            if (data == null || @base == null)
                return 0;

            return this.cellDataComparer(data.ComparableValue, @base.ComparableValue);
        }

        private Func<object, object, int> cellDataComparer;

        private static int CompareString(string data, string @base, SortOrder order)
        {
            var isDataEmpty = string.IsNullOrEmpty(data);
            var isBaseEmpty = string.IsNullOrEmpty(@base);

            switch (isDataEmpty)
            {
                case true when isBaseEmpty:
                    return 0;
                case true:
                    return order == SortOrder.Ascending ? int.MaxValue : int.MinValue;
            }

            if (isBaseEmpty)
                return order == SortOrder.Ascending ? int.MinValue : int.MaxValue;

            return data.CompareTo(@base);
        }

        private static int CompareCell<T>(T data, T @base, SortOrder order) where T : ISortedEntity
        {
            return CompareCell(data.ComparableValue, @base.ComparableValue, order);
        }

        private static int CompareCell(IComparable data, object @base, SortOrder order)
        {
            if (data == null)
                data = string.Empty;

            if (@base == null)
                @base = string.Empty;

            if (data.GetType() == @base.GetType())
            {
                if (data is string stringData && @base is string stringBase)
                    return CompareString(stringData, stringBase, order);

                return data.CompareTo(@base);
            }

            if (@base is string stringBase1)
                return CompareString(Convert.ToString(data), stringBase1, order);

            if (data is string stringData1)
                return CompareString(stringData1, Convert.ToString(@base), order);

            try
            {
                return ((double) Convert.ChangeType(data, typeof(double))).CompareTo(
                    Convert.ChangeType(@base, typeof(double)));
            }
            catch
            {
                return CompareString(Convert.ToString(data), Convert.ToString(@base), order);
            }
        }

        private void QuickSortColumn(int columnIndex, int start, int end, int startColumn, int endColumn,
            SortOrder order,
            ref RangePosition affectRange, Func<ISortedEntity, ISortedEntity, int> cellComparer)
        {
            if (start >= end)
                return;

            var toSort = new ISortedEntity[end - start + 1];
            for (int i = start, j = 0; i <= end; i++, j++)
            {
                var data = this.GetCellData(i, columnIndex) as IComparable;
                toSort[j] = new SortedEntity(i, data);
            }

            var sorter = new MergeSorter<ISortedEntity>(cellComparer, order);
            var result = sorter.Sort(toSort);

            for (var col = startColumn; col <= endColumn; col++)
            {
                var sortResult = result.Select((t, i) =>
                        new CellSortEntity(i + start, t.OriginalIndex, Cells[t.OriginalIndex, col].Clone()))
                    .ToList();

                foreach (var item in sortResult)
                {
                    if (item.CurrentIndex == item.OriginalIndex)
                        continue;

                    SetCellData(item.CurrentIndex, col, item.Cell.Data);
                    SetCellBody(Cells[item.CurrentIndex, col], item.Cell.Body);
                    if (string.IsNullOrEmpty(item.Cell.Formula))
                        Cells[item.CurrentIndex, col].Formula = null;
                    else
                        SetCellFormula(Cells[item.CurrentIndex, col], item.Cell.Formula);

                    var dataFormatArgs = item.Cell.DataFormatArgs;
                    SetCellDataFormat(Cells[item.CurrentIndex, col], item.Cell.DataFormat, ref dataFormatArgs);
                    Cells[item.CurrentIndex, col].IsReadOnly = item.Cell.IsReadOnly;
                }
            }

            if (affectRange.IsEmpty)
            {
                affectRange.Row = start;
                affectRange.Col = startColumn;
                affectRange.EndRow = end;
                affectRange.EndCol = endColumn;
            }
            else
            {
                if (affectRange.Row > start) affectRange.Row = start;
                if (affectRange.EndRow < end) affectRange.EndRow = end;
            }
        }

        /// <summary>
		/// Event raised when rows sorted on this worksheet.
		/// </summary>
		public event EventHandler<Events.RangeEventArgs> RowsSorted;
		#endregion // Sort


        private class CellSortEntity
        {
            public CellSortEntity(int currentIndex, int originalIndex, Cell cell)
            {
                CurrentIndex = currentIndex;
                OriginalIndex = originalIndex;
                Cell= cell;
            }

            public int CurrentIndex { get; }
            public int OriginalIndex { get; }

            public Cell Cell { get; }
        }
    }

	/// <summary>
	/// Sort order.
	/// </summary>
	public enum SortOrder
	{
		/// <summary>
		/// Ascending
		/// </summary>
		Ascending,

		/// <summary>
		/// Descending
		/// </summary>
		Descending,
	}
}
