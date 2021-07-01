using System;

namespace GreetingsApp.Repositories
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}