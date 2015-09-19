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
		public int CubeSize { get; private set; }

		public long TimeWaitAfter { get; set; }
		public long TimeWaitBefore { get; set; }

		public TimeSpan TotalAnimationTime { 
			get{
				return TimeSpan.FromSeconds(Frames.Select (frame => frame.FrameTime).Sum ());
			} 
			set{
				long time = value.Ticks / Frames.Count;
				foreach (var frame in Frames) {
					frame.FrameTime = time;
				}
			}
		}
		private bool breakAnimation = false;
		public Thread PlayThread { get; set; }

		public Animation (int cubeSize)
		{
			if (cubeSize > 16 | cubeSize < 2) {
				throw new Exception ("Invalid size");
			}
			AutoRepeat = true;
			this.CubeSize = cubeSize;
		}

		public Animation (ushort cubeSize, IList<Frame> frames) : this(cubeSize)
		{
			this.Frames = (List<Frame>)frames;
		}

		public Animation (ushort cubeSize, IList<Frame> frames, long overrideFrameTime) : this(cubeSize, frames)
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
			Thread.Sleep (TimeSpan.FromTicks (TimeWaitBefore));
			do 
			{
				long start = DateTime.Now.Ticks;
				int f = CurrentFrame;
				Frames[f].SendData(this.Writer.BaseStream);
				// Sleep but subtract the overhead (stuff above)...
				Thread.Sleep(TimeSpan.FromTicks(Frames[f].FrameTime - (DateTime.Now.Ticks - start)));
				f++;
			} while (CurrentFrame < Frames.Count & AutoRepeat && !breakAnimation);
			Thread.Sleep (TimeSpan.FromTicks (TimeWaitAfter));
		}

		public void Pause()
		{
			throw new NotImplementedException ();
			/*
			breakAnimation = true;
			//PlayThread.Join (TimeSpan.FromSeconds (Frames [CurrentFrame].FrameTime));
			PlayThread.Abort ();
			PlayThread = null;
			*/
		}
 
		#region IDisposable implementation
		public void Dispose ()
		{
			this.Writer.Dispose ();
		}
		#endregion
	}
}

