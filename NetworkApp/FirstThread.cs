using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetworkApp
{
	public class FirstThread
	{
		private Semaphore _sendSemaphore;
		private Semaphore _receiveSemaphore;
		private string _sendMessage;
		private PostToSecondWT _post;
		private BitArray _receivedMessage;
		private Encoding _encoding;

		private static int i = 0;
		private static bool[][] result;

		public FirstThread(ref Semaphore sendSemaphore, ref Semaphore receiveSemaphore, Encoding encoding, string sendData)
		{
			_sendSemaphore = sendSemaphore;
			_receiveSemaphore = receiveSemaphore;
			_encoding = encoding;
			_sendMessage = sendData;
		}

		public void FirstThreadMain(object obj)
		{
			_post = (PostToSecondWT)obj;
			ConsoleHelper.WriteToConsole("1 поток", "Начинаю работу. Готовлю данные для передачи.");

			//_sendMessage = "Привет! Я студент РУТ (МИИТ)";
			var bits = new BitArray(_encoding.GetBytes(_sendMessage));

			bool[] values = new bool[bits.Count];
			for (int m = 0; m < bits.Count; m++)
				values[m] = bits[m];

			int j = 0;
			result = values.GroupBy(s => j++ / Utils.FrameLength).Select(g => g.ToArray()).ToArray();

			Frame frame;
			if (result[i].Length == Utils.FrameLength)
				frame = new Frame(i, new BitArray(result[i]), GetCheckSum(), result[i].Length);
			else
				frame = new Frame(i, AddedData(), GetCheckSum(), result[i].Length);


			_post(new BitArray(Utils.SerializeObject(frame)));
			_sendSemaphore.Release();

			ConsoleHelper.WriteToConsole("1 поток", $"Переданы данные {i} потока");
			ConsoleHelper.WriteToConsole("1 поток", "Жду результата");
			i += 1;
			_receiveSemaphore.WaitOne();
			SetData();
		}

		public void ReceiveData(BitArray data)
		{
			_receivedMessage = data;
		}

		private void SetData()
		{
			Thread.Sleep(200);
			Receipt item = (Receipt)Utils.DeserializeObject(Utils.BitArrayToByteArray(_receivedMessage));
			Frame frame;

			if (item == null)
			{
				ConsoleHelper.WriteToConsole("1 поток", "Ошибка. Завершаю работу");
				return;
			}

			if (BitConverter.ToInt32(Utils.BitArrayToByteArray(item.Status), 0) == 200)
			{
				if (result.Length > i)
				{
					if (result[i].Length == Utils.FrameLength)
						frame = new Frame(i, new BitArray(result[i]), GetCheckSum(), result[i].Length);
					else
					{
						var array = AddedData();
						var checkSum = 0;
						for (int fr = 0; fr < Utils.FrameLength; fr++)
							if (fr % 5 == 0)
								checkSum += array.Get(fr) == false ? 0 : 1;

						frame = new Frame(i, array, checkSum, result[i].Length);
					}
				}
				else
					frame = new Frame(i, new BitArray(BitConverter.GetBytes(400)), 0, -1);

				_post(new BitArray(Utils.SerializeObject(frame)));
				_sendSemaphore.Release();

				ConsoleHelper.WriteToConsole("1 поток", $"Переданы данные {i} потока");
				ConsoleHelper.WriteToConsole("1 поток", "Жду результата");
				i = i + 1;
				_receiveSemaphore.WaitOne();
				SetData();
			}
			else
				ConsoleHelper.WriteToConsole("1 поток", "Завершаю работу");
		}

		private BitArray AddedData()
		{
			var bitArray = new BitArray(Utils.FrameLength);
			for (int item = 0; item < Utils.FrameLength; item++)
			{
				if (item >= result[i].Length)
					bitArray.Set(item, false);
				else
					bitArray.Set(item, result[i][item]);
			}

			return bitArray;
		}

		private int GetCheckSum()
		{
			int checkSum = 0;
			for (int fr = 0; fr < result[i].Length; fr++)
				if (fr % 5 == 0)
					checkSum += result[i][fr] == false ? 0 : 1;

			return checkSum;
		}
	}
}
