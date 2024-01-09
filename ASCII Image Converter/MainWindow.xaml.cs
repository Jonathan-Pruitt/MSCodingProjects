using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ASCII_Dreaming {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow:Window {
        BitmapMaker previewImageBMP;
        public MainWindow() {
            InitializeComponent();
        }

        private void uploadBtn_Click(object sender,RoutedEventArgs e) {             
            OpenFileDialog image = new OpenFileDialog();
            bool? result = image.ShowDialog();
            if (result == true) {
                previewImageBMP = new BitmapMaker(image.FileName);
                previewImg.Source = previewImageBMP.MakeBitmap();
            } else {
                saveBtn.Visibility = Visibility.Hidden;
            }
        }//end method

        private void convertBtn_Click(object sender,RoutedEventArgs e) {
            convertBtn.Content = "Loading...";
            BitmapAscii asciiConverter = new BitmapAscii();
            outputLbl.Content = asciiConverter.Asciitize(previewImageBMP);
            saveBtn.Visibility = Visibility.Visible;
            convertBtn.Content = "Convert Image";
        }

        private void saveBtn_Click(object sender,RoutedEventArgs e) {
            string docPath = "C:\\Users\\jonat\\Documents\\Coding\\CLASS PROJECTS\\ASCII_Dreaming\\quick_images";
            StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(docPath, "AsciiDream.txt"));

            string asciiImg = outputLbl.Content.ToString();
            for (int character = 0; character < asciiImg.Length; character++) {
                outputFile.Write(asciiImg[character]);
            }
        }
    }
}
