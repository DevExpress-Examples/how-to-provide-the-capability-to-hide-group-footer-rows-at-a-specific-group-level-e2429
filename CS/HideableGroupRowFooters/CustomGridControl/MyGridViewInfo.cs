﻿using System.Drawing;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace HideableGroupRowFooters
{

	public class MyGridViewInfo : GridViewInfo
	{
		public MyGridViewInfo(MyGridView gridView)
			: base(gridView)
		{
		}

		protected override void CalcRowFooterInfo(GridRowInfo ri, GridRow row, GridRow nextRow)
		{
			int height = ri.RowFooters.RowFootersHeight;
			if ( height == 0 )
				return;

			bool isShowCurrentFooter = IsShowCurrentRowFooter(ri);
			int startLevel = ri.Level;
			int footerRowHandle = ri.RowHandle;

			if ( !ri.IsGroupRow || !isShowCurrentFooter )
				footerRowHandle = View.GetParentRowHandle(footerRowHandle);

			if ( !isShowCurrentFooter )
			{
				startLevel--;
			}

			int top = ri.TotalBounds.Bottom - height - ri.RowSeparatorBounds.Height;
			int left = ri.IndentRect.Right - (!isShowCurrentFooter ? LevelIndent : 0);
			ri.RowFooters.Bounds = new Rectangle(left, top, ri.DataBounds.Right - left, height);

			for ( int n = 0; n < ri.RowFooters.RowFooterCount; n++ )
			{
				ShowGroupFooterEventArgs args = new ShowGroupFooterEventArgs(startLevel);
				RaiseShowGroupFooter(args);

				if ( !args.Visible )
				{
					startLevel -= 1;
					left -= LevelIndent;
					ri.RowFooters.RowFooterCount += 1;
					footerRowHandle = View.GetParentRowHandle(footerRowHandle);

					continue;
				}

				GridRowFooterInfo fi = new GridRowFooterInfo();
				ri.RowFooters.Add(fi);
				fi.RowHandle = footerRowHandle;
				fi.Bounds = ri.Bounds;
				fi.Level = startLevel;
				fi.Bounds.Y = top;
				fi.Bounds.X = left;
				fi.Bounds.Width = ri.DataBounds.Right - fi.Bounds.Left;
				fi.Bounds.Height = GroupFooterHeight;
				top += fi.Bounds.Height;

				if ( !ri.IndicatorRect.IsEmpty )
				{
					fi.IndicatorRect = ri.IndicatorRect;
					fi.IndicatorRect.Y = fi.Bounds.Y;
					fi.IndicatorRect.Height = fi.Bounds.Height;
				}

				if ( View.OptionsView.ShowHorizontalLines != DevExpress.Utils.DefaultBoolean.False )
				{
                    ri.AddRowLineInfo(fi.Bounds.Left, fi.Bounds.Bottom - 1,
                        fi.Bounds.Width, 1, PaintAppearance.HorzLine);

                    fi.Bounds.Height -= 1;
				}

				CalcRowCellsFooterInfo(fi, ri);
				footerRowHandle = View.GetParentRowHandle(footerRowHandle);
				startLevel--;
				left -= LevelIndent;
			}
		}

		public override int GetRowFooterCount(int rowHandle, int rowVisibleIndex, bool isExpanded)
		{
			int initialVisibleFootersCount = base.GetRowFooterCount(rowHandle, rowVisibleIndex, isExpanded);
			int visibleFootersCount = initialVisibleFootersCount;

			int footerRowHandle = rowHandle;
			for ( int i = 0; i < initialVisibleFootersCount; i++ )
			{
				ShowGroupFooterEventArgs args = new ShowGroupFooterEventArgs(View.GetRowLevel(footerRowHandle));
				RaiseShowGroupFooter(args);

				if ( !args.Visible )
					visibleFootersCount--;

				footerRowHandle = View.GetParentRowHandle(footerRowHandle);
			}

			return visibleFootersCount;
		}

		private void RaiseShowGroupFooter(ShowGroupFooterEventArgs args)
		{
			MyGridView view = View as MyGridView;
			if ( view != null )
				view.RaiseViewInfoShowGroupFooter(view, args);
		}
	}
}
