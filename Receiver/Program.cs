using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using Shared;

namespace MtConcurrencyTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Waiting to consume. Press any key to exit.");


			var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
			{
				var host = sbc.Host(new Uri("rabbitmq://localhost/"), h =>
				{
					h.Username(UserInfo.Name);
					h.Password(UserInfo.Password);
					h.PublisherConfirmation = false;
					h.Heartbeat(0);
				});

				sbc.UseBinarySerializer();


				sbc.UseRetry(retry => retry.None());
				sbc.Durable = false;
				sbc.PrefetchCount = 100;

				sbc.ReceiveEndpoint(host, "somequeue", ep =>
				{
					ep.UseConcurrencyLimit(1);
					ep.PrefetchCount = 100;
					ep.PurgeOnStartup = true;
					ep.Durable = false;

					ep.Instance(new ConsumesMessage1());
					ep.Instance(new ConsumesMessage2());
				});

			});

			bus.StartAsync();

			Console.ReadKey();
		}
	}


	public class ConsumesMessage1 : IConsumer<Message1>
	{
		public Task Consume(ConsumeContext<Message1> context)
		{
			Console.WriteLine($"{DateTime.Now:O} BEGIN - Consume message 1");
			Thread.Sleep(5000);
			Console.WriteLine($"{DateTime.Now:O} END - Consume message 1");

			return Task.CompletedTask;
		}
	}

	public class ConsumesMessage2 : IConsumer<Message2>
	{
		public Task Consume(ConsumeContext<Message2> context)
		{
			Console.WriteLine($"{DateTime.Now:O} BEGIN - Consuming message 2");
			Thread.Sleep(200);
			Console.WriteLine($"{DateTime.Now:O} END - Consuming message 2");

			return Task.CompletedTask;
		}
	}
}
