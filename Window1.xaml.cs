// Copyright Nick Polyak 2008

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

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
        public Window1()
        {
            var args = Environment.GetCommandLineArgs();
            args = args.Skip(1).ToArray();
            Files files = new Files(args);
            Resources.Add("MyFiles", files);
            InitializeComponent();

            // move for dragging
            ListViewFiles.PreviewMouseMove += new MouseEventHandler(ListViewFiles_PreviewMouseMove);
            // mouse up for selecting
            ListViewFiles.PreviewMouseUp += new MouseButtonEventHandler(ListViewFiles_OnMouseUp);
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
