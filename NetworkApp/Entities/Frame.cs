using System;
using System.Collections;

namespace NetworkApp
{
	[Serializable]
	public class Frame
	{
		public Frame() { }

		public Frame(int id, BitArray body, uint checkSum, int usefulData, BitArray status, int? repeatIndex)
		{
			Id = id;
			Body = body;
			CheckSum = checkSum;
			UsefulData = usefulData;
			Status = status;
			RepeatIndex = repeatIndex;
		}

		public Frame(BitArray status)
		{
			Status = status;
		}

		public int Id { get; set; }
		public BitArray Body { get; set; }
		public uint CheckSum { get; set; }
		public int UsefulData { get; set; }
		public BitArray Status { get; set; }
		public int? RepeatIndex { get; set; }
	}
}
