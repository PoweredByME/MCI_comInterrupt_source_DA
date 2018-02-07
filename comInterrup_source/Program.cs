using System;
using System.Net;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.IO;

/// <summary>
/// This code is the source desktop API of the 
/// MCI system. Here a localhost is created and
/// data from the browser is awaited. Upon Successfullly
/// getting the data from the browser, an ack. char. is
/// sent to the browser i.e "R".
/// </summary>


namespace comInterpt
{
	class MainClass
	{

		const string webPort = "3333";
		const string Prefix = "http://localhost:"+webPort+"/";
		/// <summary>
		/// This is the function which gets called when the data is recieved by the 
		/// desktop API
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="z">The z coordinate.</param>
		/// <param name="u">The u coordinate.</param>
		/// <param name="v">The v coordinate.</param>
		/// <param name="w">The w coordinate.</param>
		public static void onRecieveCtrlData(int x, int y, int z, int u, int v, int w, int LP, int RP){   // LP = left pressure, RP = right pressure
			Console.WriteLine ("X = " + x.ToString ());
			Console.WriteLine ("Y = " + y.ToString ());
			Console.WriteLine ("Z = " + z.ToString ());
			Console.WriteLine ("U = " + u.ToString ());
			Console.WriteLine ("V = " + v.ToString ());
			Console.WriteLine ("W = " + w.ToString ());
			Console.WriteLine ("LP = " + LP.ToString ());
			Console.WriteLine ("RP = " + RP.ToString ());
		}











		static HttpListener Listener = null;
		static int RequestNumber = 0;
		static readonly DateTime StartupDate = DateTime.UtcNow;

		public static void Main(string[] args)
		{
			if (!HttpListener.IsSupported)
			{
				Console.WriteLine("HttpListener is not supported on this platform.");
				return;
			}
			using (Listener = new HttpListener())
			{

				Listener.Prefixes.Add(Prefix);
				Listener.Start();
				// Begin waiting for requests.
				Listener.BeginGetContext(GetContextCallback, null);
				Console.WriteLine("Listening to http://localhost:" + webPort + "/");
				Console.WriteLine("Close the application to end the session.");
				Console.WriteLine ("Listening Now : ");
				for (;;) {};
			}
		}

		static void GetContextCallback(IAsyncResult ar)
		{
			try{
				int req = ++RequestNumber;

				// Get the context
				var context = Listener.EndGetContext(ar);

				// listen for the next request
				Listener.BeginGetContext(GetContextCallback, null);

				// get the request
				var NowTime = DateTime.UtcNow;

				Console.WriteLine("{0}: {1}", NowTime.ToString("R"), context.Request.RawUrl);
					
				var request = context.Request;
				string text;
				using (var reader = new StreamReader(request.InputStream,request.ContentEncoding))
				{
					text = reader.ReadToEnd();
					Console.WriteLine(text);
					if(text.Contains(",")){
						var tarr = text.Split (",".ToCharArray());
						if(tarr.Length == 8){
							onRecieveCtrlData(
								
								int.Parse(tarr[0]),
								int.Parse(tarr[1]),
								int.Parse(tarr[2]),
								int.Parse(tarr[3]),
								int.Parse(tarr[4]),
								int.Parse(tarr[5]),
								int.Parse(tarr[6]),
								int.Parse(tarr[7])
							);
						}
					}
				}

				var responseString = string.Format("R");
				byte[] buffer = Encoding.UTF8.GetBytes(responseString);
				var response = context.Response;
				response.ContentType = "text/html";
				response.ContentLength64 = buffer.Length;
				response.StatusCode = 200;
				response.Headers.Remove("Access-Control-Allow-Origin");
				response.AddHeader("Access-Control-Allow-Origin", "*");
				response.OutputStream.Write(buffer, 0, buffer.Length);
				response.OutputStream.Close(); 
			}catch(Exception ex){
				Console.WriteLine (ex.Message);
			}
		}


	}
}
