using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MLApp;

namespace DLLhelper
{
    public class MatlabVerificationResult
    {
        public double[] Solution { get; set; }
        public string MatlabScriptPath { get; set; }
        public string ErrorMessage { get; set; }
        public bool Success => Solution != null && string.IsNullOrEmpty(ErrorMessage);
    }

    public static class MatlabLinearSystemSolver
    {
        public delegate void UpdateStatusDelegate(string message, bool isBusy);

        private static double[] ConvertMatlabDataTo1DArray(object matlabData)
        {
            if (matlabData == null) return null;

            if (matlabData is double[,] matrix)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                if (cols == 1 && rows > 0) // Вектор-столбец (N x 1)
                {
                    double[] array = new double[rows];
                    for (int i = 0; i < rows; i++)
                    {
                        array[i] = matrix[i, 0];
                    }
                    return array;
                }
                else if (rows == 1 && cols > 0) // Вектор-строка (1 x N)
                {
                    double[] array = new double[cols];
                    for (int j = 0; j < cols; j++)
                    {
                        array[j] = matrix[0, j];
                    }
                    return array;
                }
                else
                {
                    // Попытка извлечь первый столбец или строку, если матрица не вектор
                    // Это может быть полезно, если MATLAB вернул неожиданную форму, но данные все еще там
                    if (rows > 0 && cols > 0)
                    {
                        Console.Error.WriteLine("MatlabLinearSystemSolver.ConvertMatlabDataTo1DArray: Data is a 2D matrix, attempting to extract first column/row.");
                        double[] array = new double[Math.Max(rows, cols)];
                        if (rows >= cols) // Предпочитаем столбец
                        {
                            array = new double[rows];
                            for (int i = 0; i < rows; i++) array[i] = matrix[i, 0];
                            return array;
                        }
                        else // Берем строку
                        {
                            array = new double[cols];
                            for (int j = 0; j < cols; j++) array[j] = matrix[0, j];
                            return array;
                        }
                    }
                    Console.Error.WriteLine("MatlabLinearSystemSolver.ConvertMatlabDataTo1DArray: Data is not a recognized vector format.");
                    return null;
                }
            }
            else if (matlabData is double[] singleDimArray)
            {
                return singleDimArray;
            }
            else if (matlabData is object[] objArray) // Feval может вернуть object[]
            {
                if (objArray.Length > 0 && objArray[0] is double[,] innerMatrix)
                {
                    return ConvertMatlabDataTo1DArray(innerMatrix); // Рекурсивный вызов для вложенной матрицы
                }
                if (objArray.All(item => item is double)) // Если это массив double
                {
                    return objArray.Cast<double>().ToArray();
                }
            }
            Console.Error.WriteLine($"MatlabLinearSystemSolver.ConvertMatlabDataTo1DArray: Unexpected data type: {matlabData.GetType()}");
            return null;
        }

        public static async Task<MatlabVerificationResult> SolveWithMatlabAsync(
            double[,] matrixA,
            double[] vectorB,
            UpdateStatusDelegate statusUpdater = null)
        {
            if (matrixA == null) throw new ArgumentNullException(nameof(matrixA));
            if (vectorB == null) throw new ArgumentNullException(nameof(vectorB));

            int n = vectorB.Length;
            if (matrixA.GetLength(0) != n || matrixA.GetLength(1) != n)
            {
                return new MatlabVerificationResult { ErrorMessage = "Размеры матрицы A не соответствуют длине вектора B." };
            }

            statusUpdater?.Invoke("MATLAB: Подготовка скрипта...", true);

            MLApp.MLApp matlab = null; // Замените MLApp.MLApp на правильный тип после добавления COM-ссылки
            string matlabScriptPath = string.Empty;
            string matlabSolutionVariableName = "x_matlab_solution";

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string matlabScriptName = "SolveSLAU_Verification.m";
                matlabScriptPath = Path.Combine(desktopPath, matlabScriptName);

                StringBuilder scriptContent = new StringBuilder();
                scriptContent.AppendLine($"% --- {matlabScriptName} ---");
                scriptContent.AppendLine("% Generated by KyrsovRPVS application for SLAE verification");
                scriptContent.AppendLine($"%  Date: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
                scriptContent.AppendLine();

                scriptContent.AppendLine("% Coefficient Matrix A (from C#)");
                scriptContent.Append("A_cs = [");
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        scriptContent.Append(matrixA[i, j].ToString(CultureInfo.InvariantCulture));
                        if (j < n - 1) scriptContent.Append(" ");
                    }
                    if (i < n - 1) scriptContent.Append("; ");
                }
                scriptContent.AppendLine("];");

