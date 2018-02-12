using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GraphAPILibraries
{
    public class FindMeetingTimesJSONforRequest
    {
        public class FindMeetingTimesModel
        {
            [JsonProperty("attendees")]
            public List<Attendee> Attendees { get; set; }

            [JsonProperty("timeConstraint")]
            public TimeConstraint TimeConstraint { get; set; } = new TimeConstraint();

            [JsonProperty("locationConstraint")]
            public LocationConstraint LocationConstraint { get; set; } = new LocationConstraint();

            [JsonProperty("meetingDuration")]
            public string MeetingDuration { get; set; }

            [JsonProperty("maxCandidates")]
            public int MaxCandidates { get; set; }

            [JsonProperty("isOrganizerOptional")]
            public string IsOrganizerOptional { get; set; }

            [JsonProperty("minimumAttendeePercentage")]
            [DefaultValue(0)]
            public int MinimumAttendeePercentage { get; set; }
        }
        public class TimeConstraint
        {
            [JsonProperty("activityDomain")]
            public string ActivityDomain { get; set; }

            [JsonProperty("timeSlots")]
            public List<TimeSlot> TimeSlots { get; set; }
        }

        public class TimeSlot
        {
            [JsonProperty("start")]
            public DetailTimeSlot StartTimeSlot { get; set; } = new DetailTimeSlot();

            [JsonProperty("end")]
            public DetailTimeSlot EndTimeSlot { get; set; } = new DetailTimeSlot();
        }

        public class DetailTimeSlot
        {
            [JsonProperty("dateTime")]
            public string DateTimer { get; set; }

            [JsonProperty("timeZone")]
            public string TimeZone { get; set; }
        }

        public class LocationConstraint
        {
            [JsonProperty("isRequired")]
            public string IsRequired { get; set; }

            [JsonProperty("suggestLocation")]
            public string SuggestLocation { get; set; }

            [JsonProperty("locations")]
            public List<Location> Locations { get; set; }
        }

        public class Location
        {
            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("locationEmailAddress")]
            public string LocationEmailAddress { get; set; }
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

        public static string GetJSON(string startTime, string endTime, string duration, List<string> attendees, string email_domain)
        {
            var data = new FindMeetingTimesModel()
            {
                Attendees = new List<Attendee>(),        
                TimeConstraint = new TimeConstraint()
                {
                    ActivityDomain = "unrestricted",
                    TimeSlots = new List<TimeSlot>()
                    {
                        new TimeSlot() {
                            StartTimeSlot = new DetailTimeSlot()
                            {
                                DateTimer = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(startTime), TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")).ToString(),
                                TimeZone = "UTC"
                            },
                            EndTimeSlot = new DetailTimeSlot()
                            {
                                DateTimer = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(endTime), TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")).ToString(),
                                TimeZone = "UTC"
                            }
                        }
                    }
                },
                MeetingDuration = duration,
                IsOrganizerOptional = "false",
                MaxCandidates = 10
            };

            for (int i = 0; i < attendees.Count; i++)
            {
                data.Attendees.Add(new Attendee()
                {
                    Type = "required",
                    EmailAddress = new DetailEmailAddress()
                    {
                        Address = attendees[i] + email_domain
                    }
                });
            }
            string json_data = JsonConvert.SerializeObject(data, Formatting.Indented);
            return json_data;
        }
    }
}