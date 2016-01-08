using System;
using System.Collections.Generic;
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
using FTD2XX_NET;
using System.Threading;
using USB_I2C;
using NationalInstruments.Visa;


namespace WpfApplication3
{

	/// <summary>
	/// Helper methods for UI-related tasks.
	/// </summary>
	public static class UIHelper
	{
		/// <summary>
		/// Finds a parent of a given item on the visual tree.
		/// </summary>
		/// <typeparam name="T">The type of the queried item.</typeparam>
		/// <param name="child">A direct or indirect child of the
		/// queried item.</param>
		/// <returns>The first parent item that matches the submitted
		/// type parameter. If not matching item can be found, a null
		/// reference is being returned.</returns>
		public static T TryFindParent<T>(DependencyObject child)
		  where T : DependencyObject
		{
			//get parent item
			DependencyObject parentObject = GetParentObject(child);

			//we've reached the end of the tree
			if (parentObject == null) return null;

			//check if the parent matches the type we're looking for
			T parent = parentObject as T;
			if (parent != null)
			{
				return parent;
			}
			else
			{
				//use recursion to proceed with next level
				return TryFindParent<T>(parentObject);
			}
		}

		/// <summary>
		/// This method is an alternative to WPF's
		/// <see cref="VisualTreeHelper.GetParent"/> method, which also
		/// supports content elements. Do note, that for content element,
		/// this method falls back to the logical tree of the element!
		/// </summary>
		/// <param name="child">The item to be processed.</param>
		/// <returns>The submitted item's parent, if available. Otherwise
		/// null.</returns>
		public static DependencyObject GetParentObject(DependencyObject child)
		{
			if (child == null) return null;
			ContentElement contentElement = child as ContentElement;

			if (contentElement != null)
			{
				DependencyObject parent = ContentOperations.GetParent(contentElement);
				if (parent != null) return parent;

				FrameworkContentElement fce = contentElement as FrameworkContentElement;
				return fce != null ? fce.Parent : null;
			}

			//if it's not a ContentElement, rely on VisualTreeHelper
			return VisualTreeHelper.GetParent(child);
		}
	}




	/// <summary>
	/// MainWindow.xaml 的互動邏輯
	/// </summary>
	public partial class MainWindow : Window
    {
		I2C usb_i2c;

		public MainWindow()
        {
            InitializeComponent();

			byte[] buf = { 0xC0, 0x11, 0xA0, 0xA5, 0xA4, 0xA3 };

			usb_i2c = new I2C();
			usb_i2c.I2C_instance();

			usb_i2c.usb_init();

			usb_i2c.IIC_Wrtie(buf, 6);

			//usb_i2c.ClosePort();




/*

            UInt32 ftdiDeviceCount = 0;
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

            // Create new instance of the FTDI device class
            FTDI myFtdiDevice = new FTDI();


            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            // Check status
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine("Number of FTDI devices: " + ftdiDeviceCount.ToString());
                Console.WriteLine("");
            }
            else
            {
                // Wait for a key press
                Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }



            // If no devices available, return
            if (ftdiDeviceCount == 0)
            {
                // Wait for a key press
                Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Allocate storage for device info list
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

            // Populate our device list
            ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);

            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                for (UInt32 i = 0; i < ftdiDeviceCount; i++)
                {
                    Console.WriteLine("Device Index: " + i.ToString());
                    Console.WriteLine("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
                    Console.WriteLine("Type: " + ftdiDeviceList[i].Type.ToString());
                    Console.WriteLine("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
                    Console.WriteLine("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                    Console.WriteLine("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
                    Console.WriteLine("Description: " + ftdiDeviceList[i].Description.ToString());
                    Console.WriteLine("");
                }
            }


            // Open first device in our list by serial number
            ftStatus = myFtdiDevice.OpenBySerialNumber(ftdiDeviceList[0].SerialNumber);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to open device (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Set up device data parameters
            // Set Baud rate to 9600
            ftStatus = myFtdiDevice.SetBaudRate(9600);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set Baud rate (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Set data characteristics - Data bits, Stop bits, Parity
            ftStatus = myFtdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set data characteristics (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Set flow control - set RTS/CTS flow control
            ftStatus = myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, 0x11, 0x13);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set flow control (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Set read timeout to 5 seconds, write timeout to infinite
            ftStatus = myFtdiDevice.SetTimeouts(5000, 0);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set timeouts (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Perform loop back - make sure loop back connector is fitted to the device
            // Write string data to the device
            string dataToWrite = "Hello world!";
            UInt32 numBytesWritten = 0;
            // Note that the Write method is overloaded, so can write string or byte array data
            ftStatus = myFtdiDevice.Write(dataToWrite, dataToWrite.Length, ref numBytesWritten);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to write to device (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }


            // Check the amount of data available to read
            // In this case we know how much data we are expecting, 
            // so wait until we have all of the bytes we have sent.
            UInt32 numBytesAvailable = 0;
            do
            {
                ftStatus = myFtdiDevice.GetRxBytesAvailable(ref numBytesAvailable);

                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    // Wait for a key press
                    Console.WriteLine("Failed to get number of bytes available to read (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }
                Thread.Sleep(10);
            } while (numBytesAvailable < dataToWrite.Length);

            // Now that we have the amount of data we want available, read it
            string readData;
            UInt32 numBytesRead = 0;
            // Note that the Read method is overloaded, so can read string or byte array data
            ftStatus = myFtdiDevice.Read(out readData, numBytesAvailable, ref numBytesRead);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to read data (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }
            Console.WriteLine(readData);

            // Close our device
            ftStatus = myFtdiDevice.Close();

            //if (myFtdiDevice.FT_Open(0, &ftHandle) != FT_OK)
            // return ReplyFail;
*/

        }


