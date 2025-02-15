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

#if WINFORM

using System;

using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using unvell.ReoGrid.Data;
using RGRectF = System.Drawing.RectangleF;
using unvell.ReoGrid.Interaction;

namespace unvell.ReoGrid.WinForm
{
	internal class ColumnFilterContextMenu : System.Windows.Forms.ContextMenuStrip
	{
		internal System.Windows.Forms.ToolStripItem SortAZItem { get; set; }
		internal System.Windows.Forms.ToolStripItem SortZAItem { get; set; }

		internal System.Windows.Forms.CheckedListBox CheckedListBox { get; set; }

		internal Button OkButton { get; set; }
		internal Button CancelButton { get; set; }

		public ColumnFilterContextMenu()
		{
			AutoSize = false;
			Width = 240;
			Height = 340;

			this.Items.Add(SortAZItem = new System.Windows.Forms.ToolStripMenuItem(LanguageResource.Filter_SortAtoZ));
			this.Items.Add(SortZAItem = new System.Windows.Forms.ToolStripMenuItem(LanguageResource.Filter_SortZtoA));
			this.Items.Add(new System.Windows.Forms.ToolStripSeparator());

			this.Items.Add(new System.Windows.Forms.ToolStripControlHost(
				CheckedListBox = new System.Windows.Forms.CheckedListBox()
				{
					Dock = System.Windows.Forms.DockStyle.Fill,
					TabStop = false,
					CheckOnClick = true,
				})
			{
				AutoSize = false,
				Width = 200,
				Height = 240,
			});

			CheckedListBox.ItemCheck += checkedListBox_ItemCheck;

			var panel = new System.Windows.Forms.Panel()
			{
				Padding = new System.Windows.Forms.Padding(0, 4, 0, 4),
				Dock = DockStyle.Fill,
				BackColor = Color.Transparent,
			};

			panel.Controls.Add(OkButton = new Button()
			{
				Text = LanguageResource.Button_OK,
				Dock = DockStyle.Right,
			});
			panel.Controls.Add(new Splitter
			{
				Enabled = false,
				Width = 4,
				Dock = DockStyle.Right,
			});
			panel.Controls.Add(CancelButton = new Button()
			{
				Text = LanguageResource.Button_Cancel,
				Dock = DockStyle.Right,
			});

			this.Items.Add(new ToolStripControlHost(panel)
			{
				AutoSize = false,
				Width = 200,
				Height = 30,
			});

		}

		internal int SelectedCount { get; set; }

		private bool inEventProcess = false;

		void checkedListBox_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if (inEventProcess) return;

			inEventProcess = true;
			// 'Select All'
			if (e.Index == 0)
			{
                if (CheckedListBox.GetItemCheckState(0) != e.NewValue)
                {
                    for (var i = 1; i < CheckedListBox.Items.Count; i++)
                        CheckedListBox.SetItemChecked(i, e.NewValue == CheckState.Checked);

                    SelectedCount = e.NewValue == CheckState.Checked ? CheckedListBox.Items.Count - 1 : 0;
                }
            }
			// others else 'Select All'
			else if (e.NewValue != CheckedListBox.GetItemCheckState(0))
			{
				CheckedListBox.SetItemCheckState(0, System.Windows.Forms.CheckState.Indeterminate);

				if (e.NewValue != System.Windows.Forms.CheckState.Checked)
				{
					SelectedCount--;

					if (SelectedCount <= 0)
					{
						CheckedListBox.SetItemChecked(0, false);
						SelectedCount = 0;
					}
				}
				else
				{
					SelectedCount++;

					if (SelectedCount >= CheckedListBox.Items.Count - 1)
					{
						CheckedListBox.SetItemChecked(0, true);
                        SelectedCount = CheckedListBox.Items.Count - 1;
                    }
				}
			}

			inEventProcess = false;
		}

