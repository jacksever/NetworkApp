using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetworkApp
{
	public static class Utils
	{
		public static int FrameLength = 56;

		public static byte[] BitArrayToByteArray(BitArray data)
		{
			byte[] array = new byte[(data.Length - 1) / 8 + 1];
			data.CopyTo(array, 0);
			return array;
		}

		public static object DeserializeObject(byte[] allBytes)
		{
			using (var stream = new MemoryStream(allBytes))
				return DeserializeFromStream(stream);
		}

		private static object DeserializeFromStream(MemoryStream stream)
		{
			IFormatter formatter = new BinaryFormatter();
			stream.Seek(0, SeekOrigin.Begin);

			return formatter.Deserialize(stream);
		}

		public static byte[] SerializeObject(object obj)
		{
			BinaryFormatter bf = new BinaryFormatter();
			using (var ms = new MemoryStream())
			{
				bf.Serialize(ms, obj);
				return ms.ToArray();
			}
		}

		public static int CheckSum(bool[] array)
        {
			int checkSum = 0;
			for (int fr = 0; fr < array.Length; fr++)
				if (fr % 5 == 0)
					checkSum += array[fr] == false ? 0 : 1;

			return checkSum;
		}
	}
}