		private void button_Click(object sender, RoutedEventArgs e)
		{
			////ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
			//DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
			////CheckBox target = (CheckBox)myDataTemplate.FindName("chkUniqueId", myContentPresenter);
			//CheckBox target = (CheckBox)myDataTemplate.FindName("chkUniqueId", this);

			//TextBox foundTextBox =
			//	UIHelper.FindChild<TextBox>(Application.Current.MainWindow, "myTextBoxName");

			//if (target.IsChecked)
			//{
			//	target.IsChecked = false;
			//}
			//else
			//{
			//	target.IsChecked = true;

			//}
			byte[] dataBuffer = new byte[100];
			usb_i2c.tcon_rd(0xC0, 0x11A0, dataBuffer, 10);


			var myTextBlock = (TextBlock)this.FindName("myTextBlock");
			myTextBlock.Text = "1234";

			var myTextBox = (TextBox)this.FindName("myTextBox");

			string s = "";
			for(int i=0; i<10; i++)
			{
				s += String.Format("addr=0x{0:X4} = 0x{1:X2}\n", 0x11a0 + i, (int) dataBuffer[i]);
			}
			myTextBox.Text = s;





			Window mainWindow = new Window();
			mainWindow.Title = "Grid Sample";

			// Create the Grid
			Grid myGrid = new Grid();
			myGrid.Width = 250;
			myGrid.Height = 100;
			myGrid.HorizontalAlignment = HorizontalAlignment.Left;
			myGrid.VerticalAlignment = VerticalAlignment.Top;
			myGrid.ShowGridLines = true;

			// Define the Columns
			ColumnDefinition colDef1 = new ColumnDefinition();
			ColumnDefinition colDef2 = new ColumnDefinition();
			ColumnDefinition colDef3 = new ColumnDefinition();
			myGrid.ColumnDefinitions.Add(colDef1);
			myGrid.ColumnDefinitions.Add(colDef2);
			myGrid.ColumnDefinitions.Add(colDef3);

			// Define the Rows
			RowDefinition rowDef1 = new RowDefinition();
			RowDefinition rowDef2 = new RowDefinition();
			RowDefinition rowDef3 = new RowDefinition();
			RowDefinition rowDef4 = new RowDefinition();
			myGrid.RowDefinitions.Add(rowDef1);
			myGrid.RowDefinitions.Add(rowDef2);
			myGrid.RowDefinitions.Add(rowDef3);
			myGrid.RowDefinitions.Add(rowDef4);

			// Add the first text cell to the Grid
			TextBlock txt1 = new TextBlock();
			txt1.Text = "2005 Products Shipped";
			txt1.FontSize = 20;
			txt1.FontWeight = FontWeights.Bold;
			Grid.SetColumnSpan(txt1, 3);
			Grid.SetRow(txt1, 0);

			// Add the second text cell to the Grid
			TextBlock txt2 = new TextBlock();
			txt2.Text = "Quarter 1";
			txt2.FontSize = 12;
			txt2.FontWeight = FontWeights.Bold;
			Grid.SetRow(txt2, 1);
			Grid.SetColumn(txt2, 0);

			// Add the third text cell to the Grid
			TextBlock txt3 = new TextBlock();
			txt3.Text = "Quarter 2";
			txt3.FontSize = 12;
			txt3.FontWeight = FontWeights.Bold;
			Grid.SetRow(txt3, 1);
			Grid.SetColumn(txt3, 1);

			// Add the fourth text cell to the Grid
			TextBlock txt4 = new TextBlock();
			txt4.Text = "Quarter 3";
			txt4.FontSize = 12;
			txt4.FontWeight = FontWeights.Bold;
			Grid.SetRow(txt4, 1);
			Grid.SetColumn(txt4, 2);

			// Add the sixth text cell to the Grid
			TextBlock txt5 = new TextBlock();
			Double db1 = new Double();
			db1 = 50000;
			txt5.Text = db1.ToString();
			Grid.SetRow(txt5, 2);
			Grid.SetColumn(txt5, 0);

			// Add the seventh text cell to the Grid
			TextBlock txt6 = new TextBlock();
			Double db2 = new Double();
			db2 = 100000;
			txt6.Text = db2.ToString();
			Grid.SetRow(txt6, 2);
			Grid.SetColumn(txt6, 1);

			// Add the final text cell to the Grid
			TextBlock txt7 = new TextBlock();
			Double db3 = new Double();
			db3 = 150000;
			txt7.Text = db3.ToString();
			Grid.SetRow(txt7, 2);
			Grid.SetColumn(txt7, 2);

			// Total all Data and Span Three Columns
			TextBlock txt8 = new TextBlock();
			txt8.FontSize = 16;
			txt8.FontWeight = FontWeights.Bold;
			txt8.Text = "Total Units: " + (db1 + db2 + db3).ToString();
			Grid.SetRow(txt8, 3);
			Grid.SetColumnSpan(txt8, 3);

			// Add the TextBlock elements to the Grid Children collection
			myGrid.Children.Add(txt1);
			myGrid.Children.Add(txt2);
			myGrid.Children.Add(txt3);
			myGrid.Children.Add(txt4);
			myGrid.Children.Add(txt5);
			myGrid.Children.Add(txt6);
			myGrid.Children.Add(txt7);
			myGrid.Children.Add(txt8);

			// Add the Grid as the Content of the Parent Window Object
			mainWindow.Content = myGrid;
			mainWindow.Show();

		}
	}

}
