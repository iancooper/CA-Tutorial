using System;

namespace GreetingsCore.Ports.Repositories
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}