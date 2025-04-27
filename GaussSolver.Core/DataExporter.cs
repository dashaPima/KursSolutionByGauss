using System;
using System.Collections.Generic;
using DataTable = System.Data.DataTable;

using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using PPT = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;

namespace GaussSolver.Core
{
    public static class DataExporter
    {
        public static void ExportToExcel(DataTable table, string filePath)
        {
            // Создаём Excel‑приложение
            var excel = new Excel.Application();
            // Добавляем книгу без параметров
            Excel.Workbook wb = excel.Workbooks.Add();
            Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets[1];

            // Заголовки
            for (int c = 0; c < table.Columns.Count; c++)
                ws.Cells[1, c + 1] = table.Columns[c].ColumnName;

            // Данные
            for (int r = 0; r < table.Rows.Count; r++)
                for (int c = 0; c < table.Columns.Count; c++)
                    ws.Cells[r + 2, c + 1] = table.Rows[r][c];

            // Сохраняем в формате по умолчанию через перечисление Excel
            wb.SaveAs(
                filePath,
                Excel.XlFileFormat.xlWorkbookDefault,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Excel.XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing
            );
            wb.Close(false, Type.Missing, Type.Missing);
            excel.Quit();
        }
        public static void ExportToWord(string textContent, string filePath)
        {
            var word = new Word.Application();
            var doc = word.Documents.Add();
            doc.Content.Text = textContent;
            doc.SaveAs2(filePath);
            doc.Close();
            word.Quit();
        }

        public static void ExportToPowerPoint(List<Tuple<string, string>> slides, string filePath)
        {
            var ppt = new PPT.Application();
            // Здесь MsoTriState используется только для Add, но можно обойтись без него
            PPT.Presentation pres = ppt.Presentations.Add(Office.MsoTriState.msoTrue);
            foreach (var slideInfo in slides)
            {
                var slide = pres.Slides.Add(
                    pres.Slides.Count + 1,
                    PPT.PpSlideLayout.ppLayoutText);
                slide.Shapes[1].TextFrame.TextRange.Text = slideInfo.Item1;
                slide.Shapes[2].TextFrame.TextRange.Text = slideInfo.Item2;
            }
            pres.SaveAs(filePath);
            pres.Close();
            ppt.Quit();
        }
    }
}
