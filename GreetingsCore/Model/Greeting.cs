using System;
using GreetingsCore.Ports.Repositories;

namespace GreetingsCore.Model
{
    public class Greeting : IEntity
    {
        public Guid Id { get; set; } 
        public string Message { get; set; }
    }
}