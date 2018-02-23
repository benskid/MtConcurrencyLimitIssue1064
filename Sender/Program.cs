using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using Shared;

namespace Sender
{
	class Program
	{
		static void Main(string[] args)
		{
			var bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(sbc =>
			{
				var host = sbc.Host(new Uri("rabbitmq://localhost/"), h =>
				{
					h.Username(UserInfo.Name);
					h.Password(UserInfo.Password);
					h.PublisherConfirmation = false;
					h.Heartbeat(0);
				});

				sbc.PrefetchCount = 100;
				sbc.UseConcurrencyLimit(1);
				sbc.UseBinarySerializer();
			});

			bus.StartAsync().Wait();

			Console.WriteLine("Press escape to exit. Press any key to send.");
			while (Console.ReadKey().Key != ConsoleKey.Escape)
			{
				bus.Publish(new Message1());
				bus.Publish(new Message2());
			}
		}
	}
}
