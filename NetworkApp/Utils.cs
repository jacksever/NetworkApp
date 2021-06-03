using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NetworkApp
{
	public static class Utils
	{
		public static int FrameLength = 56;
		public static int Index = 0;

		public static Encoding Encoding = Encoding.UTF8;
		public static bool[][] Result;

		public static List<bool> BitArray = new List<bool>();

		public static byte[] BitArrayToByteArray(BitArray data)
		{
			if (data == null)
				return null;

			byte[] array = new byte[(data.Length - 1) / 8 + 1];
			data.CopyTo(array, 0);
			return array;
		}

		public static object DeserializeObject(byte[] allBytes)
		{
			if (allBytes == null)
				return null;

			using var stream = new MemoryStream(allBytes);
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
			using var ms = new MemoryStream();
			bf.Serialize(ms, obj);
			return ms.ToArray();
		}

		public static int CheckSum(bool[] array)
		{
			int checkSum = 0;
			for (int fr = 0; fr < array.Length; fr++)
				if (fr % 5 == 0)
					checkSum += array[fr] == false ? 0 : 1;

			return checkSum;
		}

		public static void SerializeData(string message)
        {
			var bits = new BitArray(Encoding.GetBytes(message));

			bool[] values = new bool[bits.Count];
			for (int m = 0; m < bits.Count; m++)
				values[m] = bits[m];

			int j = 0;
			Result = values.GroupBy(s => j++ / FrameLength).Select(g => g.ToArray()).ToArray();
		}
	}
}
