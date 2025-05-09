﻿using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DLLhelper;

namespace KyrsovRPVS
{
    public partial class MainForm : Form
    {
        private LinearSystem _currentSystem;
        public MainForm()
        {
            InitializeComponent();
            this.Text = "Основное окно программы";
            // Отключаем добавление пустой строки в dgvSystem
            dgvSystem.AllowUserToAddRows = false;
            btnformTable.Click += FormTable;
            btnFindSolution.Click += findSolution;
            this.ContextMenuStrip = ctxmenu;
            MenuItem1.Click += (s, e) => {
                var help = new AboutForm("about.htm");
                help.Show();
            };
            MenuItem2.Click += (s, e) => {
                var help = new AboutForm("input.htm");
                help.Show();
            };
            MenuItem3.Click += (s, e) => экспортWord(s,e);
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
                int n = dgvSystem.Columns.Count - 1;
                double[,] matrix = new double[n, n];
                double[] constants = new double[n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                        matrix[i, j] = Convert.ToDouble(dgvSystem[j, i].Value);
                    constants[i] = Convert.ToDouble(dgvSystem[n, i].Value);
                }

                _currentSystem = new LinearSystem(matrix, constants);
                var solution = _currentSystem.SolveByGauss();

                dgvSolution.Columns.Clear();
                dgvSolution.Rows.Clear();
                // Создаем колонки с форматированием до 3-х знаков
                for (int i = 0; i < n; i++)
                {
                    var column = new DataGridViewTextBoxColumn
                    {
                        Name = $"x{i + 1}",
                        HeaderText = $"x{i + 1}",
                        Width = 60
                    };
                    column.DefaultCellStyle.Format = "F3";
                    dgvSolution.Columns.Add(column);
                }
                dgvSolution.RowCount = 1;
                for (int i = 0; i < n; i++)
                    dgvSolution[i, 0].Value = solution[i];

                // График
                var graphData = _currentSystem.GetGraphData(solution);
                DrawBarChart(graphData);

                // Анимация
                lstAnimationSteps.Items.Clear();
                int stepNumber = 1;
                foreach (var step in _currentSystem.GetAnimationSteps())
                {
                    string text = $"Шаг {stepNumber}: " +
                                  string.Join(", ", step
                                    .Select((val, idx) => $"x{idx + 1}={val:F3}"));
                    lstAnimationSteps.Items.Add(text);
                    stepNumber++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при решении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawBarChart(List<Tuple<int, double>> data)
        {
            int w = pictureBox.Width;
            int h = pictureBox.Height;
            var bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                int margin = 40;
                int plotWidth = w - margin * 2;
                int plotHeight = h - margin * 2;

                int n = data.Count;
                if (n == 0) { pictureBox.Image = bmp; return; }

                double maxVal = data.Max(t => t.Item2);
                double minVal = data.Min(t => t.Item2);
                double range = maxVal - minVal;
                if (Math.Abs(range) < 1e-6) range = 1;

                float yZero = margin + (float)((maxVal - 0) / range * plotHeight);

                // Оси
                g.DrawLine(Pens.Black, margin, margin, margin, margin + plotHeight);
                g.DrawLine(Pens.Black, margin, yZero, margin + plotWidth, yZero);

                float barWidth = plotWidth / (float)n * 0.6f;
                float gap = (plotWidth - barWidth * n) / (n + 1);

                for (int i = 0; i < n; i++)
                {
                    double val = data[i].Item2;
                    float x = margin + gap + i * (barWidth + gap);
                    float barLength = (float)(Math.Abs(val) / range * plotHeight);

                    // вычисляем верхнюю Y-позицию столбца
                    float yTop = val >= 0 ? yZero - barLength : yZero;
                    // рисуем столбец
                    g.FillRectangle(Brushes.Blue, x, yTop, barWidth, barLength);

                    // вывод значения над/под столбцом
                    string valText = val.ToString("F3");
                    var szVal = g.MeasureString(valText, this.Font);
                    float barBottom = val >= 0 ? yZero : yZero + barLength;
                    float yVal = val >= 0
                        ? yTop - szVal.Height - 2
                        : barBottom + 2;
                    g.DrawString(valText, this.Font, Brushes.Black,
                                 x + (barWidth - szVal.Width) / 2,
                                 yVal);

                    // вывод названия переменной ниже всех
                    string label = $"x{data[i].Item1}";
                    var szLabel = g.MeasureString(label, this.Font);
                    float yLabel = barBottom + 2 + szVal.Height + 2;
                    g.DrawString(label, this.Font, Brushes.Black,
                                 x + (barWidth - szLabel.Width) / 2,
                                 yLabel);
                }
            }
            pictureBox.Image = bmp;
        }

        private void exitform(object sender, EventArgs e)
        {
            Application.Exit();
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
    }
}
