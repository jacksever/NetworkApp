using System;
using System.Collections;

namespace NetworkApp
{
	[Serializable]
	public class Receipt
	{
		public Receipt() { }

		public Receipt(int id, BitArray status)
		{
			Id = id;
			Status = status;
		}

		public Receipt(BitArray status)
		{
			Status = status;
		}

		public int Id { get; set; }
		public BitArray Status { get; set; }
	}
}
