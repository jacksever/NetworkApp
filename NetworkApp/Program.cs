using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkApp
{
	public delegate void PostToFirstSenderWT(BitArray message);
	public delegate void PostToSecondSenderWT(BitArray message);
	public delegate void PostToFirstReceiveWT(BitArray message);
	public delegate void PostToSecondReceiveWT(BitArray message);

	public class Program
	{
		private static readonly string TAG = "Главный поток";

		static void Main()
		{
			ConsoleHelper.WriteToConsole(TAG, "Введите '1' для передачи сообщения..");
			ConsoleHelper.WriteToConsole(TAG, "Введите '2' для передачи файла..");
			int number = int.Parse(Console.ReadLine());

			string data = null;
			switch (number)
			{
				case 1:
					ConsoleHelper.WriteToConsole(TAG, "Введите сообщение..");
					data = Console.ReadLine();
					break;
				case 2:
					ConsoleHelper.WriteToConsole(TAG, "Введите название файла..");
					data = Console.ReadLine();
					break;
			}

			Semaphore firstReceiveSemaphore = new Semaphore(0, 1);
			Semaphore secondReceiveSemaphore = new Semaphore(0, 1);
			Semaphore firstSenderSemaphore = new Semaphore(0, 1);
			Semaphore secondSenderSemaphore = new Semaphore(0, 1);

			FirstThreadSender firstThreadSender = new FirstThreadSender(ref firstReceiveSemaphore, ref firstSenderSemaphore);
			FirstThreadReceive firstThreadReceive = new FirstThreadReceive(ref firstSenderSemaphore, ref firstReceiveSemaphore);
			SecondThreadSender secondThreadSender = new SecondThreadSender(ref secondReceiveSemaphore, ref secondSenderSemaphore);
			SecondThreadReceive secondThreadReceive = new SecondThreadReceive(ref secondSenderSemaphore, ref secondReceiveSemaphore);

			Thread threadFirstSender = new Thread(new ParameterizedThreadStart(firstThreadSender.FirstThreadMain));
			Thread threadSecondSender = new Thread(new ParameterizedThreadStart(secondThreadSender.FirstThreadMain));
			Thread threadSecondReceive = new Thread(new ParameterizedThreadStart(secondThreadReceive.SecondThreadMain));
			Thread threadFirstReceive = new Thread(new ParameterizedThreadStart(firstThreadReceive.SecondThreadMain));

			PostToFirstSenderWT postToFirstSenderWt = new PostToFirstSenderWT(firstThreadSender.ReceiveData);
			PostToSecondSenderWT postToSecondSenderWt = new PostToSecondSenderWT(secondThreadSender.ReceiveData);
			PostToFirstReceiveWT postToFirstReceiveWt = new PostToFirstReceiveWT(firstThreadReceive.ReceiveData);
			PostToSecondReceiveWT postToSecondReceiveWt = new PostToSecondReceiveWT(secondThreadReceive.ReceiveData);

			var serializeMessage = Task.Factory.StartNew(() =>
			{
				switch (number)
				{
					case 1:
						Utils.SerializeMessage(data);
						break;
					case 2:
						Utils.SerializeFile(data);
						break;
				}
			});

			serializeMessage.Wait();

			threadFirstSender.Start(postToFirstReceiveWt);
			threadSecondSender.Start(postToSecondReceiveWt);
			threadSecondReceive.Start(postToSecondSenderWt);
			threadFirstReceive.Start(postToFirstSenderWt);

			Console.ReadLine();
		}
	}
}

