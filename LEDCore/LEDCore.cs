using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using Newtonsoft.Json;
using System.Text;


namespace LEDCore
{
	public class LEDCore
	{
		//Should not be 0x00
		public const byte SYNC = 0xff;
		public const byte ESC = 0xAE;

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

		public void Play()
		{
			foreach (var animation in Animations) {
				Console.WriteLine (animation.Name + " is now playing");
				animation.Play ();
				animation.PlayThread.Join ();
			}
		}

		public void Load(StreamReader reader) 
		{
			//this = JsonConvert.DeserializeObject<LEDCore> (reader);
			foreach (var animation in Animations) 
			{
				animation.PlayThread.Abort ();
				animation.PlayThread = null;
				animation.Writer.Dispose ();
			}
		}

		public void Save(ref StreamWriter output)
		{
			output.Write (Convert.ToBase64String (Encoding.Default.GetBytes (JsonConvert.SerializeObject (this))));
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

