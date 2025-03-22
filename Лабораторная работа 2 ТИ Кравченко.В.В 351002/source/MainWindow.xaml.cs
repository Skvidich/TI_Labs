using Microsoft.Win32;
using System.IO;
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

namespace lab2TI;



public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    const int RegisterSize =35;
    const int BaseLength = 20;
    private MemoryStream InputStream;
    private MemoryStream OutputStream;
    private MemoryStream KeyStream;
    private int InputBitCount;

    private static void ShowError(string text)
    {
        MessageBox.Show(
                text,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.DefaultDesktopOnly);
    }

    private bool GetKey()
    {
        if (chbAutofill.IsChecked != true)
        {
            if (tbRegisterState.Text.Length != RegisterSize)
            {
                ShowError("State don`t match size of regester.\n Enter right count of bits or choose autofill option");
                return false;
            }
        } 
        else
        {
            if (tbRegisterState.Text.Length < RegisterSize)
            {
                var strBuild = new StringBuilder(tbRegisterState.Text);
                char ch;
                if (rbtnFill0.IsChecked != true && rbtnFill1.IsChecked != true)
                {
                    ShowError("Autofill option isn't choosed.\n Specify value of autofill: 1 or 0");
                    return false;
                }
                else
                {
                    if (rbtnFill0.IsChecked == true)
                    {
                        ch = '0';
                    }
                    else
                    {
                        ch = '1';
                    }
                    strBuild.Append(new String(ch, RegisterSize - tbRegisterState.Text.Length));
                    tbRegisterState.Text = strBuild.ToString();
                }
            }
            else
            {
                tbRegisterState.Text = tbRegisterState.Text.Substring(0, RegisterSize);
            }
            


        }
        
        if (InputStream==null )
        {
                ShowError("Input file isn't choosed");
                return false;   
        }

        var pos = new List<byte>() { 34, 1 };
        var state = new List<byte>() ;
        var rawState = tbRegisterState.Text;
        for (int i = 0; i < RegisterSize; i++)
        {
            state.Add(Byte.Parse(rawState[i].ToString()));
        }

        var reg = new Register(RegisterSize, pos, state);
        
        KeyStream = new MemoryStream();

        int ind = 0;
        byte temp = 0;
        for (;KeyStream.Length!=InputStream.Length ; ){
            byte val = reg.DoShift();
            
            if (ind < 8)
            {
                temp <<= 1;
                temp += val;
                ind++;

            } else
            {
                KeyStream.WriteByte(temp);
                ind = 0;
                temp = 0;
            }
        }



        

       
        var keySize = Math.Min(InputBitCount, RegisterSize * 2);
       
        var rawKey = new StringBuilder(keySize);
        KeyStream.Seek(0, SeekOrigin.Begin);
        for (var i = 0; i < keySize/8; i++)
        {
            var val = KeyStream.ReadByte();
            for (var j = 0; j < 8; j++)
            {   
                rawKey.Append(((val & 0x80) / 0x80).ToString());
                val <<= 1;
            }
            
        }
        
        tbKeyValue.Text = rawKey.ToString();
        KeyStream.Seek(0, SeekOrigin.Begin);
        return true;
    }

    private void CryptProcess(object sender, RoutedEventArgs e)
    {
        if (InputStream == null)
        {
            ShowError("Input file isn't choosed");
            return;
        }

        if (!GetKey())
        {
            return;
        }

        OutputStream = new MemoryStream();
        int InByte = InputStream.ReadByte();
        int KeyByte = KeyStream.ReadByte();
        for (; InByte != -1;)
        {
            byte OutByte = 0;
            for (int i = 0; i < 8; i++)
            {
                OutByte <<= 1;
                var inVal = (InByte & 0x80) / 0x80;
                var keyVal = (KeyByte & 0x80) / 0x80;
                OutByte += (byte)(inVal ^ keyVal);
                InByte <<= 1;
                KeyByte <<= 1;
            }
            OutputStream.WriteByte(OutByte);
            InByte = InputStream.ReadByte();
            KeyByte = KeyStream.ReadByte();

        }
        KeyStream.Seek(0, SeekOrigin.Begin);
        InputStream.Seek(0, SeekOrigin.Begin);
        OutputStream.Seek(0, SeekOrigin.Begin);


        tbOutputFile.Text = GetSomeText(OutputStream);
       
        OutputStream.Seek(0, SeekOrigin.Begin);

    }

    private void TbRegisterState_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        
        foreach (char c in e.Text)
        {
            if (c != '0' && c != '1')
            {
                e.Handled = true; 
                return;
            }
        }
    }

    
    private void TbRegisterState_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            foreach (char c in text)
            {
                if (c != '0' && c != '1')
                {
                    e.CancelCommand(); 
                    return;
                }
            }
        }
        else
        {
            e.CancelCommand(); 
        }
    }
    private void SaveOutputFile(object sender, RoutedEventArgs e)
    {
        if (OutputStream == null)
        {
           
                ShowError("Output file isn't generated. Nothing to save");
                return;
           
        }
        string FilePath;
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() != true) { return; }
        FilePath = openFileDialog.FileName;

        File.WriteAllBytes(FilePath, OutputStream.ToArray());
        OutputStream.Seek(0, SeekOrigin.Begin);


    }
    private void GetInputFile(object sender, RoutedEventArgs e)
    {
        string FilePath;
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() != true) { return; }
        string InputFileName = openFileDialog.FileName;

        InputStream = new MemoryStream(File.ReadAllBytes(InputFileName));
        
        tbInputFile.Text = GetSomeText(InputStream);
        InputStream.Seek(0, SeekOrigin.Begin);
        InputBitCount =  (int)(InputStream.Length)*8;
    }
    
    private string GetSomeText(MemoryStream stream )
    {
        StringBuilder BinaryText = new StringBuilder();

        if (stream.Length <= BaseLength * 2)
        {

            for (var OutByte = stream.ReadByte(); OutByte != -1; OutByte = stream.ReadByte())
            {

                for (int i = 0; i < 8; i++)
                {

                    BinaryText.Append(((OutByte & 0x80) / 0x80).ToString());
                    OutByte <<= 1;
                }
            }
        }
        else
        {
            for (var j = 0; j < BaseLength; j++)
            {
                var OutByte = stream.ReadByte();
                for (int i = 0; i < 8; i++)
                {

                    BinaryText.Append(((OutByte & 0x80) / 0x80).ToString());
                    OutByte <<= 1;
                }
            }

            BinaryText.Append(".......");

            stream.Seek(-BaseLength, SeekOrigin.End);

            for (var j = 0; j < BaseLength; j++)
            {
                var OutByte = stream.ReadByte();
                for (int i = 0; i < 8; i++)
                {

                    BinaryText.Append(((OutByte & 0x80) / 0x80).ToString());
                    OutByte <<= 1;
                }
            }

        }
        stream.Seek(0, SeekOrigin.Begin);
        return BinaryText.ToString();
    }
    
    /*private string GetSomeText(MemoryStream stream)
    {
        StringBuilder BinaryText = new StringBuilder();

        
        

            for (var OutByte = stream.ReadByte(); OutByte != -1; OutByte = stream.ReadByte())
            {

                for (int i = 0; i < 8; i++)
                {

                    BinaryText.Append(((OutByte & 0x80) / 0x80).ToString());
                    OutByte <<= 1;
                }
            }
        return BinaryText.ToString();
    }*/

}