using System;

namespace ContosoElectronics
{
    public class User
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }

    public class MessageSet
    {
        public BotMessage[] messages { get; set; }
        public string watermark { get; set; }
        public string eTag { get; set; }

    }
    public class BotMessage
    {
        public string id { get; set; }
        public string conversationId { get; set; }
        public DateTime created { get; set; }
        public string from { get; set; }
        public string text { get; set; }
        public string channelData { get; set; }
        public string[] images { get; set; }
        public Attachment[] attachments { get; set; }
        public string eTag { get; set; }
    }

    public class Attachment
    {
        public string url { get; set; }
        public string contentType { get; set; }
    }

    public class Conversation
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public string eTag { get; set; }
    }
}