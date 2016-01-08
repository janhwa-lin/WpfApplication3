using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTD2XX_NET;
using System.Threading;
using System.Windows;

namespace USB_I2C
{
	public partial class I2C
	{
		public void M(string s)
		{
			Console.WriteLine(s);
		}
		UInt32 dwNumBytesToSends = 0;
		UInt32 numBytesWritten = 0;
		UInt32 numBytesRead = 0;
		byte[] dataBuffer = new byte[3000];
		FTDI.FT_STATUS status = FTDI.FT_STATUS.FT_OK;
		FTDI ftdev;
		bool m_ExclusiveMode = false;
		bool m_MidCmd = false;

		public void I2C_instance()
		{
			ftdev = new FTDI();
		}

		public bool usb_init()
		{

			if (ftdev.ftHandle != IntPtr.Zero)
				return true;

			status = ftdev.OpenByIndex(0);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				M("Error ! can't open device.");
				MessageBox.Show("Failed to open device.");

				return false;
			}

			status = ftdev.ResetDevice();

			UInt32 numByteAvailable = 0;
			status = ftdev.GetRxBytesAvailable(ref numByteAvailable);

			UInt32 numByteRead = 0;
			//byte[] dataBuffer = new byte[3000];
			if (numByteAvailable != 0)
				ftdev.Read(dataBuffer, numByteAvailable, ref numByteRead);


			// set packet size
			ftdev.InTransferSize(65536);
			ftdev.SetCharacters(0, false, 0, false);

			ftdev.SetLatency(16);

			ftdev.SetBitMode(0x00, 0x00);
			ftdev.SetBitMode(0x00, 0x02);
			Thread.Sleep(50);

			dwNumBytesToSends = 0;
			dataBuffer[dwNumBytesToSends++] = 0x85;
			numBytesWritten = 0;
			ftdev.Write(dataBuffer, dwNumBytesToSends, ref numBytesWritten);

			dwNumBytesToSends = 0;
			// UInt32 m_I2cFreq = 400000;
			UInt32 m_I2cFreq = 400;
			UInt32 clk_low_int = (6000 / (m_I2cFreq + 1)) % 0x100;
			UInt32 clk_hi_int = (6000 / (m_I2cFreq + 1)) / 0x100;
			byte clk_low_byte = Convert.ToByte(clk_low_int);
			byte clk_hi_byte = Convert.ToByte(clk_hi_int);
			dataBuffer[dwNumBytesToSends++] = FTDI.MPSSE_SET_CLOCK_SPEED;
			dataBuffer[dwNumBytesToSends++] = clk_low_byte;
			dataBuffer[dwNumBytesToSends++] = clk_hi_byte;

			dataBuffer[dwNumBytesToSends++] = FTDI.MPSSE_SET_DATA_BITS_LOW_BYTE;
			dataBuffer[dwNumBytesToSends++] = 0x03;
			dataBuffer[dwNumBytesToSends++] = 0x03;
			status = ftdev.Write(dataBuffer, dwNumBytesToSends, ref numBytesWritten);

			if (status != FTDI.FT_STATUS.FT_OK)
			{
				M("write setting fail !");
				return false;
			}

			//MPSSE_SendStart();
			//MPSSE_SendStop();

			//if (!m_MidCmd)
			//ClosePort();


			return true;

		}
		public bool usb_init(bool MidCmd)
		{
			m_MidCmd = MidCmd;
			return usb_init();
		}

		public void MPSSE_SendStart()
		{
			//if (!m_ExclusiveMode)
			if (!ftdev.IsOpen)
				usb_init(true);

			byte[] StartCmd = new byte[]
			{
				 0x80,0x01,0x03, 0x80,0x01,0x03, 0x80,0x01,0x03, 0x80,0x01,0x03,
				0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03,
			};

			dwNumBytesToSends = 0;
			for (int i = 0; i < 24; i++)
				dataBuffer[dwNumBytesToSends++] = StartCmd[i];


		}

