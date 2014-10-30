using System;
using System.IO;
using System.Threading;

namespace AstroGrator
{
	public class Calculator
	{
		private string m_File1;
		private string m_File2;
		private char m_Operation;
		private bool m_CleanSecond;
		private bool m_CleanFirst;
		private string TempFile = "TempFile.txt";

		public long BufferSize { get; set; }
		public string ResultFile { get; set; }

		public Calculator(string file1, string file2, char operation, string resultFile)
		{
			m_File1 = file1;
			m_File2 = file2;
			m_Operation = operation;
			BufferSize = 1024 * 1024 * 256;
			ResultFile = resultFile;

			Thread.CurrentThread.Priority = ThreadPriority.Highest;
		}

		public void Calculate()
		{
			if (string.Equals(m_File1, m_File2, StringComparison.OrdinalIgnoreCase))
			{
				string newName = Path.GetDirectoryName(m_File2)
					+ Path.GetFileNameWithoutExtension(m_File2) + Guid.NewGuid().ToString()
					+ Path.GetExtension(m_File2);

				File.Copy(m_File2, newName);
				m_File2 = newName;

				m_CleanSecond = true;
			}


			FileStream file1 = new FileStream(m_File1, FileMode.Open,
				FileAccess.Read, FileShare.Read);

			FileStream file2 = new FileStream(m_File2, FileMode.Open,
				FileAccess.Read, FileShare.Read);

			byte[] buffer1 = new byte[BufferSize];
			byte[] buffer2 = new byte[BufferSize];

			//0 = plus; 1 = minus
			int sign1 = 0;
			long lastBufferSize1 = 0;
			long lastNumberPos1 = 0;
			long totalpages1 = 0;
			bool read1 = true;
			long seekAdjustmentForNumer1 = 0;


			int sign2 = 0;
			long lastBufferSize2 = 0;
			long lastNumberPos2 = 0;
			long totalpages2 = 0;
			bool read2 = true;
			long seekAdjustmentForNumer2 = 0;

			file1 = CheckFile1(file1, buffer1, buffer2);
			file2 = CheckFile2(file2, buffer1, buffer2);

			//get sign 1
			file1.Read(buffer1, 0, 1);
			if (buffer1[0] == '-' || buffer1[0] == '+')
			{
				if (buffer1[0] == '-')
				{
					sign1 = 1;
				}

				seekAdjustmentForNumer1 = 1;
			}
			else
			{
				file1.Seek(-1, SeekOrigin.Current);
			}

			//get sign 2
			file2.Read(buffer2, 0, 1);
			if (buffer2[0] == '-' || buffer2[0] == '+')
			{
				if (buffer2[0] == '-')
				{
					sign2 = 1;
				}

				seekAdjustmentForNumer2 = 1;
			}
			else
			{
				file2.Seek(-1, SeekOrigin.Current);
			}


			bool firstIsBigger = true;
			bool biggerFound = false;

			//Read length
			do
			{
				long local1 = 0;

				if (read1)
				{
					lastBufferSize1 = file1.Read(buffer1, 0, (int)BufferSize);

					local1 = GetNumberLength(buffer1, lastBufferSize1);

					lastNumberPos1 += local1;

					if (local1 > 0)
					{
						totalpages1++;
					}

					if (local1 < lastBufferSize1 || local1 == 0)
					{
						read1 = false;
					}
				}

				long local2 = 0;

				if (read2)
				{
					lastBufferSize2 = file2.Read(buffer2, 0, (int)BufferSize);

					local2 = GetNumberLength(buffer2, lastBufferSize2);

					lastNumberPos2 += local2;

					if (local2 > 0)
					{
						totalpages2++;
					}

					if (local2 < lastBufferSize2 || local2 == 0)
					{
						read2 = false;
					}
				}

				if (local1 == local2 && !biggerFound)
				{
					for (long i = 0; i < local1; i++)
					{
						if (buffer2[i] > buffer1[i])
						{
							firstIsBigger = false; //maybe second is bigger
							biggerFound = true;
							break;
						}
						if (buffer2[i] < buffer1[i])
						{
							firstIsBigger = true; //maybe first is bigger
							biggerFound = true;
							break;
						}
					}
				}

			} while ((lastBufferSize1 == BufferSize || lastBufferSize2 == BufferSize) && (read1 || read2));
			//read end

			if (lastNumberPos1 > lastNumberPos2)
			{
				firstIsBigger = true;
			}
			else if (lastNumberPos1 < lastNumberPos2)
			{
				firstIsBigger = false;
			}

			//find result sign
			int finalSign = 0;

			if (firstIsBigger)
			{
				finalSign = sign1;
			}
			else
			{
				if (m_Operation == '-')
				{
					if (sign2 == 1)
					{
						finalSign = 0;
					}
					else
					{
						finalSign = 1;
					}
				}
				else
				{
					finalSign = sign2;
				}
			}

			//find operation
			int sign3 = 0;

			if (m_Operation == '-')
			{
				sign3 = 1;
			}

			if ((sign1 + sign2 + sign3) % 2 == 1)
			{
				m_Operation = '-';
			}
			else
			{
				m_Operation = '+';
			}


			//check if swap is needed
			if (!firstIsBigger)
			{
				int tmpSign = sign1;
				long tmpLastNumberPos = lastNumberPos1;
				long tmpTotalpages = totalpages1;
				long tmpSeekAdjustmentForNumer = seekAdjustmentForNumer1;
				FileStream tmpFile = file1;

				sign1 = sign2;
				lastNumberPos1 = lastNumberPos2;
				totalpages1 = totalpages2;
				seekAdjustmentForNumer1 = seekAdjustmentForNumer2;
				file1 = file2;

				sign2 = tmpSign;
				lastNumberPos2 = tmpLastNumberPos;
				totalpages2 = tmpTotalpages;
				seekAdjustmentForNumer2 = tmpSeekAdjustmentForNumer;
				file2 = tmpFile;
			}

			//sum
			long traillingZeros = 0;
			int r = 0;

			DeleteIfExists(TempFile);
			FileStream file3 = new FileStream(TempFile, FileMode.CreateNew);
			byte[] buffer3 = new byte[BufferSize];

			for (long i = 0; i < totalpages1; i++)
			{
				long seekPos1;
				long bytesToRead1;

				seekPos1 = lastNumberPos1 - BufferSize * (i + 1) + seekAdjustmentForNumer1;

				if (seekPos1 - seekAdjustmentForNumer1 >= 0)
				{
					bytesToRead1 = BufferSize;
				}
				else
				{
					bytesToRead1 = (int)(BufferSize + seekPos1 - seekAdjustmentForNumer1);
					seekPos1 = seekAdjustmentForNumer1;
				}

				file1.Seek(seekPos1, SeekOrigin.Begin);

				file1.Read(buffer1, 0, (int)bytesToRead1);


				long seekPos2;
				long bytesToRead2;

				seekPos2 = lastNumberPos2 - BufferSize * (i + 1) + seekAdjustmentForNumer2;

				if (seekPos2 - seekAdjustmentForNumer2 >= 0)
				{
					bytesToRead2 = BufferSize;
				}
				else
				{
					bytesToRead2 = BufferSize + seekPos2 - seekAdjustmentForNumer2;
					if (bytesToRead2 <= 0)
					{
						bytesToRead2 = 0;
					}
					seekPos2 = seekAdjustmentForNumer2;
				}

				if (bytesToRead2 > 0)
				{
					file2.Seek(seekPos2, SeekOrigin.Begin);
					file2.Read(buffer2, 0, (int)bytesToRead2);
				}

				if (m_Operation == '+')
				{
					r = AddBuffers(r, buffer1, buffer2, (int)bytesToRead1 - 1, (int)bytesToRead2 - 1, buffer3);
				}
				else
				{
					r = SubstractBuffers(r, buffer1, buffer2, (int)bytesToRead1 - 1, (int)bytesToRead2 - 1, buffer3, ref traillingZeros);
				}

				file3.Write(buffer3, 0, (int)bytesToRead1);
			}

			file1.Close();
			file2.Close();

			if (r == 1)
			{
				if (m_Operation == '+')
				{
					buffer3[0] = (byte)'1';
					file3.Write(buffer3, 0, 1);
					lastNumberPos1++;
				}
			}

			DeleteIfExists(ResultFile);
			FileStream file4 = new FileStream(ResultFile, FileMode.CreateNew);

			if (finalSign == 1 && traillingZeros != lastNumberPos1)
			{
				buffer3[0] = (byte)'-';
				file4.Write(buffer3, 0, 1);
			}

			if (traillingZeros == lastNumberPos1)
			{
				buffer3[0] = (byte)'0';
				file4.Write(buffer3, 0, 1);
			}

			for (int i = 0; i < totalpages1 + 1; i++)
			{
				long seekPos1;
				long bytesToRead1;

				seekPos1 = lastNumberPos1 - BufferSize * (i + 1) - traillingZeros;

				if (seekPos1 >= 0)
				{
					bytesToRead1 = BufferSize;
				}
				else
				{
					bytesToRead1 = BufferSize + seekPos1;
					seekPos1 = 0;
				}

				if (bytesToRead1 <= 0)
					break;

				file3.Seek(seekPos1, SeekOrigin.Begin);
				file3.Read(buffer1, 0, (int)bytesToRead1);
				Reverse(buffer1, buffer2, (int)bytesToRead1);
				file4.Write(buffer2, 0, (int)bytesToRead1);
			}

			file3.Close();
			file4.Close();

			if (m_CleanFirst)
				DeleteIfExists(m_File1);

			if (m_CleanSecond)
				DeleteIfExists(m_File2);

			DeleteIfExists(TempFile);
		}

