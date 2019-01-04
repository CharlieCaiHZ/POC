  /// <summary>
    /// Excel导出帮助类
    /// </summary>
public class ExcelExportHelper
{
	public static string ExcelContentType
	{
		get 
		{
			return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
		}
	}

	/// <summary>
	/// List转DataTable
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="data"></param>
	/// <returns></returns>
	public static DataTable ListToDataTable<T>(List<T> data)
	{
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
		DataTable dataTable = new DataTable();
		for (int i = 0; i < properties.Count; i++)
		{
			PropertyDescriptor property = properties[i];  
			dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);  
		}
		object[] values = new object[properties.Count];
		foreach (T item in data)
		{
			for (int i = 0; i < values.Length; i++)
			{
				values[i] = properties[i].GetValue(item);
			}

			dataTable.Rows.Add(values);
		}
		return dataTable;  
	}


	/// <summary>
	/// 导出Excel
	/// </summary>
	/// <param name="dataTable">数据源</param>
	/// <param name="heading">工作簿Worksheet</param>
	/// <param name="showSrNo">//是否显示行编号</param>
	/// <param name="columnsToTake">要导出的列</param>
	/// <returns></returns>
	public static byte[] ExportExcel(DataTable dataTable, string heading = "", bool showSrNo = false, params string[] columnsToTake)
	{
		byte[] result = null;
		using(ExcelPackage package=new ExcelPackage())
		{
			ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(string.Format("{0}Data", heading));
			int startRowFrom = string.IsNullOrEmpty(heading) ? 1 : 3;  //开始的行
			//是否显示行编号
			if (showSrNo)
			{
				DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
				dataColumn.SetOrdinal(0);
				int index = 1;
				foreach (DataRow item in dataTable.Rows)
				{
					item[0] = index;
					index++;
				}
			}

			//Add Content Into the Excel File
			workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);
			// autofit width of cells with small content  
			int columnIndex = 1;
			foreach (DataColumn item in dataTable.Columns)
			{
				ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];  
				int maxLength = columnCells.Max(cell => cell.Value.ToString().Count());  
				if (maxLength < 150)  
				{  
					workSheet.Column(columnIndex).AutoFit();  
				}  
				columnIndex++;  
			}
			// format header - bold, yellow on black  
			using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
			{
				r.Style.Font.Color.SetColor(System.Drawing.Color.White);
				r.Style.Font.Bold = true;
				r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
				r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
			}

			// format cells - add borders  
			using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
			{
				r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
				r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
				r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
				r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

				r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
				r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
				r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
				r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
			}

			// removed ignored columns  
			for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
			{
				if (i == 0 && showSrNo)
				{
					continue;
				}
				if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
				{
					workSheet.DeleteColumn(i + 1);
				}
			}

			if (!String.IsNullOrEmpty(heading))
			{
				workSheet.Cells["A1"].Value = heading;
				workSheet.Cells["A1"].Style.Font.Size = 20;

				workSheet.InsertColumn(1, 1);
				workSheet.InsertRow(1, 1);
				workSheet.Column(1).Width = 5;
			}

			result = package.GetAsByteArray();  

		}
		return result;
	}
	//real use in project
	public byte[] ExportExcel(List<int> pmIDS,params string[] columnsToTale)
    {
        string currentPath = Server.MapPath(".") + "\\";
        //fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + DateTime.Now.Millisecond;

        string newFilePath = currentPath + fileName + ".xlsx";

        bool showSrNo = false;
        string heading = string.Empty;
        from = txtFromDate.Text;
        to = txtToDate.Text;
        calendarType = rByNormalCalendar.Checked ? "Normal" : "System";
        projectStatus = radioActive.Checked ? "Active" : (radioInactive.Checked ? "Inactive" : "All");
        byte[] reslut = null;
        using (ExcelPackage package = new ExcelPackage())
        {
            
            for (int index = 0; index < pmIDS.Count; index++)
            {
                DataTable dt = bll.GetReportByProjectManager(from, to, calendarType, projectStatus, 3);
                string pmName = lbProjectManager.Items.FindByValue(pmIDS[index].ToString()).Text;
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(string.Format("{0} Report", pmName));

                workSheet.Cells[1, 1].Value = "Project Manager";
                workSheet.Cells[2, 1].Value = "Status";
                workSheet.Cells[3, 1].Value = "From";
                workSheet.Cells[4, 1].Value = "To";

                workSheet.Cells[1, 2].Value = pmName;
                workSheet.Cells[2, 2].Value = projectStatus;
                workSheet.Cells[3, 2].Value = from;
                workSheet.Cells[4, 2].Value = to;

                int startRowFrom = 6;

                //if (showSrNo)
                //{
                //    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                //    dataColumn.SetOrdinal(0);
                //    int index = 1;
                //    foreach (DataRow item in dataTable.Rows)
                //    {
                //        item[0] = index;
                //        index++;
                //    }
                //}
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dt, true);

                int columnIndex = 1;
                foreach (DataColumn item in dt.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    int maxLength = columnCells.Max(cell => cell.Value == null ? 1 : cell.Value.ToString().Count());
                    if (maxLength < 150)
                    {
                        workSheet.Column(columnIndex).AutoFit();
                    }
                    columnIndex++;
                }
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dt.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }
                workSheet.Column(3).Style.Numberformat.Format = "yyyy/mm/dd";
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dt.Rows.Count, dt.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }
                for (int i = 0; i >= dt.Columns.Count - 1; i++)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTale.Contains(dt.Columns[i].ColumnName))
                    {
                        workSheet.DeleteRow(i + 1);
                    }
                }
                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;
                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }
            }

            //FileInfo newFile = new FileInfo(newFilePath);
            //package.SaveAs(newFile);
            //IoTools.BinaryWrite(currentPath + fileName + ".xlsx", true, "ProjectManagerReport");
            reslut = package.GetAsByteArray();
        }
	/// <summary>
	/// 导出Excel
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="data"></param>
	/// <param name="heading"></param>
	/// <param name="isShowSlNo"></param>
	/// <param name="ColumnsToTake"></param>
	/// <returns></returns>
	public static byte[] ExportExcel<T>(List<T> data, string heading = "", bool isShowSlNo = false, params string[] ColumnsToTake)
	{
		return ExportExcel(ListToDataTable<T>(data), heading, isShowSlNo, ColumnsToTake);  
	}

}