		public void MPSSE_SendStop()
		{
			byte[] StopCmd = new byte[] {
				0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03,
				0x80,0x01,0x03, 0x80,0x01,0x03, 0x80,0x01,0x03, 0x80,0x01,0x03,
				0x80,0x03,0x03, 0x80,0x03,0x03, 0x80,0x03,0x03, 0x80,0x03,0x03,
			};

			for (int i = 0; i < 36; i++)
				dataBuffer[dwNumBytesToSends++] = StopCmd[i];

			ftdev.Write(dataBuffer, dwNumBytesToSends, ref numBytesWritten);
			dwNumBytesToSends = 0;

			if (!m_ExclusiveMode)
				ClosePort();

		}

		public void ClosePort()
		{
			if (ftdev.ftHandle != IntPtr.Zero)
			{
				//ftdev.Close();
				ftdev.CyclePort();
				//ftdev.ftHandle = IntPtr.Zero;
			}
			m_MidCmd = false;
			m_ExclusiveMode = false;
		}

		public bool MPSSE_ReadByte(byte[] data, int Length)
		{

			for (UInt32 i = 0; i < Length; i++)
			{
				dataBuffer[dwNumBytesToSends++] = FTDI.MPSSE_CLOCK_DATA_BYTES_IN_RISE_EDGE;
				dataBuffer[dwNumBytesToSends++] = 0;
				dataBuffer[dwNumBytesToSends++] = 0;
				dataBuffer[dwNumBytesToSends++] = FTDI.MPSSE_CLOCK_DATA_BITS_OUT_FALL_EDGE;
				dataBuffer[dwNumBytesToSends++] = 0x00;

				if (i != (Length - 1))
					dataBuffer[dwNumBytesToSends++] = 0x00; // ACK
				else
					dataBuffer[dwNumBytesToSends++] = 0xFF;     // NACK

				dataBuffer[dwNumBytesToSends++] = 0x80;
				dataBuffer[dwNumBytesToSends++] = 0x02;
				dataBuffer[dwNumBytesToSends++] = 0x03;
				dataBuffer[dwNumBytesToSends++] = 0x87;
			}

			ftdev.Write(dataBuffer, dwNumBytesToSends, ref numBytesWritten);
			dwNumBytesToSends = 0;
			status = ftdev.Read(dataBuffer, (uint) Length, ref numBytesRead);

			for (int i = 0; i < Length; i++)
				data[i] = dataBuffer[i];

			return FTDI.ACK_;
		}

		public bool MPSSE_SendBytes(byte[] data, UInt32 Length, bool CheckAck)
		{
			//UInt32  idx = dwNumBytesToSends;
			bool ack = false;

			for (UInt32 i = 0; i < Length; i++)
			{
				dataBuffer[dwNumBytesToSends++] = FTDI.MPSSE_CLOCK_DATA_BYTES_OUT_FALL_EDGE;
				dataBuffer[dwNumBytesToSends++] = 0;
				dataBuffer[dwNumBytesToSends++] = 0;
				dataBuffer[dwNumBytesToSends++] = data[i];

				if (!CheckAck)
				{
					// don't check ACK
					dataBuffer[dwNumBytesToSends++] = FTDI.MPSSE_CLOCK_DATA_BITS_OUT_FALL_EDGE;
					dataBuffer[dwNumBytesToSends++] = 0;
					dataBuffer[dwNumBytesToSends++] = 0xFF;
					dataBuffer[dwNumBytesToSends++] = FTDI.MPSSE_SET_DATA_BITS_LOW_BYTE;
					dataBuffer[dwNumBytesToSends++] = 0x02;   // set scl low
					dataBuffer[dwNumBytesToSends++] = 0x03;   // set sda in 
					ack = FTDI.ACK_;
				}
				else
				{
					// check ACK
					dataBuffer[dwNumBytesToSends++] = FTDI.MPSSE_SET_DATA_BITS_LOW_BYTE;
					dataBuffer[dwNumBytesToSends++] = 0x06;
					dataBuffer[dwNumBytesToSends++] = 0x03;
					dataBuffer[dwNumBytesToSends++] = FTDI.MPSSE_CLOCK_DATA_BITS_IN_RISE_EDGE;
					dataBuffer[dwNumBytesToSends++] = 0x00;   // read 1 bit
					dataBuffer[dwNumBytesToSends++] = 0x087;   // read now

					ftdev.Write(dataBuffer, dwNumBytesToSends, ref numBytesWritten);
					dwNumBytesToSends = 0;
					ftdev.Read(dataBuffer, 1, ref numBytesRead);


					byte value_byte = dataBuffer[0];
					int value = (int)value_byte;
					if ((value & 0x01) == 0)
						ack = FTDI.ACK_;
					else
					{
						ack = FTDI.NACK;
						break;
					}
				}

			}
			return ack;
		}

