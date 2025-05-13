using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Reflection;
using DLLhelper;
using System.Runtime.InteropServices;
using System.Globalization;

namespace KyrsovRPVS
{
    public partial class MainForm : Form
    {
        private double[] _matlabSolution = null;
        private List<Tuple<int, double>> _matlabChartData = null;
        // Поле для хранения выбранного цвета столбцов. По умолчанию синий.
        private Color _selectedBarColor = Color.Blue;
        // Поле для хранения текущих данных графика, чтобы можно было перерисовать
        private List<Tuple<int, double>> _currentChartData;
        private readonly IniFile _ini;
        private const string IniSection = "Settings";
        private const string IniKeyN = "UnknownsCount";
        private const string IniKeyMatrix = "Matrix"; // будем хранить все элементы одной строкой через '|'

        private LinearSystem _currentSystem;
        public MainForm()
        {
            InitializeComponent();
            dgvSystem.AllowUserToAddRows = false;
            var iniPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "settings.ini");
            _ini = new IniFile(iniPath);
            LoadFromIni();
            this.FormClosing += (s, e) => SaveToIni();

            this.Text = "Основное окно программы";
            // Отключаем добавление пустой строки в dgvSystem
            this.ContextMenuStrip = ctxmenu;
            MenuItem1.Click += (s, e) =>
            {
                var help = new AboutForm("about.htm");
                help.Show();
            };
            MenuItem2.Click += (s, e) =>
            {
                var help = new AboutForm("input.htm");
                help.Show();
            };
            MenuItem3.Click += (s, e) => экспортWord(s, e);
            MenuItem4.Click += (s, e) => экспортExcel(s, e);
            MenuItem5.Click += (s, e) => экспортPowerPoint(s, e);

            toolStripLabelHelp.Click += (s, e) =>
            {
                var help = new AboutForm("about.htm");
                help.Show();
            };
            toolStripLabelInformation.Click += (s, e) =>
            {
                var help = new AboutForm("input.htm");
                help.Show();
            };
            amoutUnknowns.Enter += (s, e) => this.Tag = "input.htm";
            dgvSystem.Enter += (s, e) => this.Tag = "input.htm";

            toolStripExcel.Click += (s, e) => экспортWord(s, e);
            toolStripWord.Click += (s, e) => экспортWord(s, e);
            toolStripGraph.Click += (s, e) => findSolution(s, e);
            toolStripPowerPoint.Click += (s, e) => экспортPowerPoint(s, e);
            toolStripInfo.Click += (s, e) =>
            {
                var help = new AboutForm("input.htm");
                help.Show();
            };
        }

        private void LoadFromIni()
        {
            // Считаем n
            if (int.TryParse(_ini.Read(IniSection, IniKeyN), out int n) && n > 0)
            {
                amoutUnknowns.Text = n.ToString();
                FormTable(this, EventArgs.Empty);
                // Считаем матрицу: строка вида "1;2;3|4;5;6|..."
                var raw = _ini.Read(IniSection, IniKeyMatrix);
                if (!string.IsNullOrEmpty(raw))
                {
                    var rows = raw.Split('|');
                    for (int i = 0; i < rows.Length && i < dgvSystem.RowCount; i++)
                    {
                        var vals = rows[i].Split(';');
                        for (int j = 0; j < vals.Length && j < dgvSystem.ColumnCount; j++)
                            dgvSystem[j, i].Value = vals[j];
                    }
                }
            }
        }

