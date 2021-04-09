using System;
using System.Collections;

namespace NetworkApp
{
	[Serializable]
	public class Frame
	{	
		public Frame() { }

		public Frame(int id, BitArray array, int checkSum, int useful)
		{
			Id = id;
			Body = array;
			CheckSum = checkSum;
			UsefulData = useful;
		}

		public int Id { get; set; }
		public BitArray Body { get; set; }
		public int CheckSum { get; set; }
		public int UsefulData { get; set; }
	}
}
