using System.Collections;
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
		private bool _receivedMessage;
		private Encoding _encoding;

		public FirstThread(ref Semaphore sendSemaphore, ref Semaphore receiveSemaphore, Encoding encoding)
		{
			_sendSemaphore = sendSemaphore;
			_receiveSemaphore = receiveSemaphore;
			_encoding = encoding;
		}

		public void FirstThreadMain(object obj)
		{
			_post = (PostToSecondWT)obj;
			ConsoleHelper.WriteToConsole("1 поток", "Начинаю работу. Готовлю данные для передачи.");
			_sendMessage = "Привет! Я студент РУТ(МИИТ)!";
			_post(ToBinaryString(_sendMessage));
			_sendSemaphore.Release();

			ConsoleHelper.WriteToConsole("1 поток", "Данные переданы");
			ConsoleHelper.WriteToConsole("1 поток", "Жду результата");
			_receiveSemaphore.WaitOne();

			if (_receivedMessage)
				ConsoleHelper.WriteToConsole("1 поток", "Всё ок. Завершаю работу.");
			else
				ConsoleHelper.WriteToConsole("1 поток", "Что-то пошло не так...");
		}

		public void ReceiveData(bool state)
		{
			_receivedMessage = state;
		}

		public BitArray ToBinaryString(string text)
		{
			return new BitArray(_encoding.GetBytes(text));
		}

	}
}
