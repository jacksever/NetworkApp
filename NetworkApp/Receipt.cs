using System;
using System.Collections;

namespace NetworkApp
{
	[Serializable]
	public class Receipt
	{
		public Receipt() { }

		public Receipt(int id, BitArray array)
		{
			Id = id;
			Status = array;
		}

		public int Id { get; set; }
		public BitArray Status { get; set; }
	}
}
