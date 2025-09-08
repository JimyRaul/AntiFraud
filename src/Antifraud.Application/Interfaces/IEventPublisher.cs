using System.Threading.Tasks;

namespace Antifraud.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string topic) where T : class;
}