using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CobaltCore.Irc
{
    public enum IrcCodeHandlerPriority
    {
        Lowest = 0,
        Low = 1,
        Normal = 2,
        High = 3,
        Highest = 4
    }

	/// <summary>
	/// This class represents a handler for a specific IRC code value. It can be used to "intercept" a response
	/// to a command and prevent other components from processing it.
	/// </summary>
	public sealed class IrcCodeHandler
	{
		internal IrcCode[] Codes { get; private set; }
		internal Func<IrcInfoEventArgs, Task<bool>> Handler { get; private set; }
        internal IrcCodeHandlerPriority Priority { [UsedImplicitly] get; private set; }

        /// <summary>
        /// Primary Constructor
        /// </summary>
        /// <param name="handler">Code Handler to handle the stuff.</param>
        /// <param name="priority"></param>
        /// <param name="codes"></param>
		public IrcCodeHandler(Func<IrcInfoEventArgs, Task<bool>> handler, IrcCodeHandlerPriority priority, params IrcCode[] codes)
		{
			Handler = handler;
			Codes = codes;
		    Priority = priority;
		}
	}
}
