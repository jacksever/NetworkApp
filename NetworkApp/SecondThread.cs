using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NetworkApp
{
	public class SecondThread
	{
		private Semaphore _sendSemaphore;
		private Semaphore _receiveSemaphore;
		private BitArray _receivedMessage;
		private PostToFirstWT _post;
		private Encoding _encoding;

		private static List<bool> bitArray = new List<bool>();

		public SecondThread(ref Semaphore sendSemaphore, ref Semaphore receiveSemaphore, Encoding encoding)
		{
			_sendSemaphore = sendSemaphore;
			_receiveSemaphore = receiveSemaphore;
			_encoding = encoding;
		}

		public void SecondThreadMain(object obj)
		{
			_post = (PostToFirstWT)obj;

			ConsoleHelper.WriteToConsole("2 поток", "Начинаю работу. Жду передачи данных.");
			_receiveSemaphore.WaitOne();
			SetData();

		}

		private void SetData()
		{
			Receipt receipt;
			Frame item = (Frame)Utils.DeserializeObject(Utils.BitArrayToByteArray(_receivedMessage));

			ConsoleHelper.WriteToConsole("2 поток", $"Получен кадр #{item.Id}");

			var length = Utils.BitArrayToByteArray(item.Body);
			if (BitConverter.ToInt32(length, 0) != 400)
			{
				bool[] values = new bool[item.Body.Length];
				for (int m = 0; m < item.Body.Length; m++)
					values[m] = item.Body[m];

				var checkSum = Utils.CheckSum(values);

				if (checkSum == item.CheckSum)
				{
					for (int i = 0; i < item.UsefulData; i++)
						bitArray.Add(item.Body[i]);

					receipt = new Receipt(item.Id, new BitArray(BitConverter.GetBytes(200)));
				}
				else
				{
					ConsoleHelper.WriteToConsole("2 поток", "Ошибка. Завершаю работу");
					receipt = new Receipt(item.Id, new BitArray(BitConverter.GetBytes(400)));
				}

			}
			else
			{
				ConsoleHelper.WriteToConsole("2 поток", $"Полученные данные:  {_encoding.GetString(Utils.BitArrayToByteArray(new BitArray(bitArray.ToArray())))}");
				ConsoleHelper.WriteToConsole("2 поток", "Завершаю работу");

				receipt = new Receipt(item.Id, new BitArray(BitConverter.GetBytes(400)));
			}

			_post(new BitArray(Utils.SerializeObject(receipt)));
			_sendSemaphore.Release();
			_receiveSemaphore.WaitOne();
			SetData();
		}

		public void ReceiveData(BitArray array)
		{
			_receivedMessage = array;
		}
	}
}
