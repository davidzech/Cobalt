using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CobaltCore.Network
{
	/// <summary>
	/// A utility class to assist with NAT traversal; specifically, ports may be forwarded through a NAT router to support inbound connections. This class uses UPnP.
	/// </summary>
	public static class NatHelper
	{
		private const string BroadcastPacket = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nST:upnp:rootdevice\r\nMAN:\"ssdp:discover\"\r\nMX:3\r\n\r\n";
		private const string ResponseRootDevice = "upnp:rootdevice";
		private static readonly Regex ResponseLocation = new Regex(@"location:[\s]*(?<location>[^\s]+)");
		private const int MaxTries = 3;
		private const int DiscoverTimeout = 3000;
		private const int DeviceTimeout = 30000;
		private const string TnsNamespaceAlias = "tns";
		private const string TnsNamespace = "urn:schemas-upnp-org:device-1-0";
		private const string DevicePath = "//tns:device/tns:deviceType";
		private const string DeviceType = "InternetGatewayDevice";
		private const string ControlUrlPath = "//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:controlURL";
		private const string SoapAction = "\"urn:schemas-upnp-org:service:WANIPConnection:1#{0}\"";
		private const string ControlNamespace = "urn:schemas-upnp-org:service:WANIPConnection:1";

		private static volatile string _controlUrl;

		/// <summary>
		/// Gets a value indicating whether a UPnP-enabled device has been identified and is available for requests.
		/// </summary>
		public static bool IsAvailable => !string.IsNullOrEmpty(_controlUrl);

	    private class AsyncResult : IAsyncResult
		{
			private volatile bool _isSuccessful;

			public bool IsSuccessful { get { return _isSuccessful; } set { _isSuccessful = value; } }
			public object AsyncState { get; set; }
			public WaitHandle AsyncWaitHandle { get; set; }
			public bool CompletedSynchronously => false;
	        public bool IsCompleted => AsyncWaitHandle.WaitOne(0);
		}

	    public static async Task<bool> DiscoverAsync(object state = null)
	    {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Broadcast, 1900);

            byte[] outBuf = Encoding.ASCII.GetBytes(BroadcastPacket);
            byte[] inBuf = new byte[4096];

	        int tries = MaxTries;
	        while (--tries > 0)
	        {
	            try
	            {
	                TaskFactory factory = new TaskFactory();
	                sock.ReceiveTimeout = DiscoverTimeout;
	                await factory.FromAsync(sock.BeginSendTo(outBuf, 0, outBuf.Length, 0, endpoint, null, null), end =>
	                {
	                    sock.EndSendTo(end);
	                }).ConfigureAwait(false);
	                var ar = sock.BeginReceive(inBuf, 0, inBuf.Length, 0, null, null);
	                if (ar == null) throw new Exception("ar is null");
                    int length = await factory.FromAsync(ar, end => sock.EndReceive(end)).ConfigureAwait(false);

                    string data = Encoding.ASCII.GetString(inBuf, 0, length).ToLowerInvariant();

                    var match = ResponseLocation.Match(data);
                    if (match.Success && match.Groups["location"].Success)
                    {
                        System.Diagnostics.Debug.WriteLine("Found UPnP device at " + match.Groups["location"]);
                        string controlUrl = GetServiceUrl(match.Groups["location"].Value);
                        if (!string.IsNullOrEmpty(controlUrl))
                        {
                            _controlUrl = controlUrl;
                            System.Diagnostics.Debug.WriteLine("Found control URL at " + _controlUrl);
                            return true;                            
                        }
                    }

                }
	            catch (Exception ex)
	            {
	                // ignore timeout exceptions
	                if (!(ex is SocketException && ((SocketException) ex).ErrorCode == 10060))
	                {
	                    System.Diagnostics.Debug.WriteLine(ex.ToString());
	                }
	            }
	        }
            return false;
	    }

        /// <summary>
        /// Asynchronously discover UPnP-enabled gateways on the local network. This method must be called before
        /// attempting any other operation.
        /// </summary>
        /// <param name="callback">An optional callback invoked when the operation is complete.</param>
        /// <param name="state">An optional state parameter supplied to the callback.</param>
        /// <returns>Returns an object which must be passed to EndDiscover.</returns>
        public static IAsyncResult BeginDiscover(AsyncCallback callback = null, object state = null)
		{
			var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
			var endpoint = new IPEndPoint(IPAddress.Broadcast, 1900);

			byte[] outBuf = Encoding.ASCII.GetBytes(BroadcastPacket);
			byte[] inBuf = new byte[4096];

			var ar = new AsyncResult { AsyncWaitHandle = new ManualResetEvent(false), AsyncState = state };

			ThreadPool.QueueUserWorkItem(o =>
				{
					int tries = MaxTries;
					while (--tries >= 0)
					{
						try
						{
							sock.SendTo(outBuf, endpoint);
							sock.ReceiveTimeout = DiscoverTimeout;
							while (true)
							{
								int length = sock.Receive(inBuf);
								string data = Encoding.ASCII.GetString(inBuf, 0, length).ToLowerInvariant();

								var match = ResponseLocation.Match(data);
								if (match.Success && match.Groups["location"].Success)
								{
									System.Diagnostics.Debug.WriteLine("Found UPnP device at " + match.Groups["location"]);
									string controlUrl = GetServiceUrl(match.Groups["location"].Value);
									if (!string.IsNullOrEmpty(controlUrl))
									{
										_controlUrl = controlUrl;
										System.Diagnostics.Debug.WriteLine("Found control URL at " + _controlUrl);
										ar.IsSuccessful = true;
										break;
									}
								}
							}
						}
						catch (Exception ex)
						{
							// ignore timeout exceptions
							if (!(ex is SocketException && ((SocketException)ex).ErrorCode == 10060))
							{
								System.Diagnostics.Debug.WriteLine(ex.ToString());
							}
						}
					}

					((ManualResetEvent)ar.AsyncWaitHandle).Set();
					if (callback != null)
					{
						callback(ar);
					}
				}, null);
			return ar;
		}

		/// <summary>
		/// End the discover operation. If the operation is not yet completed, this method will block until it completes.
		/// </summary>
		/// <param name="ar">The object returned from BeginDiscover.</param>
		/// <returns>Returns true if the discovery was successful, false if no devices were found.</returns>
		public static bool EndDiscover(IAsyncResult ar)
		{
			var result = ar as AsyncResult;
			if(result == null)
			{
				throw new ArgumentException("IAsyncResult is not from this operation.", "ar");
			}
			result.AsyncWaitHandle.WaitOne();
			return result.IsSuccessful;
		}


        public static async Task<bool> AddForwardingRuleAsync(int port, ProtocolType protocol, string description, object state = null)
        {
            if(!IsAvailable)
            {
                throw new ApplicationException("No UPnP devices have been discovered");
            }

            XNamespace ns = ControlNamespace;
            IPAddress address;

            try
            {
                address = (await Dns.GetHostAddressesAsync(Dns.GetHostName())).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            }
            catch
            {
                throw new ApplicationException("Could not determine IPAddress");        
            }

            var elem = new XElement(ns + "AddPortMapping", new XAttribute(XNamespace.Xmlns + "u", ns.NamespaceName),
                new XElement("NewRemoteHost"),
                new XElement("NewExternalPort", port.ToString()),
                new XElement("NewProtocol", protocol.ToString().ToUpperInvariant()),
                new XElement("NewInternalPort", port.ToString()),
                new XElement("NewInternalClient", address.ToString()),
                new XElement("NewEnabled", "1"),
                new XElement("NewPortMappingDescription", description),
                new XElement("NewLeaseDuration", "0"));

            try
            {
                await SoapRequestAsync(_controlUrl, elem, "AddPortMapping");
                System.Diagnostics.Debug.WriteLine("Started forwarding port " + port);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return false;
        }


	    /// <summary>
	    /// Asynchronously submit a request to forward a port to this computer.
	    /// </summary>
	    /// <param name="description"></param>
	    /// <param name="callback">An optional callback invoked when the operation is complete.</param>
	    /// <param name="state">An optional state parameter supplied to the callback.</param>
	    /// <param name="port"></param>
	    /// <param name="protocol"></param>
	    /// <returns>Returns an object which must be passed to EndAddForwardingRule.</returns>
	    public static IAsyncResult BeginAddForwardingRule(int port, ProtocolType protocol, string description, AsyncCallback callback = null, object state = null)
		{
			if (!IsAvailable)
			{
				throw new ApplicationException("No UPnP devices have been discovered.");
			}

			var ar = new AsyncResult { AsyncWaitHandle = new ManualResetEvent(false), AsyncState = state };
			XNamespace ns = ControlNamespace;
			IPAddress address;

			try
			{
				address = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
			}
			catch
			{
				throw new ApplicationException("Could not determine IP address.");
			}

			var elem = new XElement(ns + "AddPortMapping", new XAttribute(XNamespace.Xmlns + "u", ns.NamespaceName),
				new XElement("NewRemoteHost"),
				new XElement("NewExternalPort", port.ToString()),
				new XElement("NewProtocol", protocol.ToString().ToUpperInvariant()),
				new XElement("NewInternalPort", port.ToString()),
				new XElement("NewInternalClient", address.ToString()),
				new XElement("NewEnabled", "1"),
				new XElement("NewPortMappingDescription", description),
				new XElement("NewLeaseDuration", "0"));

			ThreadPool.QueueUserWorkItem(o =>
				{
					try
					{
						SoapRequest(_controlUrl, elem, "AddPortMapping");
						ar.IsSuccessful = true;
						System.Diagnostics.Debug.WriteLine("Started forwarding port " + port);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine(ex.ToString());
					}

					((ManualResetEvent)ar.AsyncWaitHandle).Set();
					if (callback != null)
					{
						callback(ar);
					}
				}, null);
			return ar;
		}

		/// <summary>
		/// End the port forwarding operation. If the operation is not yet completed, this method will block until it completes.
		/// </summary>
		/// <param name="ar">The object returned from BeginAddForwardingRule.</param>
		/// <returns>Returns true if the operation was successful, false otherwise.</returns>
		public static bool EndAddForwardingRule(IAsyncResult ar)
		{
			// no difference
			return EndDiscover(ar);
		}

	    /// <summary>
	    /// Asynchronously submit a request to delete a previously added port forwarding rule.
	    /// </summary>
	    /// <param name="protocol"></param>
	    /// <param name="callback">An optional callback invoked when the operation is complete.</param>
	    /// <param name="state">An optional state parameter supplied to the callback.</param>
	    /// <param name="port"></param>
	    /// <returns>Returns an object which must be passed to EndDeleteForwardingRule.</returns>
	    public static IAsyncResult BeginDeleteForwardingRule(int port, ProtocolType protocol, AsyncCallback callback = null, object state = null)
		{
			if (!IsAvailable)
			{
				throw new ApplicationException("No UPnP devices have been discovered.");
			}

			var ar = new AsyncResult { AsyncWaitHandle = new ManualResetEvent(false), AsyncState = state };
			XNamespace ns = ControlNamespace;

	        try
	        {
	            var ipAddress = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
	        }
	        catch
			{
				throw new ApplicationException("Could not determine IP address.");
			}

			var elem = new XElement(ns + "DeletePortMapping", new XAttribute(XNamespace.Xmlns + "u", ns.NamespaceName),
				new XElement("NewRemoteHost"),
				new XElement("NewExternalPort", port.ToString()),
				new XElement("NewProtocol", protocol.ToString().ToUpperInvariant()));

			ThreadPool.QueueUserWorkItem(o =>
			{
				try
				{
					SoapRequest(_controlUrl, elem, "DeletePortMapping");
					ar.IsSuccessful = true;
					System.Diagnostics.Debug.WriteLine("Stopped forwarding port " + port);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}

				((ManualResetEvent)ar.AsyncWaitHandle).Set();
				if (callback != null)
				{
					callback(ar);
				}
			}, null);
			return ar;
		}

		/// <summary>
		/// End the port forwarding operation. If the operation is not yet completed, this method will block until it completes.
		/// </summary>
		/// <param name="ar">The object returned from BeginDeleteForwardingRule.</param>
		/// <returns>Returns true if the operation was successful, false otherwise.</returns>
		public static bool EndDeleteForwardingRule(IAsyncResult ar)
		{
			// no difference
			return EndAddForwardingRule(ar);
		}

        private static async Task<XDocument> SoapRequestAsync(string url, XElement content, string function)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "s", soap.NamespaceName),
                new XAttribute(soap + "encodingStyle", "http://schemas.xmlsoap.org/soap/encoding/"),
                new XElement(soap + "Body", content)));
            var req = HttpWebRequest.Create(_controlUrl);
            req.Method = "POST";
            byte[] buf = Encoding.UTF8.GetBytes(doc.ToString());
            req.Headers.Add("SOAPACTION", string.Format(SoapAction, function));
            req.ContentType = "text/xml; charset=\"utf-8\"";
            req.ContentLength = buf.Length;

            var stream = await req.GetRequestStreamAsync();

            await stream.WriteAsync(buf, 0, buf.Length);
            var response = await req.GetResponseAsync();

            return XDocument.Load(response.GetResponseStream());
        }

		private static XDocument SoapRequest(string url, XElement content, string function)
		{
			XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
			var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
				new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "s", soap.NamespaceName),
				new XAttribute(soap + "encodingStyle", "http://schemas.xmlsoap.org/soap/encoding/"),
				new XElement(soap + "Body", content)));
			var req = HttpWebRequest.Create(_controlUrl);
			req.Method = "POST";
			byte[] buf = Encoding.UTF8.GetBytes(doc.ToString());
			req.Headers.Add("SOAPACTION", string.Format(SoapAction, function));
			req.ContentType = "text/xml; charset=\"utf-8\"";
			req.ContentLength = buf.Length;
			var ar = req.BeginGetRequestStream(null, null);
			if (!ar.AsyncWaitHandle.WaitOne(DeviceTimeout))
			{
				throw new ApplicationException("Timeout sending SOAP request.");
			}
			var stream = req.EndGetRequestStream(ar);
			stream.Write(buf, 0, buf.Length);
			ar = req.BeginGetResponse(null, null);
			if (!ar.AsyncWaitHandle.WaitOne(DeviceTimeout))
			{
				throw new ApplicationException("Timeout sending SOAP request.");
			}
			var response = req.EndGetResponse(ar);
			return XDocument.Load(response.GetResponseStream());
		}

	    private static async Task<string> GetServiceUrlAsync(string url)
	    {
	        var request = WebRequest.Create(url);
	        var response = await request.GetResponseAsync();
	        var doc = XDocument.Load(response.GetResponseStream());
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace(TnsNamespaceAlias, TnsNamespace);
            var node = doc?.Root?.XPathSelectElement(DevicePath, ns);
            if (node == null)
            {
                throw new ApplicationException("Could not determine device type.");
            }
            if (node.Value.IndexOf(DeviceType, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return null;
            }
            node = doc.Root.XPathSelectElement(ControlUrlPath, ns);
            if (node == null)
            {
                throw new ApplicationException("Could not find control URL.");
            }
            if (node.Value.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return node.Value;
            }
            else if (node.Value.StartsWith("/"))
            {
                var uri = new Uri(url);
                return $"http://{uri.Host}:{uri.Port.ToString()}{node.Value}";
            }
            else
            {
                return url + node.Value;
            }
        }

		private static string GetServiceUrl(string url)
		{
			var request = WebRequest.Create(url);
			var ar = request.BeginGetResponse(null, null);
			if (!ar.AsyncWaitHandle.WaitOne(DeviceTimeout))
			{
				throw new ArgumentException("UPnP device timeout.");
			}

			var doc = XDocument.Load(request.EndGetResponse(ar).GetResponseStream());
			var ns = new XmlNamespaceManager(new NameTable());
			ns.AddNamespace(TnsNamespaceAlias, TnsNamespace);
		    var node = doc?.Root?.XPathSelectElement(DevicePath, ns);
		    if (node == null)
			{
				throw new ApplicationException("Could not determine device type.");
			}
			if (node.Value.IndexOf(DeviceType, StringComparison.OrdinalIgnoreCase) < 0)
			{
				return null;
			}
			node = doc.Root.XPathSelectElement(ControlUrlPath, ns);
			if (node == null)
			{
				throw new ApplicationException("Could not find control URL.");
			}
			if (node.Value.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
			{
				return node.Value;
			}
			else if (node.Value.StartsWith("/"))
			{
				var uri = new Uri(url);
				return $"http://{uri.Host}:{uri.Port.ToString()}{node.Value}";
			}
			else
			{
				return url + node.Value;
			}
		}
	}
}
