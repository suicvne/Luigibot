using System;
using LuigibotCommon;
using LuigibotCommon.Integrations;

namespace LuigibotCommon.Integrations
{
	/// <summary>
	/// Documents a basic Integration.
	/// </summary>
	public interface IIntegration
	{
        /// <summary>
        /// A simple, one word name for your integration (usually the name of the service)
        /// </summary>
		string IntegrationName {get;set;}
		string IntegrationDescription {get;set;}

		bool IsIntegrationRunning {get;set;}

		event EventHandler<IMessageReceivedEventArgs> MessageReceived;
        event EventHandler<IMessageReceivedEventArgs> MentionReceived;
        event EventHandler<IErrorReceivedEventArgs> ErrorReceived;
        event EventHandler<IConnectionClosedEventArgs> ConnectionClosed;
		event EventHandler<EventArgs> Connected;

        ConsoleColor LogColor { get; set; }

		void StartIntegration();
		void StopIntegration();

        string BoldText(string text);
        string ItalicizeText(string text);

		void SendMessage(string text, IChannel target);
	}
}

