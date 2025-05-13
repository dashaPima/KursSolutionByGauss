using System.Data;

namespace DLLhelper
{
    public class LinearSystem
    {
        // Поля
        private double[,] _matrix;
        private double[] _constants;

        // Свойства
        public double[,] Matrix
        {
            get => _matrix;
            set => _matrix = value;
        }
        public double[] Constants
        {
            get => _constants;
            set => _constants = value;
        }
        public LinearSystem(double[,] matrix, double[] constants)
        {
            _matrix = matrix;
            _constants = constants;
        }
        // Решает систему методом Гаусса.
        public double[] SolveByGauss()
        {
            int n = _constants.Length;
            double[,] a = (double[,])_matrix.Clone();
            double[] b = (double[])_constants.Clone();
            for (int k = 0; k < n; k++)
            {
                // Прямой ход
                for (int i = k + 1; i < n; i++)
                {
                    double factor = a[i, k] / a[k, k];
                    for (int j = k; j < n; j++)
                        a[i, j] -= factor * a[k, j];
                    b[i] -= factor * b[k];
                }
            }
            // Обратный ход
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = b[i];
                for (int j = i + 1; j < n; j++)
                    sum -= a[i, j] * x[j];
                x[i] = sum / a[i, i];
            }
            return x;
        }
        public DataTable ToDataTable()
        {
            int n = _constants.Length;
            var table = new DataTable();
            for (int j = 0; j < n; j++)
                table.Columns.Add($"x{j + 1}", typeof(double));
            table.Columns.Add("Const", typeof(double));

            for (int i = 0; i < n; i++)
            {
                var row = table.NewRow();
                for (int j = 0; j < n; j++)
                    row[$"x{j + 1}"] = _matrix[i, j];
                row["Const"] = _constants[i];
                table.Rows.Add(row);
            }
            return table;
        }
        public IEnumerable<double[]> GetAnimationSteps()
        {
            int n = _constants.Length;
            double[,] a = (double[,])_matrix.Clone();
            double[] b = (double[])_constants.Clone();
            double[] x = new double[n];
            for (int k = 0; k < n; k++)
            {
                for (int i = k + 1; i < n; i++)
                {
                    double factor = a[i, k] / a[k, k];
                    for (int j = k; j < n; j++)
                        a[i, j] -= factor * a[k, j];
                    b[i] -= factor * b[k];
                }
            }
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = b[i];
                for (int j = i + 1; j < n; j++)
                    sum -= a[i, j] * x[j];
                x[i] = sum / a[i, i];

                // Отдаём копию текущего вектора решений
                yield return (double[])x.Clone();
            }
        }
        public List<Tuple<int, double>> GetGraphData(double[] solution)
        {
            var list = new List<Tuple<int, double>>();
            for (int i = 0; i < solution.Length; i++)
                list.Add(Tuple.Create(i + 1, solution[i]));
            return list;
        }
    }
}
 