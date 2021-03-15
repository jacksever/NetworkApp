using System.Collections;
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

		public SecondThread(ref Semaphore sendSemaphore, ref Semaphore receiveSemaphore, Encoding encoding)
		{
			_sendSemaphore = sendSemaphore;
			_receiveSemaphore = receiveSemaphore;
			_encoding = encoding;
		}

		public void SecondThreadMain(object obj)
		{
			string text = null;
			_post = (PostToFirstWT)obj;

			ConsoleHelper.WriteToConsole("2 поток", "Начинаю работу. Жду передачи данных.");
			_receiveSemaphore.WaitOne();

			if (_receivedMessage.Length > 0)
			{
				text = _encoding.GetString(BitArrayToByteArray(_receivedMessage));
				ConsoleHelper.WriteToConsole("2 поток", "Полученные данные: " + text);

				_post(true);
				_sendSemaphore.Release();
				ConsoleHelper.WriteToConsole("2 поток", "Заканчиваю работу");
			}
			else
			{
				ConsoleHelper.WriteToConsole("2 поток", "Сообщение пустое..");
				_post(false);
				_sendSemaphore.Release();
				ConsoleHelper.WriteToConsole("2 поток", "Заканчиваю работу");
			}
		}

		public void ReceiveData(BitArray array)
		{
			_receivedMessage = array;
		}

		public byte[] BitArrayToByteArray(BitArray data)
		{
			byte[] array = new byte[(data.Length - 1) / 8 + 1];
			data.CopyTo(array, 0);
			return array;
		}
	}
}
