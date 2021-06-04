using System;
using System.Collections;
using System.Threading;

namespace NetworkApp
{
	public class SecondThread : IWithData
	{
		private Semaphore _sendSemaphore;
		private Semaphore _receiveSemaphore;
		private BitArray _receivedMessage;
		private PostToFirstWT _post;

		private readonly string TAG = "2 поток";

		public SecondThread(ref Semaphore sendSemaphore, ref Semaphore receiveSemaphore)
		{
			_sendSemaphore = sendSemaphore;
			_receiveSemaphore = receiveSemaphore;
		}

		public void SecondThreadMain(object obj)
		{
			_post = (PostToFirstWT)obj;

			ConsoleHelper.WriteToConsole(TAG, "Жду запрос на инициализацию");
			WaitOne();
			SetData();
		}

		public void SetData()
		{
			Receipt receipt = null;
			Frame item = (Frame)Utils.DeserializeObject(Utils.BitArrayToByteArray(_receivedMessage));

			switch (BitConverter.ToInt32(Utils.BitArrayToByteArray(item.Status), 0))
			{
				case (int)Type.RIM:
					ConsoleHelper.WriteToConsole(TAG, "Пришел запрос на инициализацию. Отправляю разрешение.");
					receipt = new Receipt(status: new BitArray(BitConverter.GetBytes((int)Type.SIM)));
					break;
				case (int)Type.UP:
					ConsoleHelper.WriteToConsole(TAG, "Пришел запрос на передачу данных. Отправляю разрешение.");
					receipt = new Receipt(status: new BitArray(BitConverter.GetBytes((int)Type.UA)));
					break;
				case (int)Type.RD:
					ConsoleHelper.WriteToConsole(TAG, "Пришел запрос на разъединение. Отправляю согласие и завершаю работу.");
					Utils.DeserializeMessage(TAG);
					receipt = new Receipt(status: new BitArray(BitConverter.GetBytes((int)Type.DISC)));
					break;
				case (int)Type.RR:
					ConsoleHelper.WriteToConsole(TAG, $"Получен кадр #{item.Id}");

					var values = new bool[item.Body.Length];
					for (int m = 0; m < item.Body.Length; m++)
						values[m] = item.Body[m];

					var checkSum = Utils.CheckSum(values);

					if (checkSum == item.CheckSum)
					{
						Utils.AddDataInBuffer(item.Body);
						receipt = new Receipt(id: item.Id, status: new BitArray(BitConverter.GetBytes((int)Type.RR)));
					}
					else
					{
						ConsoleHelper.WriteToConsole(TAG, "Ошибка. Завершаю работу.");
						receipt = new Receipt(id: item.Id, status: new BitArray(BitConverter.GetBytes((int)Type.RNR)));
					}
					break;
				case (int)Type.REJ:
					break;
				default:
					break;
			}

			_post(new BitArray(Utils.SerializeObject(receipt)));
			Release();
			WaitOne();
			SetData();
		}

		public void ReceiveData(BitArray array)
		{
			_receivedMessage = array;
		}

		public void Release()
		{
			_sendSemaphore.Release();
		}

		public void WaitOne()
		{
			_receiveSemaphore.WaitOne();
		}
	}
}
