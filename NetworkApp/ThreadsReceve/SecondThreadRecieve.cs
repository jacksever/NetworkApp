using System;
using System.Collections;
using System.Threading;

namespace NetworkApp
{
	public class SecondThreadReceive : IWithData
	{
		private Semaphore _sendSemaphore;
		private Semaphore _receiveSemaphore;
		private BitArray _receivedMessage;
		private PostToFirstWT _post;

		public SecondThreadReceive(ref Semaphore sendSemaphore, ref Semaphore receiveSemaphore)
		{
			_sendSemaphore = sendSemaphore;
			_receiveSemaphore = receiveSemaphore;
		}

		public void SecondThreadMain(object obj)
		{
			_post = (PostToFirstWT)obj;

			ConsoleHelper.WriteToConsole("2 поток", "Жду запрос на инициализацию");
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
					ConsoleHelper.WriteToConsole("2 поток", "Пришел запрос на инициализацию. Отправляю разрешение.");
					receipt = new Receipt(status: new BitArray(BitConverter.GetBytes((int)Type.SIM)));
					break;
				case (int)Type.UP:
					ConsoleHelper.WriteToConsole("2 поток", "Пришел запрос на передачу данных. Отправляю разрешение.");
					receipt = new Receipt(status: new BitArray(BitConverter.GetBytes((int)Type.UA)));
					break;
				case (int)Type.RD:
					ConsoleHelper.WriteToConsole("2 поток", "Пришел запрос на разъединение. Отправляю согласие и завершаю работу.");
					ConsoleHelper.WriteToConsole("2 поток", $"Полученные данные:  {Utils.Encoding.GetString(Utils.BitArrayToByteArray(new BitArray(Utils.BitArray.ToArray())))}");
					receipt = new Receipt(status: new BitArray(BitConverter.GetBytes((int)Type.DISC)));
					break;
				case (int)Type.RR:
					ConsoleHelper.WriteToConsole("2 поток", $"Получен кадр #{item.Id}");

					bool[] values = new bool[item.Body.Length];
					for (int m = 0; m < item.Body.Length; m++)
						values[m] = item.Body[m];

					var checkSum = Utils.CheckSum(values);

					if (checkSum == item.CheckSum)
					{
						for (int i = 0; i < item.UsefulData; i++)
							Utils.BitArray.Add(item.Body[i]);

						receipt = new Receipt(id: item.Id, status: new BitArray(BitConverter.GetBytes((int)Type.RR)));
					}
					else
					{
						ConsoleHelper.WriteToConsole("2 поток", "Ошибка. Завершаю работу.");
						receipt = new Receipt(id: item.Id, status: new BitArray(BitConverter.GetBytes(400)));
						// TODO: запросить конкретный пакет
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
