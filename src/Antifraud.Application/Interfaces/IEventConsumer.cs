using System;
using System.Threading.Tasks;

namespace Antifraud.Application.Interfaces;

public interface IEventConsumer
{
    Task StartAsync(string topic, Func<string, Task> messageHandler);
    Task StopAsync();
}