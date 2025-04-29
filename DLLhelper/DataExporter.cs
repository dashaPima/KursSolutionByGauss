using System;
using System.Collections.Generic;
using DataTable = System.Data.DataTable;

using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using PPT = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Data;
using System.Drawing.Imaging;
using System.Text;
using System.Diagnostics;

namespace DLLhelper
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
            int rows = table.Rows.Count;
            int cols = table.Columns.Count;

            // 1) Заголовки
            for (int c = 0; c < cols; c++)
                ws.Cells[1, c + 1] = table.Columns[c].ColumnName;

            // 2) Данные
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    ws.Cells[r + 2, c + 1] = table.Rows[r][c];

            // 3) Поиск столбца "Решение"
            int solCol = table.Columns.IndexOf("Решение");
            if (solCol >= 0)
            {
                int solStartRow = rows + 4;
                ws.Cells[solStartRow, 1] = "Решение:";
                // Запись в виде x1, x2 ...
                for (int i = 0; i < rows; i++)
                {
                    ws.Cells[solStartRow + 1 + i, 1] = $"x{i + 1}";
                    ws.Cells[solStartRow + 1 + i, 2] = table.Rows[i][solCol];
                }
                // 4) Построение диаграммы по решениям
                var charts = (Excel.ChartObjects)ws.ChartObjects();
                float left = 300, top = 10, width = 400, height = 300;
                var chartObj = charts.Add(left, top, width, height);
                var chart = chartObj.Chart;
                chart.ChartType = Excel.XlChartType.xlColumnClustered;

                // Диапазон данных: столбец решений
                var yRange = ws.Range[
                    ws.Cells[solStartRow + 1, 2],
                    ws.Cells[solStartRow + rows, 2]
                ];
                // Категории: названия переменных
                var xRange = ws.Range[
                    ws.Cells[solStartRow + 1, 1],
                    ws.Cells[solStartRow + rows, 1]
                ];
                chart.SetSourceData(yRange);
                chart.SeriesCollection(1).XValues = xRange;
                chart.HasTitle = true;
                chart.ChartTitle.Text = "Решение (гистограмма)";
            }

            // Сохраняем в формате по умолчанию через перечисление Excel
            wb.SaveAs(
                filePath,
                Excel.XlFileFormat.xlOpenXMLWorkbook,
                Type.Missing, Type.Missing,
                false, false,
                Excel.XlSaveAsAccessMode.xlNoChange);
            wb.Close(false, Type.Missing, Type.Missing);
            excel.Quit();
        }
        public static void ExportToWord(string textContent, Image chartImage, string filePath)
        {
            // Сохраняем график во временный файл
            var tmpPng = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            chartImage.Save(tmpPng, ImageFormat.Png);
            var word = new Word.Application();
            var doc = word.Documents.Add();
            doc.Content.Text = textContent;

            // Переходим в конец документа
            var range = doc.Content;
            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
            range.InsertParagraphAfter();
            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
            // Вставляем картинку
            doc.InlineShapes.AddPicture(
                FileName: tmpPng,
                LinkToFile: false,
                SaveWithDocument: true,
                Range: range);

            doc.SaveAs2(filePath);
            doc.Close();
            word.Quit();
            // Убираем временный файл
            File.Delete(tmpPng);
        }

        public static void ExportToPowerPoint(DataTable table,
            double[] solution,
            Image chartImage,
            string topic,
            string author,
            string discipline,
            string filePath)
        {
            // Создаем приложение и презентацию
            var pptApp = new PPT.Application();
            var pres = pptApp.Presentations.Add(Office.MsoTriState.msoTrue);

            // Слайд 1: титульный
            var slide1 = pres.Slides.Add(1, PPT.PpSlideLayout.ppLayoutTitle);
            slide1.Shapes[1].TextFrame.TextRange.Text = topic;
            slide1.Shapes[2].TextFrame.TextRange.Text =
                $"Автор: {author}\nДисциплина: {discipline}";

            // Слайд 2: матрица и решение
            var slide2 = pres.Slides.Add(2, PPT.PpSlideLayout.ppLayoutText);
            slide2.Shapes[1].TextFrame.TextRange.Text = "Матрица и решение";
            var sb = new StringBuilder();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var coeffs = string.Join(
                    " ",
                    table.Columns.Cast<DataColumn>()
                         .Where(c => c.ColumnName.StartsWith("x"))
                         .Select(c => table.Rows[i][c]));
                var b = table.Rows[i]["Const"];
                sb.AppendLine($"{coeffs} | {b}");
            }
            sb.AppendLine();
            for (int i = 0; i < solution.Length; i++)
                sb.AppendLine($"x{i + 1} = {solution[i]:F3}");
            slide2.Shapes[2].TextFrame.TextRange.Text = sb.ToString();

            // Слайд 3: гистограмма
            // Сохраняем временно изображение
            string tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            chartImage.Save(tmp, ImageFormat.Png);
            var slide3 = pres.Slides.Add(3, PPT.PpSlideLayout.ppLayoutBlank);
            slide3.Shapes.AddPicture(
                tmp,
                Office.MsoTriState.msoFalse,
                Office.MsoTriState.msoTrue,
                50, 50,
                pres.PageSetup.SlideWidth - 100,
                pres.PageSetup.SlideHeight - 100);

            pres.SaveAs(filePath);
            pres.Close();
            pptApp.Quit();
            File.Delete(tmp);
            // Открываем презентацию в PowerPoint для просмотра
            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            });
        }
    }
}
