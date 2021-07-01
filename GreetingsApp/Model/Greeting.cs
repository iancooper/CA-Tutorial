using System;
using GreetingsApp.Repositories;

namespace GreetingsApp.Model
{
    public class Greeting : IEntity
    {
        public Guid Id { get; set; } 
        public string Message { get; set; }
    }
}