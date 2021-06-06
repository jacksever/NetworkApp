using System;
using System.Collections;
using System.Threading;

namespace NetworkApp
{
	public class FirstThreadSender : IWithData
	{
		private Semaphore _sendSemaphore;
		private Semaphore _receiveSemaphore;
		private PostToFirstReceiveWT _post;
		private BitArray _receivedMessage;

		private readonly string TAG = "1 поток";
		private readonly Random random = new Random();
		private int index = 0;

		public FirstThreadSender(ref Semaphore sendSemaphore, ref Semaphore receiveSemaphore)
		{
			_sendSemaphore = sendSemaphore;
			_receiveSemaphore = receiveSemaphore;
		}

		public void FirstThreadMain(object obj)
		{
			_post = (PostToFirstReceiveWT)obj;

			ConsoleHelper.WriteToConsole(TAG, $"Отправлен запрос на инициализацию с 2 потоком. Жду подтверждение.");
			_post(new BitArray(Utils.SerializeObject(new Frame(status: new BitArray(BitConverter.GetBytes((int)Type.RIM))))));
			Release();
			WaitOne();
			SetData();
		}

		public void ReceiveData(BitArray data)
		{
			_receivedMessage = data;
		}

		public void SetData()
		{
			Thread.Sleep(300);
			Receipt item = (Receipt)Utils.DeserializeObject(Utils.BitArrayToByteArray(_receivedMessage));
			Frame frame = null;

			if (item != null)
			{
				switch (BitConverter.ToInt32(Utils.BitArrayToByteArray(item.Status), 0))
				{
					case (int)Type.SIM:
						frame = new Frame(status: new BitArray(BitConverter.GetBytes((int)Type.UP)));
						ConsoleHelper.WriteToConsole(TAG, $"Отправлен запрос на передачу данных 2 потоку. Жду подтверждение.");
						break;
					case (int)Type.UA:
						frame = GetFrameWithData(null);
						Utils.IncrementIndex();
						break;
					case (int)Type.DISC:
						ConsoleHelper.WriteToConsole(TAG, "Закрываю соединение и завершаю работу.");
						break;
					case (int)Type.RR:
						if (Utils.Data.Length > Utils.Index)
							frame = GetFrameWithData(null);
						else
						{
							frame = new Frame(status: new BitArray(BitConverter.GetBytes((int)Type.RD)));
							ConsoleHelper.WriteToConsole(TAG, "Передан запрос на разрыв соединения. Жду подтверждения.");
						}
						Utils.IncrementIndex();
						break;
					case (int)Type.REJ:
						frame = GetFrameWithData(index);
						break;
					default:
						break;
				}
			}
			else
			{
				ConsoleHelper.WriteToConsole(TAG, "Квитанция по пакету не пришла. Отправляю заново");
				frame = GetFrameWithData(index);
			}
			 
			if (frame != null)
			{
				var randomNumber = random.Next(1, 100);

				if (randomNumber > 10)
				{
					_post(Utils.SetNoiseRandom(new BitArray(Utils.SerializeObject(frame))));
					Release();
					WaitOne();
					SetData();
				}
				else
				{
					if (BitConverter.ToInt32(Utils.BitArrayToByteArray(frame.Status), 0) == (int)Type.RR ||
						BitConverter.ToInt32(Utils.BitArrayToByteArray(frame.Status), 0) == (int)Type.UA)
					{
						_receivedMessage = null;
						SetData();
					}
					else
					{
						_post(Utils.SetNoiseRandom(new BitArray(Utils.SerializeObject(frame))));
						Release();
						WaitOne();
						SetData();
					}
				}
			}
		}

		private BitArray AddedData()
		{
			var bitArray = new BitArray(Utils.FrameLength);
			for (int item = 0; item < Utils.FrameLength; item++)
			{
				if (item >= Utils.Data[Utils.Index].Length)
					bitArray.Set(item, false);
				else
					bitArray.Set(item, Utils.Data[Utils.Index][item]);
			}

			return bitArray;
		}

		private Frame GetFrameWithData(int? repeat)
		{
			Frame frame;
			if (repeat == null)
			{
				if (Utils.Data.Length > Utils.Index)
				{
					if (Utils.Data[Utils.Index].Length == Utils.FrameLength)
						frame = new Frame(
							id: Utils.IncrementIndexFrame(),
							body: Utils.SetNoiseRandom(new BitArray(Utils.Data[Utils.Index])),
							checkSum: Utils.CheckSum(Utils.Data[Utils.Index]),
							usefulData: Utils.Data[Utils.Index].Length,
							status: new BitArray(BitConverter.GetBytes((int)Type.RR)),
							repeatIndex: null);
					else
					{
						var array = AddedData();
						var values = new bool[array.Length];
						for (int m = 0; m < array.Length; m++)
							values[m] = array[m];

						frame = new Frame(
							id: Utils.IncrementIndexFrame(),
							body: array,
							checkSum: Utils.CheckSum(values),
							usefulData: Utils.Data[Utils.Index].Length,
							status: new BitArray(BitConverter.GetBytes((int)Type.RR)),
							repeatIndex: null);
					}

					index = Utils.Index;

					ConsoleHelper.WriteToConsole(TAG, $"Передан {Utils.GetIndexFrame} кадр. Жду подтверждения.");
				}
				else
				{
					frame = new Frame(status: new BitArray(BitConverter.GetBytes((int)Type.RD)));
					ConsoleHelper.WriteToConsole(TAG, "Передан запрос на разрыв соединения. Жду подтверждения.");
				}
			}
			else
			{
				if (Utils.Data[(int)repeat].Length == Utils.FrameLength)
					frame = new Frame(
						id: Utils.IncrementIndexFrame(),
						body: Utils.SetNoiseRandom(new BitArray(Utils.Data[(int)repeat])),
						checkSum: Utils.CheckSum(Utils.Data[(int)repeat]),
						usefulData: Utils.Data[(int)repeat].Length,
						status: new BitArray(BitConverter.GetBytes((int)Type.RR)),
						repeatIndex: index);
				else
				{
					var array = AddedData();
					var values = new bool[array.Length];
					for (int m = 0; m < array.Length; m++)
						values[m] = array[m];

					frame = new Frame(
						id: Utils.IncrementIndexFrame(),
						body: array,
						checkSum: Utils.CheckSum(values),
						usefulData: Utils.Data[(int)repeat].Length,
						status: new BitArray(BitConverter.GetBytes((int)Type.RR)),
						repeatIndex: index);
				}

				ConsoleHelper.WriteToConsole(TAG, $"Передан {Utils.GetIndexFrame} кадр. Жду подтверждения.");
			}

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
