using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GraphAPILibraries
{
    public class FindMeetingTimesJSONforResponse
    {
        public class FindMeetingTimesModel
        {
            [JsonProperty("meetingTimeSuggestions")]
            public List<MeetingTimeSuggestion> MeetingTimeSuggestions { get; set; }
        }

        public class MeetingTimeSuggestion
        {
            [JsonProperty("confidence")]
            public Double Confidence { get; set; }

            [JsonProperty("attendeeAvailability")]
            public List<AttendeeAvailability> AttendeeAvailabilitys { get; set; }

            [JsonProperty("meetingTimeSlot")]
            public MeetingTimeSlot MeetingTimeSlot { get; set; } = new MeetingTimeSlot();
        }

        public class AttendeeAvailability
        {
            [JsonProperty("availability")]
            public string Availability { get; set; }

            [JsonProperty("attendee")]
            public Attendee Attendees { get; set; } = new Attendee();
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
            [JsonProperty("address")]
            public string Address { get; set; }
        }

        public class MeetingTimeSlot
        {
            [JsonProperty("start")]
            public DetailTimeSlot StartTimeSlot { get; set; } = new DetailTimeSlot();

            [JsonProperty("end")]
            public DetailTimeSlot EndTimeSlot { get; set; } = new DetailTimeSlot();
        }

        public class DetailTimeSlot
        {
            [JsonProperty("dateTime")]
            public DateTime DateTimer { get; set; }

            [JsonProperty("timeZone")]
            public string TimeZone { get; set; }
        }

        public static List<string> GetFreeTimeList(string response_json)
        {
            var result = JsonConvert.DeserializeObject<FindMeetingTimesModel>(response_json);
            
            var freeroom = new List<string>();
            for (int i = 0; i < result.MeetingTimeSuggestions.Count; i++)
            {
                var start_time = TimeZoneInfo.ConvertTimeFromUtc(result.MeetingTimeSuggestions[i].MeetingTimeSlot.StartTimeSlot.DateTimer, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
                var end_time = TimeZoneInfo.ConvertTimeFromUtc(result.MeetingTimeSuggestions[i].MeetingTimeSlot.EndTimeSlot.DateTimer, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
                var room_item = start_time + " - " + end_time + "\n\n";

                if (result.MeetingTimeSuggestions[i].Confidence == 100.0)
                {
                    for (int j = 0; j < result.MeetingTimeSuggestions[i].AttendeeAvailabilitys.Count; j++)
                    {
                        room_item = room_item + result.MeetingTimeSuggestions[i].AttendeeAvailabilitys[j].Attendees.EmailAddress.Address + "\n\n";
                    }
                    freeroom.Add(room_item);

                }

            }
            return freeroom;
        }
    }
}