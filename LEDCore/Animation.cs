using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Dynamic.Utils;
using System.Linq;


namespace LEDCore
{
	public class Animation : IDisposable
	{

		public string Name { get; set; }
		public List<Frame> Frames { get; set; }
		public BinaryWriter Writer { get; set; }
		public bool AutoRepeat { get; set; }
		public int CurrentFrame { get; set; }
		public ushort CubeSize { get; private set; }
		public TimeSpan TotalAnimationTime { 
			get{
				return TimeSpan.FromSeconds(Frames.Select (frame => frame.FrameTime).Sum ());
			} 
			set{
				double time = value.TotalSeconds / Frames.Count;
				foreach (var frame in Frames) {
					frame.FrameTime = time;
				}
			}
		}
		private bool breakAnimation = false;
		public Thread PlayThread { get; set; }
	
		public int PackageSize {
			get
			{ 
				return ((int)(Math.Pow(CubeSize, 2) + CubeSize) / 8);
			}
		}

		public Animation (ushort cubeSize)
		{
			AutoRepeat = true;
			this.CubeSize = cubeSize;
		}

		public Animation (ushort cubeSize, IList<Frame> frames) : this(cubeSize)
		{
			this.Frames = (List<Frame>)frames;
		}

		public Animation (ushort cubeSize, IList<Frame> frames, float overrideFrameTime) : this(cubeSize, frames)
		{
			foreach (var frame in frames) {
				frame.FrameTime = overrideFrameTime;
			}
		}

		public void Play() 
		{
			PlayThread = new Thread (play);
			PlayThread.Name = Name + ".Play";
			PlayThread.Start ();
		}

		private void play()
		{
			do 
			{
				long start = DateTime.Now.Ticks;
				int f = CurrentFrame;
				Writer.Write(Frames[f].Data); 				
				// Sleep but subtract the overhead (stuff above)...
				Thread.Sleep(TimeSpan.FromTicks(TimeSpan.FromSeconds(Frames[f].FrameTime).Ticks - (DateTime.Now.Ticks - start)));
				f++;
			} while (AutoRepeat && !breakAnimation);
		}

		public void Pause()
		{
			breakAnimation = true;
			PlayThread.Join (TimeSpan.FromSeconds (Frames [CurrentFrame].FrameTime));
			PlayThread.Abort ();
			PlayThread = null;
		}


 
		#region IDisposable implementation
		public void Dispose ()
		{
			//TODO Build in
			this.Writer.Dispose ();
			
		}
		#endregion
	}
}