		private FileStream CheckFile2(FileStream file2, byte[] buffer1, byte[] buffer2)
		{
			file2.Read(buffer2, 0, 2);

			file2.Seek(0, SeekOrigin.Begin);

			if (buffer2[0] != '0' && buffer2[1] != '0')
			{

			}
			else
			{
				DeleteIfExists("t2.txt");
				FileStream t2 = new FileStream("t2.txt", FileMode.CreateNew);
				//useless update to use strange numbers 
				int lastBufferSize = 0;
				bool zeros = true;
				long total = 0;

				do
				{
					int l = 0;

					lastBufferSize = file2.Read(buffer1, 0, (int)BufferSize);
					RemoveZeros(buffer1, lastBufferSize, buffer2, ref l, ref zeros);

					t2.Write(buffer2, 0, l);

					total += l;

				} while (lastBufferSize == BufferSize);


				if (total == 0)
				{
					buffer2[0] = (byte)'0';
					t2.Write(buffer2, 0, 1);
				}

				t2.Close();
				file2.Close();

				m_File2 = "t2.txt";
				file2 = new FileStream(m_File2, FileMode.Open, FileAccess.Read, FileShare.Read);
				m_CleanSecond = true;
			}

			return file2;
		}

		private FileStream CheckFile1(FileStream file1, byte[] buffer1, byte[] buffer2)
		{
			file1.Read(buffer1, 0, 2);

			file1.Seek(0, SeekOrigin.Begin);

			if (buffer1[0] != '0' && buffer1[1] != '0')
			{

			}
			else
			{
				//useless update to use strange numbers 
				DeleteIfExists("t1.txt");
				FileStream t1 = new FileStream("t1.txt", FileMode.CreateNew);
				int lastBufferSize;
				bool zeros = true;
				long total = 0;

				do
				{
					int l = 0;

					lastBufferSize = file1.Read(buffer1, 0, (int)BufferSize);
					RemoveZeros(buffer1, lastBufferSize, buffer2, ref l, ref zeros);

					t1.Write(buffer2, 0, l);

					total += l;

				} while (lastBufferSize == BufferSize);

				if (total == 0)
				{
					buffer2[0] = (byte)'0';
					t1.Write(buffer2, 0, 1);
				}

				t1.Close();
				file1.Close();

				m_File1 = "t1.txt";
				file1 = new FileStream(m_File1, FileMode.Open, FileAccess.Read, FileShare.Read);
				m_CleanFirst = true;
			}
			return file1;
		}

