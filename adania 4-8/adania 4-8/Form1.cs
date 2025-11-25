using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Numerics;
namespace adania_4_8
{
    public partial class Form1 : Form
    {
        private int totalFilesToCopy;
        private int totalCopiedFiles;
        private object progressLock = new object();
        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
            progressBar.Minimum = 0;
            progressBar.Value = 0;
            button4.Enabled = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBoxSource.Text = dlg.SelectedPath;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBoxDestination.Text = dlg.SelectedPath;
                }
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            string sourcePath = textBoxSource.Text;
            string destinationPath = textBoxDestination.Text;

            int maxThreads = 1;
            if (this.Controls.Find("numericUpDownThreads", true).Length > 0)
            {
                var numeric = this.Controls.Find("numericUpDownThreads", true).First() as NumericUpDown;
                if (numeric != null)
                {
                    maxThreads = (int)numeric.Value;
                }
            }

            if (!Directory.Exists(sourcePath))
            {
                MessageBox.Show("Исходная папка не существует.");
                return;
            }

            if (!Directory.Exists(destinationPath))
            {
                MessageBox.Show("Целевая папка не существует.");
                return;
            }

            totalCopiedFiles = 0;
            progressBar.Value = 0;

            string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            totalFilesToCopy = files.Length;

            if (totalFilesToCopy == 0)
            {
                MessageBox.Show("Нет файлов для копирования.");
                return;
            }

            cts = new CancellationTokenSource();

            button3.Enabled = false; // старт
            button4.Enabled = true; // отмена

            try
            {
                await CopyDirectoryAsync(sourcePath, destinationPath, maxThreads, cts.Token);
                MessageBox.Show("Копирование завершено успешно");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Копирование отменено");
            }
            finally
            {
                button3.Enabled = true;
                button4.Enabled = false;
            }
        }

        private async Task CopyDirectoryAsync(string sourceDir, string targetDir, int maxDegreeOfParallelism, CancellationToken token)
        {
            Directory.CreateDirectory(targetDir);

            var dirs = Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories);
            foreach (var dir in dirs)
            {
                var relativePath = GetRelativePath(sourceDir, dir);
                Directory.CreateDirectory(Path.Combine(targetDir, relativePath));
            }

            var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = new List<Task>();

            foreach (var file in files)
            {
                await semaphore.WaitAsync(token);

                string relativePath = GetRelativePath(sourceDir, file);
                string destFilePath = Path.Combine(targetDir, relativePath);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        using (var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        using (var destStream = new FileStream(destFilePath, FileMode.Create, FileAccess.Write))
                        {
                            await sourceStream.CopyToAsync(destStream, 81920, token);
                        }
                        lock (progressLock)
                        {
                            totalCopiedFiles++;
                        }
                        UpdateProgress();
                    }
                    catch (OperationCanceledException)
                    {

                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, token);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private void UpdateProgress()
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(UpdateProgress));
            }
            else
            {
                progressBar.Value = Math.Min(totalCopiedFiles, totalFilesToCopy);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            button4.Enabled = false;
        }

        public static string GetRelativePath(string basePath, string fullPath)
        {
            Uri baseUri = new Uri(AppendDirectorySeparatorChar(basePath));
            Uri fullUri = new Uri(fullPath);
            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }

        private async void buttonCalculate_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxNumber.Text, out int number) && number >= 0)
            {
                textBoxResult.Text = "Вычисление...";
                try
                {
                    BigInteger result = await CalculateFactorialAsync(number);
                    textBoxResult.Text = $"{number}! = {result}";
                }
                catch (Exception ex)
                {
                    textBoxResult.Text = $"Ошибка: {ex.Message}";
                }
            }
            else
            {
                textBoxResult.Text = "Введите корректное неотрицательное число.";
            }
        }
        private Task<BigInteger> CalculateFactorialAsync(int number)
        {
            return Task.Run(() => CalculateFactorial(number));
        }

        private BigInteger CalculateFactorial(int n)
        {
            BigInteger result = BigInteger.One;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox3.Text, out double number) &&
            int.TryParse(textBox1.Text, out int exponent))
            {
                textBox2.Text = "Вычисление...";
                try
                {
                    double result = await CalculatePowerAsync(number, exponent);
                    textBox2.Text = $"{number} ^ {exponent} = {result}";
                }
                catch (Exception ex)
                {
                    textBox2.Text = $"Ошибка: {ex.Message}";
                }
            }
            else
            {
                textBox2.Text = "Введите корректное число и степень.";
            }
        }
        private Task<double> CalculatePowerAsync(double baseNumber, int exponent)
        {
            return Task.Run(() => Math.Pow(baseNumber, exponent));
        }

        private  async void bbuttonCount_Click(object sender, EventArgs e)
        {
            string inputText = textBoxInput.Text;

            var counts = await CountCharactersAsync(inputText);

            textBoxVowels.Text = counts.Vowels.ToString();
            textBoxConsonants.Text = counts.Consonants.ToString();
            textBoxSymbols.Text = counts.Symbols.ToString();
        }
        private Task<(int Vowels, int Consonants, int Symbols)> CountCharactersAsync(string input)
        {
            return Task.Run(() =>
            {
                int vowels = 0, consonants = 0, symbols = 0;
                string vowelsLetters = "aeiouaeiouAEIOU";
                foreach (char c in input)
                {
                    if (char.IsLetter(c))
                    {
                        if (vowelsLetters.Contains(c))
                            vowels++;
                        else
                            consonants++;
                    }
                    else
                    {
                        symbols++;
                    }
                }

                return (vowels, consonants, symbols);
            });
        }
    }
}
