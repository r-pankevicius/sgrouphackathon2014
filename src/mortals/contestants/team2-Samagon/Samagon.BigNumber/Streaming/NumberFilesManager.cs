using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samagon.BigNumber.Streaming
{
	internal sealed class NumberFilesManager : IDisposable
	{
		#region Fields

		readonly NumberStreamReader m_Reader1;
		readonly NumberStreamReader m_Reader2;
		readonly Stream m_OutputStream;
		readonly Operation m_Operation;
		Task m_WritingTask;
		bool m_PreviousHasCarry;
		bool m_PreviousHasBorrow;
		bool m_ResultIsNegative;
		byte[] m_CachedBytesForEmpty;
		bool m_IsFast = false;

		#endregion

		#region Properties

		public Operation Operation { get { return m_Operation; } }
		public bool ResultIsNegative { get { return m_ResultIsNegative; } }

		#endregion

		#region Constructors

		public NumberFilesManager(Arguments arguments)
		{
			m_Reader1 = new NumberStreamReader(arguments.Input1);
			m_Reader2 = new NumberStreamReader(arguments.Input2);
			m_OutputStream = new FileStream(arguments.Output, File.Exists(arguments.Output) ?
				FileMode.Truncate : FileMode.Create, FileAccess.Write);
			m_Operation = arguments.Operator;
			m_IsFast = arguments.IsFast;
		}

		public NumberFilesManager(
			Stream inputStream1, Stream inputStream2, Stream outputSteam, Operation operation)
		{
			if (operation != Operation.Substraction && operation != Operation.Addition)
				throw new ArgumentOutOfRangeException("operation");

			m_Reader1 = new NumberStreamReader(inputStream1);
			m_Reader2 = new NumberStreamReader(inputStream2);
			m_OutputStream = outputSteam;
			m_Operation = operation;
		}

		#endregion

		#region Methods

		public Task DoArithmeticOperation()
		{
			DateTime now = DateTime.Now;

			return Task.WhenAll(m_Reader1.PrepareFileInfoAsync(m_IsFast), m_Reader2.PrepareFileInfoAsync(m_IsFast))
				.ContinueWith(infos =>
				{
					if (m_Reader1.Info != null && !m_Reader1.Info.IsInvalid
						&& m_Reader2.Info != null && !m_Reader2.Info.IsInvalid)
					{
						var operationFunc = PrepareOperationFunction();

						Console.WriteLine("File metadata prepared: {0}", DateTime.Now - now);

						if (operationFunc == null)
						{
							m_OutputStream.WriteByte(NumberStreamReader.Number_Zero);
							m_OutputStream.Flush();
							Console.WriteLine("Real part calculated: {0}", DateTime.Now - now);
							return;
						}

						m_OutputStream.SetLength(Math.Max(
							m_Reader1.Info.RealPartPosition.GetLength(),
							m_Reader2.Info.RealPartPosition.GetLength()) + 1 + (m_ResultIsNegative ? 1 : 0));

						// Write negative symbol
						if (m_ResultIsNegative)
						{
							m_OutputStream.Seek(0, SeekOrigin.Begin);
							m_OutputStream.WriteByte(NumberStreamReader.Minus_ASCI);
						}
						m_OutputStream.Seek(0, SeekOrigin.End);

						// Check which file has bigger data!
						// Now process real part. Move to the last numbers. Here can happen that lengths are not same.
						m_Reader1.Stream.Seek(m_Reader1.Info.RealPartPosition.To + 1, SeekOrigin.Begin);
						m_Reader2.Stream.Seek(m_Reader2.Info.RealPartPosition.To + 1, SeekOrigin.Begin);

						var readLength = Math.Max(
							m_Reader1.Info.RealPartPosition.GetLength(), m_Reader2.Info.RealPartPosition.GetLength());

						// Now we need go thru 2 streams and read
						var enumerator1 = m_Reader1.ReadContentFromEnd(readLength).GetEnumerator();
						var enumerator2 = m_Reader2.ReadContentFromEnd(readLength).GetEnumerator();

						bool continueReading1 = true, continueReading2 = true;

						while (continueReading1 || continueReading2)
						{
							Task.WaitAll(
								Task.Factory.StartNew(() => continueReading1 = continueReading1 ? enumerator1.MoveNext() : false),
								Task.Factory.StartNew(() => continueReading2 = continueReading2 ? enumerator2.MoveNext() : false));

							if (continueReading1)
								continueReading1 = (enumerator1.Current != null && enumerator1.Current.Length > 0);

							if (continueReading2)
								continueReading2 = enumerator2.Current != null && enumerator2.Current.Length > 0;

							if (continueReading1 || continueReading2)
							{
								// Handle numbers if there is what to handle only
								HandleSomeNumbers(
									continueReading1 && enumerator1.Current != null ? enumerator1.Current : new byte[] { },
									continueReading2 && enumerator2.Current != null ? enumerator2.Current : new byte[] { },
										operationFunc);
							}
						}

						// ensure that all items written
						EnsureWritingTaskFinished();
						m_OutputStream.Seek(m_ResultIsNegative ? 1 : 0, SeekOrigin.Begin);

						if (m_PreviousHasCarry)
							m_OutputStream.WriteByte(NumberStreamReader.Number_One);
						else
							m_OutputStream.WriteByte(NumberStreamReader.Number_Zero);

						Console.WriteLine("Real part calculated: {0}", DateTime.Now - now);
					}
					else
					{
						throw new InvalidOperationException(
							"Input file(s) have invalid data: not possible to calculate.");
					}
				});
		}

		public static bool? FirstIsBigger(NumberStreamReader reader1, NumberStreamReader reader2)
		{
			if (reader1 == null)
				throw new ArgumentNullException("reader1");

			if (reader2 == null)
				throw new ArgumentNullException("reader2");

			if (reader1.Info == null)
				throw new ArgumentNullException("rangeInfo1");

			if (reader2.Info == null)
				throw new ArgumentNullException("rangeInfo2");

			bool? firstIsBigger = null;
			var length1 = reader1.Info.RealPartPosition.GetLength();
			var length2 = reader2.Info.RealPartPosition.GetLength();

			if (length1 > length2)
			{
				firstIsBigger = true;
			}
			else if (length1 < length2)
			{
				firstIsBigger = false;
			}
			else
			{
				// Need to scan file and find out which is bigger!
				reader1.Stream.Seek(reader1.Info.RealPartPosition.From, SeekOrigin.Begin);
				reader2.Stream.Seek(reader2.Info.RealPartPosition.From, SeekOrigin.Begin);

				var enumerator1 = reader1.ReadContentFromBegin(length1).GetEnumerator();
				var enumerator2 = reader2.ReadContentFromBegin(length1).GetEnumerator();
				bool continueReading = true;

				while (continueReading)
				{
					continueReading = enumerator1.MoveNext();

					if (continueReading)
						continueReading = enumerator2.MoveNext();

					if (continueReading)
					{
						continueReading = (enumerator1.Current != null && enumerator1.Current.Length > 0) ||
							(enumerator2.Current != null && enumerator2.Current.Length > 0);
					}

					if (continueReading)
					{
						// Compare symbols from
						for (int i = 0; i < enumerator1.Current.Length; i++)
						{
							if (enumerator1.Current[i] > enumerator2.Current[i])
							{
								firstIsBigger = true;
								continueReading = false;
								break;
							}
							else if (enumerator1.Current[i] < enumerator2.Current[i])
							{
								firstIsBigger = false;
								continueReading = false;
								break;
							}
						}
					}
				}
			}

			return firstIsBigger;
		}

		#endregion

		#region Private Helpers

		private Func<IEnumerable<byte>, IEnumerable<byte>, byte[]> PrepareOperationFunction()
		{
			Lazy<bool?> firstIsBigger = new Lazy<bool?>(() => FirstIsBigger(m_Reader1, m_Reader2));

			return PrepareOperationFunction(firstIsBigger);
		}

		private Func<IEnumerable<byte>, IEnumerable<byte>, byte[]> PrepareOperationFunction(Lazy<bool?> firstIsBigger)
		{
			//If we have subtraction, change second number sign
			if (Operation == BigNumber.Operation.Substraction)
			{
				m_Reader2.Info.IsNegative = !m_Reader2.Info.IsNegative;
			}

			//add numbers if they have the same sign
			if (m_Reader1.Info.IsNegative == m_Reader2.Info.IsNegative)
			{
				m_ResultIsNegative = m_Reader1.Info.IsNegative;
				return (f1, f2) => BigMath.Add(
					f1, f2, m_PreviousHasCarry, out m_PreviousHasCarry);
			}

			if (!firstIsBigger.Value.HasValue)
				return null;

			//we must use subtract
			if (firstIsBigger.Value.Value)
			{
				m_ResultIsNegative = m_Reader1.Info.IsNegative;
				return (f1, f2) =>
					BigMath.Subtract(f1, f2, m_PreviousHasBorrow, out m_PreviousHasBorrow);
			}
			else
			{
				m_ResultIsNegative = m_Reader2.Info.IsNegative;
				return (f1, f2) =>
					BigMath.Subtract(f2, f1, m_PreviousHasBorrow, out m_PreviousHasBorrow);
			}
		}

		private void EnsureWritingTaskFinished()
		{
			if (m_WritingTask != null && m_WritingTask.Status != TaskStatus.RanToCompletion)
			{
				m_WritingTask.Wait();
			}
		}

		private void HandleSomeNumbers(
			byte[] fileData1,
			byte[] fileData2,
			Func<IEnumerable<byte>, IEnumerable<byte>, byte[]> operationFunc)
		{
			if (fileData1.Length < fileData2.Length)
				fileData1 = CopyContent(fileData1, fileData2.Length);
			else if (fileData1.Length > fileData2.Length)
				fileData2 = CopyContent(fileData2, fileData1.Length);

			byte[] data = operationFunc(fileData1, fileData2);

			EnsureWritingTaskFinished();
			m_WritingTask = Task.Factory.StartNew(() =>
				{
					//var data = operationFunc(task1.Result, task2.Result);

					m_OutputStream.Seek(-data.Length, SeekOrigin.Current);
					m_OutputStream.Write(data, 0, data.Length);
					m_OutputStream.Seek(-data.Length, SeekOrigin.Current);
				});
		}

		private byte[] CopyContent(byte[] content, int expectedLength)
		{
			if (m_CachedBytesForEmpty != null && m_CachedBytesForEmpty.Length == expectedLength && content.Length == 0)
				return m_CachedBytesForEmpty;

			var result = new byte[expectedLength];
			var numberOfZeros = expectedLength - content.Length;

			for (var i = 0; i < numberOfZeros; i++)
				result[i] = NumberStreamReader.Number_Zero;

			for (var i = 0; i < content.Length; i++)
				result[i + numberOfZeros] = content[i];

			if (content.Length == 0)
				m_CachedBytesForEmpty = result;

			return result;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			m_Reader1.Dispose();
			m_Reader2.Dispose();
			m_OutputStream.Flush();
			m_OutputStream.Dispose();
		}

		#endregion
	}
}