                scriptContent.AppendLine("% Constants Vector b (from C#)");
                scriptContent.Append("b_cs = [");
                for (int i = 0; i < n; i++)
                {
                    scriptContent.Append(vectorB[i].ToString(CultureInfo.InvariantCulture));
                    if (i < n - 1) scriptContent.Append("; ");
                }
                scriptContent.AppendLine("];");
                scriptContent.AppendLine();

                scriptContent.AppendLine("disp('Matrix A from C#:'); disp(A_cs);");
                scriptContent.AppendLine("disp('Vector b from C#:'); disp(b_cs);");
                scriptContent.AppendLine();

                scriptContent.AppendLine($"% Solve the system: {matlabSolutionVariableName} = A_cs \\ b_cs");
                scriptContent.AppendLine($"{matlabSolutionVariableName} = A_cs \\ b_cs;");
                scriptContent.AppendLine();

                scriptContent.AppendLine($"disp('Solution {matlabSolutionVariableName} from MATLAB:');");
                scriptContent.AppendLine($"disp({matlabSolutionVariableName});");
                //scriptContent.AppendLine($"disp(['Size {matlabSolutionVariableName}: ', num2str(size({matlabSolutionVariableName}))]);");


                File.WriteAllText(matlabScriptPath, scriptContent.ToString(), Encoding.Default); // Encoding.Default для кириллицы в комментариях если нужно
                statusUpdater?.Invoke("MATLAB: Запуск скрипта...", true);

                double[] solutionFromMatlab = null;

                await Task.Run(() =>
                {
                    try
                    {
                        Type matlabAppType = Type.GetTypeFromProgID("Matlab.Application");
                        if (matlabAppType == null) matlabAppType = Type.GetTypeFromProgID("MLApp.MLApp"); // Для старых версий

                        if (matlabAppType == null)
                            throw new InvalidOperationException("Не удалось найти ProgID для MATLAB. Убедитесь, что MATLAB установлен.");

                        matlab = (MLApp.MLApp)Activator.CreateInstance(matlabAppType);
                        if (matlab == null)
                            throw new InvalidOperationException("Не удалось создать экземпляр MATLAB.");

                        // matlab.Visible = 0; // Можно сделать MATLAB невидимым

                        string scriptDirectory = Path.GetDirectoryName(matlabScriptPath);
                        string scriptFileNameOnly = Path.GetFileNameWithoutExtension(matlabScriptPath);

                        string cdCommand = $"cd '{scriptDirectory.Replace("'", "''")}'";
                        string runCommand = $"run('{scriptFileNameOnly.Replace("'", "''")}')";

                        matlab.Execute(cdCommand);
                        matlab.Execute(runCommand);

                       
                        object resultData;
                        matlab.GetWorkspaceData(matlabSolutionVariableName, "base", out resultData);

                        solutionFromMatlab = ConvertMatlabDataTo1DArray(resultData);

                        if (solutionFromMatlab == null)
                        {
                            string matlabOutput = matlab.Execute($"disp('Переменная {matlabSolutionVariableName} не найдена или имеет неверный формат.'); lasterror");
                            Console.Error.WriteLine("MATLAB output on error: " + matlabOutput);
                            throw new Exception($"Не удалось получить или преобразовать данные переменной '{matlabSolutionVariableName}' из MATLAB. Тип данных: {resultData?.GetType().ToString() ?? "null"}");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Ошибка во время выполнения MATLAB: {ex.Message}", ex);
                    }
                    finally
                    {
                        if (matlab != null)
                        {
                            try
                            {
                                matlab.Quit();
                            }
                            catch { /* Игнорировать ошибки при закрытии */ }
                            Marshal.ReleaseComObject(matlab);
                            matlab = null;
                        }
                    }
                });

                statusUpdater?.Invoke("MATLAB: Готово.", false);
                return new MatlabVerificationResult
                {
                    Solution = solutionFromMatlab,
                    MatlabScriptPath = matlabScriptPath
                };
            }
            catch (Exception ex)
            {
                statusUpdater?.Invoke("MATLAB: Ошибка.", false);
                Console.Error.WriteLine($"Ошибка при взаимодействии с MATLAB: {ex.ToString()}");
                return new MatlabVerificationResult
                {
                    ErrorMessage = $"Ошибка MATLAB: {ex.Message}\nПуть к скрипту: {matlabScriptPath}",
                    MatlabScriptPath = matlabScriptPath
                };
            }
        }
    }
}