		internal static void ShowFilterPanel(unvell.ReoGrid.Data.AutoColumnFilter.AutoColumnFilterBody headerBody, Point point,
            Action<Func<OnColumnSortedEventHandlerArgs>> action = null)
		{
			if (headerBody.ColumnHeader == null || headerBody.ColumnHeader.Worksheet == null) return;

			var worksheet = headerBody.ColumnHeader.Worksheet;
			if (worksheet == null) return;

			RGRectF headerRect = unvell.ReoGrid.Views.ColumnHeaderView.GetColHeaderBounds(worksheet, headerBody.ColumnHeader.Index, point);
			if (headerRect.Width == 0 || headerRect.Height == 0) return;

			RGRectF buttonRect = headerBody.GetColumnFilterButtonRect(headerRect.Size);

			if (headerBody.ContextMenuStrip == null)
			{
				var filterPanel = new ColumnFilterContextMenu();

				filterPanel.SortAZItem.Click += (s, e) =>
				{
					try
					{ 
                        var affectedRange = worksheet.SortColumn(headerBody.ColumnHeader.Index, headerBody.autoFilter.ApplyRange,	SortOrder.Ascending);
                        action?.Invoke(()=> new OnColumnSortedEventHandlerArgs(affectedRange));
                    }
					catch (Exception ex)
					{
						worksheet.NotifyExceptionHappen(ex);
					}
				};

				filterPanel.SortZAItem.Click += (s, e) =>
				{
                    try
                    {
                        var affectedRange = worksheet.SortColumn(headerBody.ColumnHeader.Index, headerBody.autoFilter.ApplyRange, SortOrder.Descending);
                        action?.Invoke(() => new OnColumnSortedEventHandlerArgs(affectedRange));
                    }
                    catch (Exception ex)
                    {
                        worksheet.NotifyExceptionHappen(ex);
                    }
                };

				filterPanel.OkButton.Click += (s, e) =>
				{
					if (filterPanel.CheckedListBox.GetItemCheckState(0) == CheckState.Checked)
					{
						headerBody.IsSelectAll = true;
					}
					else
					{
						headerBody.IsSelectAll = false;
						headerBody.selectedTextItems.Clear();

						for (int i = 1; i < filterPanel.CheckedListBox.Items.Count; i++)
						{
							if (filterPanel.CheckedListBox.GetItemChecked(i))
							{
								headerBody.selectedTextItems.Add(Convert.ToString(filterPanel.CheckedListBox.Items[i]));
							}
						}
					}

					headerBody.autoFilter.Apply();
					filterPanel.Hide();
				};

				filterPanel.CancelButton.Click += (s, e) => filterPanel.Hide();

				headerBody.ContextMenuStrip = filterPanel;
			}

			if (headerBody.ContextMenuStrip != null)
			{
				if (headerBody.ContextMenuStrip is ColumnFilterContextMenu)
				{
					var filterPanel = (ColumnFilterContextMenu)headerBody.ContextMenuStrip;

					if (headerBody.DataDirty)
					{
						// todo: keep select status for every items before clear
						filterPanel.CheckedListBox.Items.Clear();

						filterPanel.CheckedListBox.Items.Add(LanguageResource.Filter_SelectAll);
						filterPanel.CheckedListBox.SetItemChecked(0, headerBody.IsSelectAll);

						try
						{
							headerBody.ColumnHeader.Worksheet.ControlAdapter.ChangeCursor(CursorStyle.Busy);

							var items = headerBody.GetDistinctItems();

                            filterPanel.CheckedListBox.Items.AddRange(items.OfType<object>().ToArray());

                            filterPanel.CheckedListBox.ItemCheck -= filterPanel.checkedListBox_ItemCheck;
                            for (var i = 0; i < items.Count; i++)
                            {
                                var item = items[i];
                                filterPanel.CheckedListBox.SetItemChecked(i + 1,
                                    headerBody.IsSelectAll || headerBody.selectedTextItems.Contains(item));
                            }
                            filterPanel.CheckedListBox.ItemCheck += filterPanel.checkedListBox_ItemCheck;
                        }
						finally
						{
							headerBody.ColumnHeader.Worksheet.ControlAdapter.ChangeCursor(CursorStyle.PlatformDefault);
						}

                        filterPanel.SelectedCount = filterPanel.CheckedListBox.CheckedItems.Count
                                                    - (filterPanel.CheckedListBox.GetItemChecked(0) ? 1 : 0);

						headerBody.DataDirty = false;

						headerBody.IsSelectAll = filterPanel.SelectedCount == filterPanel.CheckedListBox.Items.Count - 1;
					}
				}

				var pp = new Graphics.Point(headerRect.Right - 240, buttonRect.Bottom + 1);

				pp = worksheet.ControlAdapter.PointToScreen(pp);

				headerBody.ContextMenuStrip.Show(Point.Round(pp));
			}
		}
	}
}

#endif // WINFORM
