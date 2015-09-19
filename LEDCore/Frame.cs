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
			this.FrameData = null;
		}

		#endregion

		public Frame ()
		{

		}

		public Frame(bool[,,] frameData) {		
			this.FrameData = frameData;
			this.FrameTime = 10000;
		}

		public Frame(bool[,,] frameData, long timeVisible) : this(frameData){
			this.FrameTime = timeVisible;
		}

		public bool[,,] FrameData {
			get;
			private set;
		}
		/// <summary>
		/// Time for a single frame in ticks
		/// 
		/// Ticks per second              10,000,000
		///	Ticks per millisecond             10,000
		/// </summary>
		/// <value>The frame time.</value>
		public long FrameTime { 
			get; 
			set; 		
		}

		public void SendData(System.IO.Stream stream)
		{
			int n = AssociatedAnimation.CubeSize;
			stream.WriteByte (LEDCore.ESC);
			stream.WriteByte (LEDCore.SYNC);
		
			for(int layer = 0; layer < n; layer++)
			{
				for(int x = 0; x < n; x++) 
				{

					for (int y = 0; y < n; y++) 
					{	
						if (FrameData [layer, x, y]) {

							//TODO: fix maxsize from 16x16x16 to nxnxn (split to multiple bytes)
							if ((byte)layer == LEDCore.ESC) {
								stream.WriteByte (LEDCore.ESC);
							}

							stream.WriteByte ((byte)layer);

							if ((byte)(x + n * y) == LEDCore.ESC) {
								stream.WriteByte (LEDCore.ESC);
							}

							stream.WriteByte ((byte)(x + n * y));
						}
					}
				}							          
			}
		}
	}
}

