using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace TMS.Common
{
    /// <summary>
    /// 将文件以流的形式输出
    /// </summary>
    /// <module>
    /// 公用类模块
    /// </module>
    public class IoTools
    {
        /// <summary>
        /// 输出文件流
        /// </summary>
        /// <param name="sFileName">实际路径</param>
        /// <param name="isDelFile">是否删除原有文件,true为删除</param>
        public static void BinaryWrite(string sFileName, bool isDelFile, string OutFileName)
        {
            using (FileStream fileStream = new FileStream(sFileName, FileMode.Open))
            {
                string exten = System.IO.Path.GetExtension(sFileName);
                long fileSize = fileStream.Length;
                byte[] fileBuffer = new byte[fileSize];
                fileStream.Read(fileBuffer, 0, (int)fileSize);

                fileStream.Close();//及时关闭掉

                if (isDelFile)
                {
                    System.IO.File.Delete(sFileName);//是否删除原有文件
                }


                HttpContext.Current.Response.Clear();

                if (exten == ".xls" || exten == ".xlsx" || exten == ".csv")
                {
                    HttpContext.Current.Response.ContentType = "application/ms-excel";
                }
                else
                {
                    HttpContext.Current.Response.ContentType = "application/octet-stream";
                }

                HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(OutFileName + exten, Encoding.UTF8));
                HttpContext.Current.Response.AddHeader("Content-Length", fileSize.ToString());

                HttpContext.Current.Response.BinaryWrite(fileBuffer);
                HttpContext.Current.Response.Flush();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }

        }
    }
}
