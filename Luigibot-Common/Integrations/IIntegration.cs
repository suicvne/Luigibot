using System;
using LuigibotCommon;

namespace Luigibot
{
	/// <summary>
	/// Documents a basic Integration.
	/// </summary>
	public interface IIntegration
	{
		string IntegrationName {get;set;}
		string IntegrationDescription {get;set;}

		bool IsIntegrationRunning {get;set;}

		//event EventHandler<EventArgs> MessageReceived;
		event EventHandler<EventArgs> Connected;

		void StartIntegration();
		void StopIntegration();

		void SendMessage(string text, IChannel target);
	}
}