        private void SaveToIni()
        {
            // Сохраняем n
            _ini.Write(IniSection, IniKeyN, amoutUnknowns.Text);

            // Сохраняем все ячейки матрицы в одну строку
            var sb = new StringBuilder();
            int rows = dgvSystem.RowCount;
            int cols = dgvSystem.ColumnCount;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    sb.Append(dgvSystem[j, i].Value?.ToString() ?? "");
                    if (j < cols - 1) sb.Append(';');
                }
                if (i < rows - 1) sb.Append('|');
            }
            _ini.Write(IniSection, IniKeyMatrix, sb.ToString());
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                string file = this.Tag as string ?? "about.htm";
                new AboutForm(file).Show();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FormTable(object sender, EventArgs e)
        {
            if (!int.TryParse(amoutUnknowns.Text, out int n) || n <= 0)
            {
                MessageBox.Show("Введите корректное число переменных (n > 0)", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Очистка существующей таблицы
            dgvSystem.Columns.Clear();
            dgvSystem.Rows.Clear();

            // Создание столбцов A1..An и столбца B
            for (int i = 1; i <= n; i++)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    Name = $"A{i}",
                    HeaderText = $"A{i}",
                    Width = 60
                };
                dgvSystem.Columns.Add(col);
            }
            var freeCol = new DataGridViewTextBoxColumn
            {
                Name = "B",
                HeaderText = "B",
                Width = 60
            };
            dgvSystem.Columns.Add(freeCol);

            // Добавить n строк для ввода
            dgvSystem.RowCount = n;
        }

        private void findSolution(object sender, EventArgs e)
        {
            try
            {
                // число столбцов (последний столбец — свободные члены B)
                int colCount = dgvSystem.Columns.Count;
                // число неизвестных
                int n = colCount - 1;
                // число строк
                int rowCount = dgvSystem.RowCount;

                // проверяем, что строк ровно n
                if (rowCount != n)
                {
                    MessageBox.Show(
                        $"Матрица должна быть {n}×{n}, а не {rowCount}×{colCount}.",
                        "Неверный размер матрицы",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                // заполняем матрицу коэффициентов и вектор свободных членов
                var matrix = new double[n, n];
                var constants = new double[n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        var cell = dgvSystem.Rows[i].Cells[j].Value;
                        if (!double.TryParse(cell?.ToString(), out matrix[i, j]))
                        {
                            MessageBox.Show(
                                $"Неверное значение в ячейке A{j + 1},{i + 1}: '{cell}'",
                                "Ошибка ввода",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }
                    }
                    // последний столбец — свободные члены B
                    var bc = dgvSystem.Rows[i].Cells[n].Value;
                    if (!double.TryParse(bc?.ToString(), out constants[i]))
                    {
                        MessageBox.Show(
                            $"Неверное значение свободного члена в строке {i + 1}: '{bc}'",
                            "Ошибка ввода",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }
                }

                // собственно, решение
                _currentSystem = new LinearSystem(matrix, constants);
                var solution = _currentSystem.SolveByGauss();

                // выводим в dgvSolution с форматом F3
                dgvSolution.Columns.Clear();
                dgvSolution.Rows.Clear();
                for (int i = 0; i < n; i++)
                {
                    var col = new DataGridViewTextBoxColumn
                    {
                        Name = $"x{i + 1}",
                        HeaderText = $"x{i + 1}",
                        DefaultCellStyle = { Format = "F3" },
                        Width = 60
                    };
                    dgvSolution.Columns.Add(col);
                }
                dgvSolution.RowCount = 1;
                for (int i = 0; i < n; i++)
                    dgvSolution.Rows[0].Cells[i].Value = solution[i];

                // график
                var graphData = _currentSystem.GetGraphData(solution);
                _currentChartData = graphData;
                DrawBarChart(_currentChartData, _matlabChartData);

                // анимация
                lstAnimationSteps.Items.Clear();
                int step = 1;
                foreach (var vec in _currentSystem.GetAnimationSteps())
                {
                    lstAnimationSteps.Items.Add(
                        "Шаг " + (step++) + ": " +
                        string.Join(", ", vec
                            .Select((v, idx) => $"x{idx + 1}={v:F3}"))
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при решении:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void DrawBarChart(List<Tuple<int, double>> gaussData, List<Tuple<int, double>> matlabData)
        {
            if (pictureBox == null) return;

            int w = pictureBox.Width;
            int h = pictureBox.Height;

            if (w <= 0 || h <= 0)
            {
                if (pictureBox.Image != null) pictureBox.Image.Dispose();
                pictureBox.Image = null;
                return;
            }

            Bitmap bmp = null;
            try
            {
                bmp = new Bitmap(w, h);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.White);
                    int margin = 40; // Увеличим немного отступ для меток и легенды

                    if (w <= margin * 2 + 20 || h <= margin * 2 + 20) // +20 для запаса
                    {
                        using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                            g.DrawString("Недостаточно места\nдля графика", this.Font, Brushes.Black, new RectangleF(0, 0, w, h), sf);
                        if (pictureBox.Image != null) pictureBox.Image.Dispose();
                        pictureBox.Image = bmp;
                        return;
                    }

                    int plotWidth = w - margin * 2;
                    int plotHeight = h - margin * 2;

                    List<Tuple<int, double>> allDataForRange = new List<Tuple<int, double>>();
                    if (gaussData != null && gaussData.Any()) allDataForRange.AddRange(gaussData);
                    if (matlabData != null && matlabData.Any()) allDataForRange.AddRange(matlabData);


                    if (!allDataForRange.Any())
                    {
                        g.DrawLine(Pens.Black, margin, margin, margin, margin + plotHeight); // Ось Y
                        g.DrawLine(Pens.Black, margin, margin + plotHeight, margin + plotWidth, margin + plotHeight); // Ось X
                        if (pictureBox.Image != null) pictureBox.Image.Dispose();
                        pictureBox.Image = bmp;
                        return;
                    }

                    int n = 0; // Количество переменных (столбцов на графике)
                    if (gaussData != null && gaussData.Any()) n = gaussData.Count;
                    else if (matlabData != null && matlabData.Any()) n = matlabData.Count;

                    if (n == 0)
                    { // Если после проверок n все равно 0
                        if (pictureBox.Image != null) pictureBox.Image.Dispose(); pictureBox.Image = bmp; return;
                    }


                    double maxVal = allDataForRange.Max(t => t.Item2);
                    double minVal = allDataForRange.Min(t => t.Item2);

                    if (Math.Abs(maxVal - minVal) < 1e-9)
                    {
                        if (Math.Abs(maxVal) < 1e-9) { minVal = -1; maxVal = 1; }
                        else
                        {
                            double offset = Math.Abs(maxVal * 0.5);
                            if (offset < 0.5) offset = 0.5; // Минимальный отступ
                            minVal = maxVal - offset;
                            maxVal = maxVal + offset;
                        }
                    }
                    // Гарантируем, что 0 включен в диапазон, если значения одного знака
                    if (minVal > 0) minVal = 0;
                    if (maxVal < 0) maxVal = 0;
                    // Если после этого minVal стал равен maxVal (например, оба 0), снова расширяем
                    if (Math.Abs(maxVal - minVal) < 1e-9) { minVal -= 1; maxVal += 1; }


                    double rangeY = maxVal - minVal;
                    if (Math.Abs(rangeY) < 1e-9) rangeY = 1; // Защита от деления на ноль

                    float yZeroPhysical = margin + (float)((maxVal / rangeY) * plotHeight);
                    // Коррекция для случая, когда все значения отрицательные или все положительные
                    if (maxVal <= 0) yZeroPhysical = margin; // Ось X вверху
                    else if (minVal >= 0) yZeroPhysical = margin + plotHeight; // Ось X внизу


                    g.DrawLine(Pens.Black, margin, margin, margin, margin + plotHeight); // Ось Y
                    g.DrawLine(Pens.Black, margin, yZeroPhysical, margin + plotWidth, yZeroPhysical); // Ось X

                    float totalSlotWidth = plotWidth / (float)n; // Ширина "слота" для одной переменной xi
                    float barWidth;
                    float gapBetweenSeries = totalSlotWidth * 0.05f; // Небольшой отступ между столбцами Гаусса и MATLAB

                    bool hasGauss = gaussData != null && gaussData.Any();
                    bool hasMatlab = matlabData != null && matlabData.Any();

                    if (hasGauss && hasMatlab)
                    {
                        barWidth = (totalSlotWidth * 0.7f - gapBetweenSeries) / 2f; // 0.7f - чтобы оставить место по краям слота
                    }
                    else
                    {
                        barWidth = totalSlotWidth * 0.5f; // 0.5f - если только один набор данных, столбец будет пошире
                    }
                    if (barWidth < 1) barWidth = 1;


                    using (SolidBrush gaussBrush = new SolidBrush(_selectedBarColor))
                    using (SolidBrush matlabBrush = new SolidBrush(Color.Red))
                    using (Font valueFont = new Font(this.Font.FontFamily, 8f)) // Фиксированный размер для читаемости
                    using (Font labelFont = new Font(this.Font.FontFamily, 9f))
                    {
                        for (int i = 0; i < n; i++)
                        {
                            float slotStartX = margin + i * totalSlotWidth;
                            float currentX;

                            // Столбец Гаусса
                            if (hasGauss && i < gaussData.Count)
                            {
                                if (hasMatlab) currentX = slotStartX + (totalSlotWidth * 0.15f); // Смещаем влево в слоте
                                else currentX = slotStartX + (totalSlotWidth - barWidth) / 2f; // Центрируем в слоте

                                double val = gaussData[i].Item2;
                                float barLength = (float)(Math.Abs(val) / rangeY * plotHeight);
                                if (barLength < 1 && val != 0) barLength = 1;
                                float yTop = (val >= 0) ? (yZeroPhysical - barLength) : yZeroPhysical;
                                g.FillRectangle(gaussBrush, currentX, yTop, barWidth, barLength);
                                // Можно добавить текст и для Гаусса
                            }

                            // Столбец MATLAB
                            if (hasMatlab && i < matlabData.Count)
                            {
                                if (hasGauss) currentX = slotStartX + (totalSlotWidth * 0.15f) + barWidth + gapBetweenSeries; // Правее Гаусса
                                else currentX = slotStartX + (totalSlotWidth - barWidth) / 2f; // Центрируем в слоте

                                double val = matlabData[i].Item2;
                                float barLength = (float)(Math.Abs(val) / rangeY * plotHeight);
                                if (barLength < 1 && val != 0) barLength = 1;
                                float yTop = (val >= 0) ? (yZeroPhysical - barLength) : yZeroPhysical;
                                g.FillRectangle(matlabBrush, currentX, yTop, barWidth, barLength);

                                string valText = val.ToString("F2");
                                SizeF szVal = g.MeasureString(valText, valueFont);
                                float yValText = (val >= 0) ? (yTop - szVal.Height - 2) : (yTop + barLength + 2);
                                // Проверка, чтобы текст не выходил за пределы сверху/снизу
                                if (yValText < margin - szVal.Height) yValText = margin;
                                if (yValText > margin + plotHeight) yValText = margin + plotHeight - szVal.Height;

                                g.DrawString(valText, valueFont, Brushes.DarkRed, currentX + (barWidth - szVal.Width) / 2, yValText);
                            }

                            // Метка X (одна на слот)
                            string label = $"x{i + 1}"; // Используем индекс i+1
                            SizeF szLabel = g.MeasureString(label, labelFont);
                            float xLabelPos = slotStartX + (totalSlotWidth - szLabel.Width) / 2;
                            float yLabelPos = margin + plotHeight + 5;
                            if (yLabelPos + szLabel.Height > h - 5) yLabelPos = h - szLabel.Height - 5; // Не выходить за край
                            g.DrawString(label, labelFont, Brushes.Black, xLabelPos, yLabelPos);
                        }

                        // Легенда
                        float legendY = margin - 25; // Рисуем над графиком
                        float legendXStart = margin + 10;
                        if (hasGauss)
                        {
                            g.FillRectangle(gaussBrush, legendXStart, legendY, 15, 10);
                            g.DrawString("Gauss", labelFont, Brushes.Black, legendXStart + 20, legendY - 3);
                            legendXStart += 70 + g.MeasureString("Gauss", labelFont).Width;
                        }
                        if (hasMatlab)
                        {
                            g.FillRectangle(matlabBrush, legendXStart, legendY, 15, 10);
                            g.DrawString("MATLAB", labelFont, Brushes.Black, legendXStart + 20, legendY - 3);
                        }
                    }

                    // Метки оси Y
                    int numYTicks = Math.Max(3, Math.Min(6, plotHeight / 40)); // Адаптивное кол-во меток
                    using (Font axisFont = new Font(this.Font.FontFamily, 7f))
                    {
                        for (int k = 0; k <= numYTicks; k++)
                        {
                            double tickValue = minVal + (rangeY / numYTicks) * k;
                            float yTickPhysical = margin + plotHeight - (float)((tickValue - minVal) / rangeY * plotHeight);

                            if (yTickPhysical >= margin - 1 && yTickPhysical <= margin + plotHeight + 1) // Небольшой допуск
                            {
                                g.DrawLine(Pens.Gray, margin - 4, yTickPhysical, margin, yTickPhysical);
                                string tickText = tickValue.ToString("F1");
                                SizeF tickTextSize = g.MeasureString(tickText, axisFont);
                                g.DrawString(tickText, axisFont, Brushes.Black, margin - tickTextSize.Width - 6, yTickPhysical - tickTextSize.Height / 2);
                            }
                        }
                    }
                } // конец using (Graphics g ...)

                if (pictureBox.Image != null) pictureBox.Image.Dispose();
                pictureBox.Image = bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отрисовке графика с MATLAB: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (bmp != null) bmp.Dispose();
                try
                {
                    using (var g = pictureBox.CreateGraphics())
                    {
                        g.Clear(Color.White);
                        using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                            g.DrawString($"Ошибка отрисовки:\n{ex.Message.Split('\n')[0]}", this.Font, Brushes.Red, new RectangleF(0, 0, w, h), sf);
                    }
                }
                catch { }
            }
        }

        private void exitform(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void экспортWord(object sender, EventArgs e)
        {
            if (_currentSystem == null)
            {
                MessageBox.Show("Сначала решите систему!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var sfd = new SaveFileDialog
            {
                Filter = "Word Document|*.docx;*.doc",
                Title = "Сохранить в Word"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                // Собираем текст: исходная матрица + решение
                var sb = new StringBuilder();
                sb.AppendLine("Разработка приложения решения систем линейных алгебраических уравнений методом Гаусса");
                sb.AppendLine();
                // Исходная матрица
                sb.AppendLine("Матрица A и вектор B:");
                var dt = _currentSystem.ToDataTable();
                foreach (DataRow row in dt.Rows)
                {
                    var coeffs = dt.Columns
                                   .Cast<DataColumn>()
                                   .Where(c => c.ColumnName.StartsWith("x"))
                                   .Select(c => row[c].ToString());
                    var b = row["Const"];
                    sb.AppendLine(string.Join("\t", coeffs) + "\t|\t" + b);
                }
                sb.AppendLine();
                // Решение
                var sol = _currentSystem.SolveByGauss();
                sb.AppendLine("Результаты:");
                for (int i = 0; i < sol.Length; i++)
                    sb.AppendLine($"x{i + 1} = {sol[i]:F4}");

                // Экспортируем вместе с графиком
                DataExporter.ExportToWord(
                    sb.ToString(),
                    pictureBox.Image,
                    sfd.FileName);
                MessageBox.Show("Экспорт в Word выполнен.", "",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void экспортExcel(object sender, EventArgs e)
        {
            if (_currentSystem == null)
            {
                MessageBox.Show("Сначала решите систему!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook|*.xlsx",
                Title = "Сохранить в Excel"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                // Превращаем систему в DataTable
                DataTable table = _currentSystem.ToDataTable();
                // Добавляем ещё один столбец — решение
                var sol = _currentSystem.SolveByGauss();
                table.Columns.Add("Решение", typeof(double));
                for (int i = 0; i < sol.Length; i++)
                    table.Rows[i]["Решение"] = sol[i];

                // Экспортируем в Excel (вместе с диаграммой, если в DataExporter это реализовано)
                DataExporter.ExportToExcel(table, sfd.FileName);
                MessageBox.Show("Экспорт в Excel выполнен.", "",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void экспортPowerPoint(object sender, EventArgs e)
        {
            if (_currentSystem == null)
            {
                MessageBox.Show("Сначала решите систему!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var sfd = new SaveFileDialog
            {
                Filter = "PowerPoint Presentation|*.pptx",
                Title = "Сохранить в PowerPoint"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            string filePath = sfd.FileName;
            DataExporter.ExportToPowerPoint(
                _currentSystem.ToDataTable(),
                _currentSystem.SolveByGauss(),
                pictureBox.Image,
                "Разработка приложения решения систем линейных алгебраических уравнений методом Гаусса",
                "Пимошенко Дарья Андреевна ст. 2 курса гр.10701323",
                "Разработка приложений в визуальных средах",
                filePath);
            MessageBox.Show("Экспорт в PowerPoint выполнен.", "",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripChangeColor(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = _selectedBarColor;
                if (colorDialog.ShowDialog(this) == DialogResult.OK) // Передаем 'this' для правильного родительского окна
                {
                    _selectedBarColor = colorDialog.Color;

                    // Перерисовываем график с новым цветом, если есть данные
                    if (_currentChartData != null && _currentChartData.Any())
                    {
                        DrawBarChart(_currentChartData, _matlabChartData);
                    }
                    else
                    {
                        // Если данных нет, можно просто очистить PictureBox или показать сообщение
                        if (pictureBox.Image != null)
                        {
                            pictureBox.Image.Dispose();
                            pictureBox.Image = null;
                        }
                        using (var g = pictureBox.CreateGraphics()) g.Clear(Color.White); // Очистить фон
                        MessageBox.Show("Данные для графика отсутствуют. Загрузите данные перед сменой цвета.");
                    }
                }
            }
        }

        private async void solveMatlab(object sender, EventArgs e)
        {
            int colCount = dgvSystem.Columns.Count;
            if (colCount <= 1)
            {
                MessageBox.Show("Матрица не определена. Сначала сформируйте систему.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int n = colCount - 1;
            int rowCount = dgvSystem.RowCount;

            if (rowCount != n)
            {
                MessageBox.Show($"Матрица должна быть квадратной {n}×{n}. Текущий размер {rowCount}×{colCount}.", "Неверный размер матрицы", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var matrixA = new double[n, n];
            var vectorB = new double[n];
            try
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        var cell = dgvSystem.Rows[i].Cells[j].Value;
                        // Пытаемся парсить с текущей культурой, затем с инвариантной
                        if (!double.TryParse(cell?.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out matrixA[i, j]) &&
                            !double.TryParse(cell?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out matrixA[i, j]))
                        {
                            MessageBox.Show($"Неверное значение в ячейке A{j + 1}, строка {i + 1}: '{cell}'", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dgvSystem.Rows[i].Cells[j].Selected = true;
                            return;
                        }
                    }
                    var bcCell = dgvSystem.Rows[i].Cells[n].Value;
                    if (!double.TryParse(bcCell?.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out vectorB[i]) &&
                        !double.TryParse(bcCell?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out vectorB[i]))
                    {
                        MessageBox.Show($"Неверное значение свободного члена в строке {i + 1}: '{bcCell}'", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        dgvSystem.Rows[i].Cells[n].Selected = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении данных из таблицы: {ex.Message}", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            string originalButtonText = btnMatlabVerify.Text;

            // Используем лямбда-выражение для обновления текста кнопки
            MatlabLinearSystemSolver.UpdateStatusDelegate statusUpdater = (message, isBusy) =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((System.Windows.Forms.MethodInvoker)delegate { btnMatlabVerify.Text = message; });
                }
                else
                {
                    btnMatlabVerify.Text = message;
                }
            };
            statusUpdater("MATLAB (Запуск...)", true);
            MatlabVerificationResult matlabResult = await MatlabLinearSystemSolver.SolveWithMatlabAsync(matrixA, vectorB, statusUpdater);

            this.Enabled = true;
            Cursor.Current = Cursors.Default;
            btnMatlabVerify.Text = originalButtonText;
            if (matlabResult.Success)
            {
                _matlabSolution = matlabResult.Solution;
                _matlabChartData = new List<Tuple<int, double>>();
                for (int i = 0; i < _matlabSolution.Length; i++)
                {
                    _matlabChartData.Add(Tuple.Create(i + 1, _matlabSolution[i]));
                }

                StringBuilder sbSolution = new StringBuilder("Решение, полученное из MATLAB:\n");
                for (int i = 0; i < _matlabSolution.Length; i++)
                {
                    sbSolution.AppendLine($"x{i + 1} = {_matlabSolution[i]:F4}"); // Форматируем до 4 знаков после запятой
                }
                MessageBox.Show(sbSolution.ToString(), "Результат MATLAB", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DrawBarChart(_currentChartData, _matlabChartData);

                MessageBox.Show($"Скрипт MATLAB сохранен на рабочий стол: {Path.GetFileName(matlabResult.MatlabScriptPath)}",
                                "MATLAB", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Ошибка при выполнении в MATLAB:\n{matlabResult.ErrorMessage}", "Ошибка MATLAB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _matlabSolution = null;
                _matlabChartData = null;
                // Если была ошибка, можно перерисовать график только с данными Гаусса (если они есть)
                DrawBarChart(_currentChartData, null);
            }
        }
    }
}
