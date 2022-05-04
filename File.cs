// copyright Nick Polyak 2008

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace DragDropTest
{
    public class File
    {
        public string FileName { get; set; }
        public ImageSource PngIcon
        {
            get {
                try
                {
                    var icon = Icon.ExtractAssociatedIcon(FileName);
                    Bitmap bmp = icon.ToBitmap();
                    var stream = new System.IO.MemoryStream();
                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return System.Windows.Media.Imaging.BitmapFrame.Create(stream);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        public File()
        {
        }

        public File(string filename)
        {
            FileName = filename;
        }

    }

    public class Files : ObservableCollection<File>
    {
        public Files()
        {
        }

        public Files(string[] filenames)
        {
            foreach (string filename in filenames)
            {
                if (System.IO.File.Exists(filename) || System.IO.Directory.Exists(filename))
                {
                    Add(new File(filename));
                } 
                else
                {
                    System.Console.WriteLine("File does not exist when starting:");
                    System.Console.WriteLine(filename);
                }
            }
        }
    }
}