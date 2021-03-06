// Copyright Nick Polyak 2008

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.IO;

namespace DragonWindows
{
    // delegate for GetPosition function of DragEventArgs and
    // MouseButtonEventArgs event argument objects. This delegate is used to reuse the code
    // for processing both types of events.
    delegate Point GetPositionDelegate(IInputElement element);

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private bool andExit = false;
        private bool help = false;
        private string helpText = @"
Usage:
    DragonWindows.exe [--and-exit] [-x] filename [filename2] ...

Arguments:
    --and-axit / -x: if specified, quit immediately after dragging done
";

        public Window1()
        {
            var args = Environment.GetCommandLineArgs();
            // skip the first argument, which is the self's filename
            args = args.Skip(1).ToArray();

            var filenames = new List<string>();
            foreach (var arg in args)
            {
                if (arg == "--help" || arg == "-h")
                {
                    help = true;
                } 
                else if (arg == "--and-exit" || arg == "-x") {
                    andExit = true;
                } 
                else
                {
                    filenames.Add(arg);
                }
            }

            // glob matching
            var matcher = new Matcher();
            matcher.AddIncludePatterns(filenames);
            PatternMatchingResult result = matcher.Execute(
                new DirectoryInfoWrapper(new DirectoryInfo(".")));
            filenames = result.Files.Select(file => file.Path).ToList();

            // check if filenames available
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            var fullFilenames = new List<String>();
            foreach (var filename in filenames)
            {
                if (System.IO.File.Exists(filename) || System.IO.Directory.Exists(filename))
                {
                    string fullFilename = System.IO.Path.GetFullPath(filename);
                    fullFilenames.Add(fullFilename);
                }
                else
                {
                    System.Console.WriteLine("File does not exist when starting:");
                    System.Console.WriteLine(filename);
                }
            }

            if (help)
            {
                Resources.Add("HelpText", helpText);
                Resources.Add("MyFiles", null);
                // only help visible
                Resources.Add("MainWindowVisibility", "hidden");
                Resources.Add("HelpVisibility", "visible");
            }
            else
            {
                Resources.Add("HelpText", null);
                // only files visible
                Resources.Add("MainWindowVisibility", "visible");
                Resources.Add("HelpVisibility", "hidden");

                Files files = new Files(fullFilenames.ToArray());
                Resources.Add("MyFiles", files);

                if (fullFilenames.Count() == 0)
                {
                    System.Console.WriteLine("No valid filename is given!");
                    this.Close();
                }
            }
            InitializeComponent();

            // Esc to quit
            this.KeyDown += new KeyEventHandler(Escape);

            // move for dragging
            ListViewFiles.PreviewMouseMove += new MouseEventHandler(ListViewFiles_PreviewMouseMove);
            // mouse up for selecting
            ListViewFiles.PreviewMouseUp += new MouseButtonEventHandler(ListViewFiles_OnMouseUp);
        }

        protected void Escape(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        protected void ListViewFiles_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            // select item when mouse up
            if (e.ChangedButton == MouseButton.Left)
            {
                int index = this.GetCurrentIndex(e.GetPosition);
                var items = ListViewFiles.Items;
                if (index >= 0 && index <= items.Count)
                {
                    var item = (File)items[index];
                    // reverse selection state
                    item.IsSelected = !item.IsSelected;
                }
            }
        }

        // disable default mouse down selection behavior of ListViewFiles
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void ListViewFiles_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // if mouse is not pressed, just return
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            int index = this.GetCurrentIndex(e.GetPosition);

            // get files for dragging
            List<string> filenames = new List<string>();
            // put the dragged file
            var fileItems = ListViewFiles.Items;
            // select current file, according to index
            if (index >= 0 && index < fileItems.Count)
            {
                var currentFile = (File)fileItems[index];
                currentFile.IsSelected = true;
            }
            // put all selected files into list
            foreach (File file in fileItems)
            {
                // if other file selected, also put in
                // only drag selected files
                if (!filenames.Contains(file.FileName) && file.IsSelected)
                {
                    filenames.Add(file.FileName);
                }
            }

            // filter files that exist
            List<string> existentFilenames = new List<string>();
            foreach (string filename in filenames)
            {
                if (System.IO.File.Exists(filename) || System.IO.Directory.Exists(filename))
                {
                    existentFilenames.Add(filename);
                } 
                else
                {
                    System.Console.WriteLine("File does not exist when dragging:");
                    System.Console.WriteLine(filename);
                }
            }
            // drag!
            var data = new DataObject(DataFormats.FileDrop, filenames.ToArray());
            DragDrop.DoDragDrop(this.ListViewFiles, data, DragDropEffects.Copy);
            if (andExit)
            {
                this.Close();
            }
        }

        ListViewItem GetListViewItem(int index)
        {
            if (ListViewFiles.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return ListViewFiles.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        // returns the index of the item in the ListView
        int GetCurrentIndex(GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0; i < this.ListViewFiles.Items.Count; ++i)
            {
                ListViewItem item = GetListViewItem(i);
                if (this.IsMouseOverTarget(item, getPosition))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        bool IsMouseOverTarget( Visual target, GetPositionDelegate getPosition)
		{
			Rect bounds = VisualTreeHelper.GetDescendantBounds( target );
			Point mousePos = getPosition((IInputElement) target);
			return bounds.Contains( mousePos );
		}
    }
}
