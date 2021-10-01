using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet
{
	public class AlgorithmPackingResult
	{
		public AlgorithmPackingResult()
		{
			this.PackedItems = new List<Item>();
			this.UnpackedItems = new List<Item>();
		}

		public int AlgorithmID { get; set; }

		public string AlgorithmName { get; set; }

		public bool IsCompletePack { get; set; }

		public List<Item> PackedItems { get; set; }

		public long PackTimeInMilliseconds { get; set; }

		public decimal PercentContainerVolumePacked { get; set; }

		public decimal PercentItemVolumePacked { get; set; }

		public List<Item> UnpackedItems { get; set; }
	}
	public class Item
	{

		private decimal volume;

		public Item(int id, decimal dim1, decimal dim2, decimal dim3, int quantity)
		{
			this.ID = id;
			this.Dim1 = dim1;
			this.Dim2 = dim2;
			this.Dim3 = dim3;
			this.volume = dim1 * dim2 * dim3;
			this.Quantity = quantity;
		}


		public int ID { get; set; }
		public bool IsPacked { get; set; }
		public decimal Dim1 { get; set; }
		public decimal Dim2 { get; set; }

		public decimal Dim3 { get; set; }
		public decimal CoordX { get; set; }
		public decimal CoordY { get; set; }
		public decimal CoordZ { get; set; }
		public int Quantity { get; set; }
		public decimal PackDimX { get; set; }

		public decimal PackDimY { get; set; }

		public decimal PackDimZ { get; set; }

		public decimal Volume
		{
			get
			{
				return volume;
			}
		}
	}
	public class ContainerPackingResult
	{
		public ContainerPackingResult()
		{
			this.AlgorithmPackingResults = new List<AlgorithmPackingResult>();
		}

		public int ContainerID { get; set; }

		public List<AlgorithmPackingResult> AlgorithmPackingResults { get; set; }
	}
	public class Container
	{
		private decimal volume;
		public Container(int id, decimal length, decimal width, decimal height)
		{
			this.ID = id;
			this.Length = length;
			this.Width = width;
			this.Height = height;
			this.Volume = length * width * height;
		}

		public int ID { get; set; }

		public decimal Length { get; set; }

		public decimal Width { get; set; }

		public decimal Height { get; set; }

		public decimal Volume
		{
			get
			{
				return this.volume;
			}
			set
			{
				this.volume = value;
			}
		}
	}
}