		public void MPSSE_SendRepeatStart()
		{

			byte[] RepeatStartCmd = {
				0x80,0x03,0x03, 0x80,0x03,0x03, 0x80,0x03,0x03, 0x80,0x03,0x03,
				0x80,0x01,0x03, 0x80,0x01,0x03, 0x80,0x01,0x03, 0x80,0x01,0x03,
				0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03,
				0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03,
				0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03,
				0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03,
				0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03,
				0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03,
				0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03, 0x80,0x00,0x03,
			};
			for (int i = 0; i < 108; i++)
				dataBuffer[dwNumBytesToSends++] = RepeatStartCmd[i];

		}

		public bool tcon_rd(int nSlave, int nAddr, byte[] dataBuffer, int ReadNum)
		{
			bool Ack;
			Ack = IIC_Read(nSlave, nAddr, 2,  dataBuffer, ReadNum);
			return Ack;
		}

		public bool IIC_Read(int nSlave, int  nAddr, uint nAddrNum, byte[] data, int ReadNum)
		{
			byte[] buf = new byte[10];
			bool Ack;
			int i;
			MPSSE_SendStart();

			if (nAddrNum == 0)
			{
				buf[0] = (byte)(nSlave | 0x01);
				Ack = MPSSE_SendBytes(buf, 1, false);
				ftdev.Write(dataBuffer, (int)dwNumBytesToSends, ref numBytesWritten);
				dwNumBytesToSends = 0;
				Thread.Sleep(1);
				Ack |= MPSSE_ReadByte(data, ReadNum);
			}
			else
			{
				buf[0] = (byte)nSlave;
				if (nAddrNum == 2)
				{
					buf[1] = (byte) ((nAddr >> 8) & 0xFF);
					buf[2] = (byte) (nAddr  & 0xFF);
				}
				else
					buf[1] = (byte) (nAddr  & 0xFF);
				Ack = MPSSE_SendBytes(buf, nAddrNum+1, false);
				MPSSE_SendRepeatStart();

				buf[0] = (byte)(nSlave | 0x01);
				Ack = MPSSE_SendBytes(buf, 1, false);
				ftdev.Write(dataBuffer, (int)dwNumBytesToSends, ref numBytesWritten);
				dwNumBytesToSends = 0;
				Thread.Sleep(1);
				Ack |= MPSSE_ReadByte(data, ReadNum);
			}
			MPSSE_SendStop();
			return Ack;
		}



		public bool IIC_Wrtie(byte[] data, UInt32 Length)
	{
		bool Ack;
		dwNumBytesToSends = 0;
		MPSSE_SendStart();

		bool bCheckAck = true;
		//Ack = MPSSE_SendBytes(data, Length, false);
		Ack = MPSSE_SendBytes(data, Length, bCheckAck);

		MPSSE_SendStop();
		return Ack;
	}

} // end of class I2C


} // end of namespace USB_I2C