		private static void Reverse(byte[] buffer1, byte[] buffer2, int lenght1)
		{
			long i = 0;
			long j = lenght1 - 1;

			while (j >= 0)
			{
				buffer2[i] = buffer1[j];

				i++;
				j--;
			}
		}

		private static long GetNumberLength(byte[] buffer, long bs)
		{
			long i = 0;

			while (i < bs && (buffer[i] >= (byte)'0' && buffer[i] <= (byte)'9'))
			{
				i++;
			}

			return i;
		}

		private static int AddBuffers(int r, byte[] buffer1, byte[] buffer2, int posStart1, int postStart2, byte[] buffer3)
		{
			int i = posStart1;
			int j = postStart2;
			int z = 0;

			while (j >= 0)
			{
				// optimize 0
				int s = (buffer1[i] /*- '0'*/) + (buffer2[j] /*- '0'*/) + r;

				r = 0;

				if (s >= (10 + 2 * '0'))
				{
					s -= 10;
					r = 1;
				}

				buffer3[z++] = (byte)(s - '0');

				i--;
				j--;
			}

			while (i >= 0)
			{
				int s = (buffer1[i] /*- '0'*/) + r;

				r = 0;

				if (s >= (10 + '0'))
				{
					s -= 10;
					r = 1;
				}

				buffer3[z++] = (byte)(s /*+ '0'*/);

				i--;
			}

			return r;
		}

