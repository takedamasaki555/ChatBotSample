using Newtonsoft.Json;
using System.Collections.Generic;

namespace GraphAPILibraries
{
    public class EventsJSONforRequest
    {
        // https://developer.microsoft.com/ja-jp/graph/docs/api-reference/v1.0/api/user_post_events
        public class EventsModel
        {
            [JsonProperty("subject")]
            public string Subject { get; set; }

            [JsonProperty("body")]
            public Body Body { get; set; } = new Body();

            [JsonProperty("start")]
            public DetailTimeSlot StartTime { get; set; } = new DetailTimeSlot();

            [JsonProperty("end")]
            public DetailTimeSlot EndTime { get; set; } = new DetailTimeSlot();

            [JsonProperty("attendees")]
            public List<Attendee> Attendees { get; set; }
        }

        public class Body
        {
            [JsonProperty("contentType")]
            public string ContentType { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
        }

        public class DetailTimeSlot
        {
            [JsonProperty("dateTime")]
            public string DateTime { get; set; }

            [JsonProperty("timeZone")]
            public string TimeZone { get; set; }
        }

        public class Attendee
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("emailAddress")]
            public DetailEmailAddress EmailAddress { get; set; } = new DetailEmailAddress();
        }

        public class DetailEmailAddress
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }
        }

        public static string GetJSON(string startTime, string endTime, List<string> attendees, string email_domain)
        {
            var data = new EventsModel()
            {
                Subject = "Meeting",
                Body = new Body()
                {
                    ContentType = "HTML",
                    Content = "Bot から予約した会議です。"
                },
                StartTime = new DetailTimeSlot()
                {
                    DateTime = startTime,
                    TimeZone = "UTC"
                },
                EndTime = new DetailTimeSlot()
                {
                    DateTime = endTime,
                    TimeZone = "UTC"
                },
                Attendees = new List<Attendee>(),
            };

            for (int i = 0; i < attendees.Count; i++)
            {
                data.Attendees.Add(new Attendee()
                {
                    Type = "required",
                    EmailAddress = new DetailEmailAddress()
                    {
                        Address = attendees[i]
                    }
                });
            }
            string json_data = JsonConvert.SerializeObject(data, Formatting.Indented);
            return json_data;
        }
    }
}