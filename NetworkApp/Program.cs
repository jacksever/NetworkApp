using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkApp
{
	public delegate void PostToFirstWT(BitArray message);
	public delegate void PostToSecondWT(BitArray message);

	class Program
	{
		static void Main()
		{
			ConsoleHelper.WriteToConsole("Главный поток", "Введите Ваше сообщение...");
			var data = Console.ReadLine();

			Semaphore firstReceiveSemaphore = new Semaphore(0, 1);
			Semaphore secondReceiveSemaphore = new Semaphore(0, 1);

			FirstThreadSender firstThread = new FirstThreadSender(ref secondReceiveSemaphore, ref firstReceiveSemaphore);
			FirstThreadReceive secondThread = new FirstThreadReceive(ref firstReceiveSemaphore, ref secondReceiveSemaphore);

			Thread threadFirst = new Thread(new ParameterizedThreadStart(firstThread.FirstThreadMain));
			Thread threadSecond = new Thread(new ParameterizedThreadStart(secondThread.SecondThreadMain));

			PostToFirstWT postToFirstWt = new PostToFirstWT(firstThread.ReceiveData);
			PostToSecondWT postToSecondWt = new PostToSecondWT(secondThread.ReceiveData);

			var serializeMessage = Task.Factory.StartNew(() =>
			{
				Utils.SerializeData(data);
			});

			serializeMessage.Wait();

			threadFirst.Start(postToSecondWt);
			threadSecond.Start(postToFirstWt);

			Console.ReadLine();
		}
	}
}