		private static int SubstractBuffers(int r, byte[] buffer1, byte[] buffer2, int posStart1, int postStart2, byte[] buffer3, ref long traillingZeros)
		{
			int i = posStart1;
			int j = postStart2;
			int z = 0;

			while (j >= 0)
			{
				// optimize 0
				int s = (buffer1[i] /*- '0'*/) - (buffer2[j] /*- '0'*/) - r;

				r = 0;

				if (s < 0)
				{
					s += 10;
					r = 1;
				}

				if (s == 0)
				{
					traillingZeros++;
				}
				else
				{
					traillingZeros = 0;
				}

				buffer3[z++] = (byte)(s + '0');

				i--;
				j--;
			}

			while (i >= 0)
			{
				int s = buffer1[i] - r;

				r = 0;

				if (s < 0 + '0')
				{
					s += 10;
					r = 1;
				}

				if (s == 0 + '0')
				{
					traillingZeros++;
				}
				else
				{
					traillingZeros = 0;
				}

				buffer3[z++] = (byte)(s);

				i--;
			}

			return r;
		}

		private static void DeleteIfExists(string file)
		{
			try
			{
				if (File.Exists(file))
					File.Delete(file);
			}
			catch (Exception ex)
			{
				Console.WriteLine("I can't take this anymore... Please delete the damn file yourself!");
				Console.WriteLine(ex.Message);
				throw new DoomsDayException() { EverythingIsDoomed = true };
			}
		}

		private static void RemoveZeros(byte[] buffer1, int s, byte[] buffer2, ref int length, ref bool zeros)
		{
			length = 0;
			int i = 0;
			int j = 0;

			while (i < s)
			{
				if (buffer1[i] == '+' || buffer1[i] == '-')
				{
					buffer2[j++] = buffer1[i++];
					continue;
				}
				if (buffer1[i] == '0' && zeros)
				{
					i++;
				}
				else
				{
					buffer2[j++] = buffer1[i++];
					zeros = false;
				}
			}

			length = j;
		}
	}
}
