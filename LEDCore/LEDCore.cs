using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using Newtonsoft.Json;
using System.Text;
using System.Threading;


namespace LEDCore
{
	public class LEDCore
	{
		public const byte SYNC = 0x55;
		public const byte ESC = 0xEE;

		private Thread playThread;
		//public Queue AnimationQueue {get;set;}
		public List<Animation> Animations { get; set; }
		public string Port { get; private set; }
		private SerialPort serialPort;
		public int BaudRate { get; private set; }
		private LEDCore ()
		{
			this.Animations = new List<Animation> ();
			this.BaudRate = 9600;
		}

		public LEDCore (string port, Animation animation) : this()
		{
			this.Port = port;
			this.Animations.Add (animation);
		}

		public LEDCore (string port, int baudRate, Animation animation) : this(port, animation)
		{
			this.BaudRate = baudRate;
		}

		public LEDCore (string port, IList<Animation> animations) : this()
		{
			this.Port = port;
			this.Animations.AddRange (animations);
		}

		public LEDCore (string port, int baudRate, IList<Animation> animations) : this(port, animations)
		{
			this.BaudRate = baudRate;
		}

		public void PlayAsync()
		{
			playThread = new Thread (play);
			playThread.Start ();
		}

		private void play(){
			foreach (var animation in Animations) {
				Console.WriteLine (animation.Name + " is now playing");
				animation.AutoRepeat = false;
				animation.Play ();
				animation.PlayThread.Join ();
			}
		}

		public void PlayAsync(Animation animation)
		{
			Console.WriteLine (animation.Name + " is now playing");
			animation.Play ();
		}

		public void Play(Animation animation)
		{
			Console.WriteLine (animation.Name + " is now playing");
			animation.Play ();
			animation.PlayThread.Join ();
		}

		public void Load(StreamReader reader) 
		{
			this.Animations =  JsonConvert.DeserializeObject<List<Animation>> (/*Convert.FromBase64String(*/reader.ReadToEnd()/*)*/);
			foreach (var animation in Animations) 
			{
				animation.PlayThread.Abort ();
				animation.PlayThread = null;
				animation.Writer.Dispose ();
			}
		}

		public void Save(ref StreamWriter output)
		{
			output.Write (/*Convert.ToBase64String (Encoding.Default.GetBytes (*/JsonConvert.SerializeObject (this.Animations))/*))*/;
		}

		public bool Connect()
		{

			if (serialPort == null) {
				serialPort = new SerialPort (Port, BaudRate);
				serialPort.Open ();
				foreach (var animation in Animations) {
					animation.Writer = new BinaryWriter (serialPort.BaseStream);
				}
				 
			}
			return true;
		}
	}
}

