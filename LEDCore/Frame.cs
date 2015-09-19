using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace LEDCore
{
	public class Frame : IDisposable
	{
		public Animation AssociatedAnimation { get; set; }

		#region IDisposable implementation

		public void Dispose ()
		{
			this.AssociatedAnimation = null;
			this.FrameData = null;
		}

		#endregion

		public Frame ()
		{

		}

		public Frame(bool[,,] frameData) {		
			this.FrameData = frameData;
			this.FrameTime = 1.0f;
		}

		public Frame(bool[,,] frameData, float timeVisible) : this(frameData){
			this.FrameTime = timeVisible;
		}

		public bool[,,] FrameData {
			get;
			private set;
		}

		public double FrameTime { 
			get; 
			set; 		
		}

		private struct Location
		{
			public int x;
			public int y;
			public int layer;
		}

		public byte[] Data
		{
			get 
			{
				Debug.WriteLine ("SYNC");
				int n = AssociatedAnimation.CubeSize;
				bool[] bitBuffer = new bool[(int)Math.Pow(n,3)];
				byte[] buffer = new byte[bitBuffer.Length / 8 + 2];
				buffer [0] = LEDCore.ESC;
				buffer [1] = LEDCore.SYNC;
				for(int layer = 0; layer < n; layer++)
				{
					for(int x = 0; x < n; x++) 
					{
						for (int y = 0; y < n; y++) 
						{	
							int ledPos = layer + x + (y * n) + 2;
							int val = FrameData [x, y, layer] ? 1 : 0;
							buffer[ledPos / 8] = (byte)(val << (8 - (ledPos % 8)));
						}
					}							          
				}
				int pos = 0;
				List<byte> workspace = new List<byte> (buffer);
				foreach (byte b in buffer) {
					if (b == LEDCore.ESC) {
						workspace.Insert (++pos, 0x00);
						Debug.WriteLine ("ESC @ " + pos);
					}
					pos++;
				}
				return buffer;
			}
		}

	}
}

