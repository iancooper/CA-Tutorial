using GreetingsCore.Ports.Commands;
using Newtonsoft.Json;
using Paramore.Brighter;

namespace GreetingsCore.Ports.Mappers
{
    public class RegreetCommandMessageMapper : IAmAMessageMapper<RegreetCommand>
    {
        public Message MapToMessage(RegreetCommand request)
        {
            var header = new MessageHeader(messageId: request.Id, topic: "greeting.regreet.command", messageType: MessageType.MT_COMMAND);
            var body = new MessageBody(JsonConvert.SerializeObject(request));
            var message = new Message(header, body);
            return message;
  
        }

        public RegreetCommand MapToRequest(Message message)
        {
            var greetingCommand = JsonConvert.DeserializeObject<RegreetCommand>(message.Body.Value);
            
            return greetingCommand;
        }
    }
}