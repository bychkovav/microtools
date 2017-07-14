namespace Platform.Utils.Events.Domain.Objects
{
    using System;
    using Enums;

    public class Envelope
    {
        public Envelope() { }

        public Envelope(Envelope en)
        {
            ContentEncode = en.ContentEncode;
            ContentLength = en.ContentLength;
            From = en.From;
            TimeStamp = en.TimeStamp;
            ToType = en.ToType;
            Payload = en.Payload;
        }

        public string ContentEncode { get; set; }

        public int ContentLength { get; set; }

        public string From { get; set; }

        public DateTime TimeStamp { get; set; }

        public ToType ToType { get; set; }

        public Payload Payload { get; set; }
    }
}
