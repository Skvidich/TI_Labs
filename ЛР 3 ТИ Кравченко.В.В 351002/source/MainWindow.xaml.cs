using Microsoft.Win32;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TI3;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string inputFilePath = "";
    private string outputFilePath = "";
    private ElGamalCrypto elGamal;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void SelectInputFile_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            inputFilePath = openFileDialog.FileName;
            InputFilePath.Text = inputFilePath;
        }
    }

    private void SetList_Click(object sender, RoutedEventArgs e)
    {
        
        try
        {
            int p = int.Parse(PValue.Text);
            if (!ElGamalCrypto.isPrime(p))
            {
                throw new Exception("p не простое");
            }

            int x = int.Parse(XValue.Text);
            if (!(x > 1 && x < p-1))
            {
                throw new Exception("x некорректное");
            }
            elGamal = new ElGamalCrypto(p, x);
            GList.Text = string.Join(", ", elGamal.GList);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка: " + ex.Message);
        }

    }

    private void SetY_Click(object sender, RoutedEventArgs e)
    {
        
        try
        {
            int g = int.Parse(GValue.Text);

            if (!elGamal.SetG(g))
            {
                throw new Exception("значение g не в списке");
            }
            elGamal.SetY();
            YValue.Text = elGamal.Y.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка: " + ex.Message);
        }
    }

    private void SelectOutputFile_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        if (saveFileDialog.ShowDialog() == true)
        {
            outputFilePath = saveFileDialog.FileName;
            OutputFilePath.Text = outputFilePath;
        }
    }

    private void Encrypt_Click(object sender, RoutedEventArgs e)
    {
        
    
        try
        {
            var InputStream = new MemoryStream(File.ReadAllBytes(inputFilePath));

            InputData.Text = GetPlain(InputStream);
            InputStream.Seek(0, SeekOrigin.Begin);

            
            int k = int.Parse(KValue.Text);
            if (!ElGamalCrypto.isRelativelyPrime(k, elGamal.P - 1))
            {
                throw new Exception("p-1 и k не взаимнопростые");
            }
            if (!(k > 1 && k < elGamal.P - 1))
            {
                throw new Exception("k некорректное");
            }

            var output = elGamal.EncryptFile(InputStream, k);
            OutputData.Text = GetCrypt(output);
            File.WriteAllBytes(outputFilePath, output.ToArray());
            output.Seek(0, SeekOrigin.Begin);
            MessageBox.Show("Файл зашифрован");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка: " + ex.Message);
        }
    }

    private void Decrypt_Click(object sender, RoutedEventArgs e)
    {
        var InputStream = new MemoryStream(File.ReadAllBytes(inputFilePath));

        InputData.Text = GetCrypt(InputStream);
        InputStream.Seek(0, SeekOrigin.Begin);

        try
        {
            var output = elGamal.DecryptFile(InputStream);
            OutputData.Text = GetPlain(output);
            File.WriteAllBytes(outputFilePath, output.ToArray());
            output.Seek(0, SeekOrigin.Begin);
            MessageBox.Show("Файл расшифрован");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка: " + ex.Message);
        }
    }

    private string GetPlain(MemoryStream plain)
    {
        var sb = new StringBuilder();

        for (var OutByte = plain.ReadByte(); OutByte != -1; OutByte = plain.ReadByte())
        {
                sb.Append(OutByte.ToString()+" ");
        }
        plain.Seek(0, SeekOrigin.Begin);
        return sb.ToString();

    }

    private string GetCrypt(MemoryStream crypt)
    {
        var sb = new StringBuilder();
        byte[] buffer = new byte[4];
        for ( int i = crypt.Read(buffer,0,4); i != 0; i = crypt.Read(buffer, 0, 4))
        {
            int first = BitConverter.ToInt32(buffer, 0);
            crypt.Read(buffer, 0, 4);
            int second = BitConverter.ToInt32(buffer, 0);
            sb.Append($"{{ {first}, {second} }} ");
        }
        crypt.Seek(0, SeekOrigin.Begin);
        return sb.ToString();

    }

}