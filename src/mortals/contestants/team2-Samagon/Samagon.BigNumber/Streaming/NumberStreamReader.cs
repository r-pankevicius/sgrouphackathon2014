using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Samagon.BigNumber.Streaming
{
	internal sealed class NumberStreamReader : IDisposable
	{
		#region Constants

		public const int Buffer = 1000000;
		public const byte Plus_ASCI = 43;
		public const byte Minus_ASCI = 45;
		public const byte Number_Zero = 48;
		public const byte Number_One = 49;
		public const byte Number_Nine = 57;
		public const byte NewLine = 10;
		public const byte CR = 13;

		#endregion

		#region Fields

		Stream m_Stream;
		NumberFileInfo m_Info;

		#endregion

		#region Properties

		public NumberFileInfo Info { get { return m_Info; } }
		public Stream Stream { get { return m_Stream; } }

		#endregion

		#region Constructors

		public NumberStreamReader(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentNullException("fileName");

			m_Stream = new FileStream(
				fileName, FileMode.Open, FileAccess.Read, FileShare.Read, Buffer, FileOptions.SequentialScan);
		}

		public NumberStreamReader(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			m_Stream = stream;
		}

		#endregion

		#region Methods

		public async Task PrepareFileInfoAsync(bool isFileCorrect)
		{
			if (m_Stream.Position != 0)
				m_Stream.Seek(0, SeekOrigin.Begin);

			if (isFileCorrect)
			{
				PrepareFileInfoForCorrectFile();
				return;
			}

			m_Info = null;

			int readCount;
			bool toPositionWasFound = false;
			byte[] buffer = new byte[Math.Min(Buffer, m_Stream.Length)];
			bool crWasDetectedBefore = false;
			long lastNumberIndex = -1;
			int? lastZeroIndexFromStart = null;

			// Need to Check if file info is ok.
			while ((readCount = await m_Stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
			{
				bool isFirstRead = m_Info == null;

				if (buffer.Length > 0 && isFirstRead)
				{
					var first = buffer[0];

					if (first != Plus_ASCI && first != Minus_ASCI && !IsNumber(first))
					{
						m_Info = new NumberFileInfo()
						{
							IsInvalid = true
						};

						break;
					}
					else
					{
						m_Info = new NumberFileInfo()
						{
							IsNegative = first == Minus_ASCI
						};

						m_Info.RealPartPosition = new FileRangeInfo()
						{
							From = first == Minus_ASCI || first == Plus_ASCI ? 1 : 0
						};

						if (IsNumber(first))
							lastNumberIndex = 0;

						if (first == Number_Zero)
							lastZeroIndexFromStart = 0;
					}
				}

				byte value;
				var streamPosition = m_Stream.Position;
				var bufferLength = buffer.Length;

				for (int i = isFirstRead ? 1 : 0; i < bufferLength; i++)
				{
					value = buffer[i];

					// ignore cr - can exist before new line
					if (value == CR)
					{
						crWasDetectedBefore = true;
						continue;
					}

					if (lastZeroIndexFromStart.HasValue || (lastNumberIndex == -1))
					{
						if (value == Number_Zero)
						{
							lastZeroIndexFromStart = i;
						}
						else
						{
							// skip 0
							if (lastZeroIndexFromStart.HasValue)
								m_Info.RealPartPosition.From = lastZeroIndexFromStart.Value + 1;

							lastZeroIndexFromStart = null;
						}
					}

					// Take real position from begin of the file.
					var realPosition = streamPosition - readCount + i;

					if (value == NewLine)
					{
						if (m_Info.RealPartPosition.To == 0)
						{
							toPositionWasFound = true;
							m_Info.RealPartPosition.To = realPosition - (crWasDetectedBefore ? 2 : 1);
						}

						crWasDetectedBefore = false;
						break;
					}
					else if (!IsNumber(value))
					{
						m_Info.IsInvalid = true;
						break;
					}
					else
					{
						lastNumberIndex = realPosition;
					}
				}
			}

			if (m_Info.RealPartPosition != null && m_Info.RealPartPosition.To == 0 && !toPositionWasFound)
			{
				if (lastNumberIndex >= m_Info.RealPartPosition.From)
					m_Info.RealPartPosition.To = m_Stream.Length - 1;
				else
					m_Info.RealPartPosition = null;
			}

			if (m_Info.RealPartPosition == null || m_Info.RealPartPosition.GetLength() <= 0)
			{
				m_Info.IsInvalid = true;
			}
		}

		public IEnumerable<byte[]> ReadContentFromBegin(long howManyToRead)
		{
			int readCount;
			long totalReadCount = 0;

			byte[] buffer = new byte[Math.Min(Math.Min(Buffer, howManyToRead), m_Stream.Length)];

			// Need to Check if file info is ok.
			while ((readCount = m_Stream.Read(buffer, 0, buffer.Length)) != 0)
			{
				totalReadCount += readCount;
				yield return buffer.ToArray();

				if (howManyToRead == totalReadCount)
				{
					yield break;
				}
				else
				{
					buffer = new byte[Math.Min(Math.Min(Buffer, howManyToRead - totalReadCount), m_Stream.Length)];
				}
			}
		}

		public IEnumerable<byte[]> ReadContentFromEnd(long howManyToRead)
		{
			int readCount;
			long totalReadCount = 0;

			byte[] buffer = new byte[Math.Min(Math.Min(Buffer, howManyToRead), m_Stream.Position)];
			m_Stream.Seek(-buffer.Length, SeekOrigin.Current);
			var streamLength = m_Stream.Length;

			// Need to Check if file info is ok.
			while ((readCount = m_Stream.Read(buffer, 0, buffer.Length)) != 0)
			{
				totalReadCount += readCount;

				if (buffer.Length > 0 && !IsNumber(buffer[0]))
					buffer[0] = Number_Zero;

				m_Stream.Seek(-buffer.Length, SeekOrigin.Current);
				yield return buffer.ToArray();

				if (howManyToRead <= totalReadCount || streamLength == totalReadCount)
				{
					yield break;
				}
				else
				{
					buffer = new byte[Math.Min(Math.Min(Buffer, howManyToRead - totalReadCount), m_Stream.Position)];
					m_Stream.Seek(-buffer.Length, SeekOrigin.Current);
				}
			}
		}

		#endregion

		#region Private Helpers

		private static bool IsNumber(byte value)
		{
			return value >= Number_Zero && value <= Number_Nine;
		}

		private void PrepareFileInfoForCorrectFile()
		{
			byte first = Convert.ToByte(m_Stream.ReadByte());

			m_Info = new NumberFileInfo()
			{
				IsInvalid = false,
				IsNegative = first == Minus_ASCI
			};

			m_Info.RealPartPosition = new FileRangeInfo()
			{
				From = (first == Minus_ASCI || first == Plus_ASCI) ? 1 : 0,
				To = m_Stream.Length - 1
			};
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			m_Stream.Dispose();
		}

		#endregion
	}
}
