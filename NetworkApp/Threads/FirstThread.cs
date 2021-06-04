using System;
using System.Collections;
using System.Threading;

namespace NetworkApp
{
	public class FirstThread : IWithData
	{
		private Semaphore _sendSemaphore;
		private Semaphore _receiveSemaphore;
		private PostToSecondWT _post;
		private BitArray _receivedMessage;

		private static int i = 0;
		private readonly string TAG = "1 поток";

		public FirstThread(ref Semaphore sendSemaphore, ref Semaphore receiveSemaphore)
		{
			_sendSemaphore = sendSemaphore;
			_receiveSemaphore = receiveSemaphore;
		}

		public void FirstThreadMain(object obj)
		{
			_post = (PostToSecondWT)obj;

			_post(new BitArray(Utils.SerializeObject(new Frame(status: new BitArray(BitConverter.GetBytes((int)Type.RIM))))));
			Release();
			ConsoleHelper.WriteToConsole(TAG, $"Отправлен запрос на инициализацию с 2 потоком. Жду подтверждение.");
			WaitOne();
			SetData();
		}

		public void ReceiveData(BitArray data)
		{
			_receivedMessage = data;
		}

		public void SetData()
		{
			Thread.Sleep(200);
			Receipt item = (Receipt)Utils.DeserializeObject(Utils.BitArrayToByteArray(_receivedMessage));
			Frame frame = null;

			switch (BitConverter.ToInt32(Utils.BitArrayToByteArray(item.Status), 0))
			{
				case (int)Type.SIM:
					frame = new Frame(status: new BitArray(BitConverter.GetBytes((int)Type.UP)));
					ConsoleHelper.WriteToConsole(TAG, $"Отправлен запрос на передачу данных 2 потоку. Жду подтверждение.");
					break;
				case (int)Type.UA:
					frame = GetFrameWithData(item.Id);
					i++;
					break;
				case (int)Type.DISC:
					ConsoleHelper.WriteToConsole(TAG, "Закрываю соединение и завершаю работу.");
					break;
				case (int)Type.RR:
					if (Utils.Data.Length > i)
						frame = GetFrameWithData(item.Id);
					else
					{
						frame = new Frame(status: new BitArray(BitConverter.GetBytes((int)Type.RD)));
						ConsoleHelper.WriteToConsole(TAG, "Передан запрос на разрыв соединения. Жду подтверждения.");
					}
					i++;
					break;
				default:
					break;
			}

			if (frame != null)
			{
				_post(new BitArray(Utils.SerializeObject(frame)));
				Release();
				WaitOne();
				SetData();
			}
		}

		private BitArray AddedData()
		{
			var bitArray = new BitArray(Utils.FrameLength);
			for (int item = 0; item < Utils.FrameLength; item++)
				if (item >= Utils.Data[i].Length)
					bitArray.Set(item, false);
				else
					bitArray.Set(item, Utils.Data[i][item]);

			return bitArray;
		}

		private int GetIndexFrame(int lastIndex)
		{
			return (lastIndex + 1) != 8 ? lastIndex + 1 : 0;
		}

		private Frame GetFrameWithData(int index)
		{
			Frame frame;
			if (Utils.Data[i].Length == Utils.FrameLength)
				frame = new Frame(
					id: GetIndexFrame(index),
					body: new BitArray(Utils.Data[i]),
					checkSum: Utils.CheckSum(Utils.Data[i]),
					usefulData: Utils.Data[i].Length,
					status: new BitArray(BitConverter.GetBytes((int)Type.RR)));
			else
			{
				var array = AddedData();
				bool[] values = new bool[array.Length];
				for (int m = 0; m < array.Length; m++)
					values[m] = array[m];

				frame = new Frame(
					id: GetIndexFrame(index),
					body: array,
					checkSum: Utils.CheckSum(values),
					usefulData: Utils.Data[i].Length,
					status: new BitArray(BitConverter.GetBytes((int)Type.RR)));
			}

			ConsoleHelper.WriteToConsole(TAG, $"Передан {GetIndexFrame(index)} кадр. Жду подтверждения.");

			return frame;
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
