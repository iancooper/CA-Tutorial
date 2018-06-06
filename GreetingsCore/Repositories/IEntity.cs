using System;

namespace GreetingsCore.Repositories
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}