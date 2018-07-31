using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using GraphAPILibraries;
using Newtonsoft.Json;

namespace ChatBot.Dialogs
{
    [Serializable]
    public class FindMeetingTimesDialog : IDialog<object>
    {
        [Pattern(@"^(0[1-9]|1[0-2])/(0[1-9]|[12][0-9]|3[01])$")]
        [Prompt("Step 1/5\n\n予約日はいつにしますか？\n\n例）02/01")]
        public string MeetingDate { get; set; }

        [Pattern(@"^(0[9]|1[0-9]):(00|30)$")]
        [Prompt("Step 2/5\n\n予約開始時間はいつにしますか？\n\n09:00～19:30の間で30分単位で入力してください。\n\n例）09:00")]
        public string StartTime { get; set; }

        [Pattern(@"^09:30|(1[0-9]|20):(00|30)$")]
        [Prompt("Step 3/5\n\n予約終了時間はいつにしますか？\n\n09:30～20:30の間で30分単位で入力してください。\n\n例）10:00")]
        public string EndTime { get; set; }

        [Prompt("Step 3\n\n参加者のメールアドレス(@の手前まで)を入力してください。\n\naddress@***.com の場合は address と入力します。\n\n複数の参加者がいる場合は、\",\"区切りで入力します。\n\n例) address1,address2,address3")]
        public string Attendees { get; set; }

        public string MeetingDateTime { get; set; }
        public string Duration;

        public async Task StartAsync(IDialogContext context)
        {
            var formDialog = FormDialog.FromForm(FindMeetingForm, FormOptions.PromptInStart);
            context.Call(formDialog, ResumeAfterFormDialog);
        }

        public static IForm<FindMeetingTimesDialog> FindMeetingForm()
        {
            OnCompletionAsyncDelegate<FindMeetingTimesDialog> processMeetingTimesSearch = async (context, state) =>
            {
                await context.PostAsync("検索しています...");
            };

            return new FormBuilder<FindMeetingTimesDialog>()
                .Field(new FieldReflector<FindMeetingTimesDialog>(nameof(MeetingDateTime))
                    .SetType(null)
                    .SetFieldDescription("Step 1\n\nミーティング日")
                    .SetDefine(async (state, field) =>
                    {
                        var now = DateTime.Now;
                        field
                            .AddDescription(now.ToString("yyyy/MM/dd"), now.ToString("M月d日（ddd）9:00-20:00"))
                            .AddTerms(now.ToString("yyyy/MM/dd"), now.ToString("yyyy/MM/dd"), now.ToString("M月d日（ddd）9:00-20:00"))
                            .AddDescription(now.AddDays(1).ToString("yyyy/MM/dd"), now.AddDays(1).ToString("M月d日（ddd）9:00-20:00"))
                            .AddTerms(now.AddDays(1).ToString("yyyy/MM/dd"), now.AddDays(1).ToString("yyyy/MM/dd"), now.AddDays(1).ToString("M月d日（ddd）9:00-20:00"))
                            .AddDescription(now.AddDays(2).ToString("yyyy/MM/dd"), now.AddDays(2).ToString("M月d日（ddd）9:00-20:00"))
                            .AddTerms(now.AddDays(2).ToString("yyyy/MM/dd"), now.AddDays(2).ToString("yyyy/MM/dd"), now.AddDays(2).ToString("M月d日（ddd）9:00-20:00"))
                            .AddDescription(now.AddDays(3).ToString("yyyy/MM/dd"), now.AddDays(3).ToString("M月d日（ddd）9:00-20:00"))
                            .AddTerms(now.AddDays(3).ToString("yyyy/MM/dd"), now.AddDays(3).ToString("yyyy/MM/dd"), now.AddDays(3).ToString("M月d日（ddd）9:00-20:00"))
                            .AddDescription(now.AddDays(4).ToString("yyyy/MM/dd"), now.AddDays(4).ToString("M月d日（ddd）9:00-20:00"))
                            .AddTerms(now.AddDays(4).ToString("yyyy/MM/dd"), now.AddDays(4).ToString("yyyy/MM/dd"), now.AddDays(4).ToString("M月d日（ddd）9:00-20:00"))
                            .AddDescription(now.AddDays(5).ToString("yyyy/MM/dd"), now.AddDays(5).ToString("M月d日（ddd）9:00-20:00"))
                            .AddTerms(now.AddDays(5).ToString("yyyy/MM/dd"), now.AddDays(5).ToString("yyyy/MM/dd"), now.AddDays(5).ToString("M月d日（ddd）9:00-20:00"))
                            .AddDescription(now.AddDays(6).ToString("yyyy/MM/dd"), now.AddDays(6).ToString("M月d日（ddd）9:00-20:00"))
                            .AddTerms(now.AddDays(6).ToString("yyyy/MM/dd"), now.AddDays(6).ToString("yyyy/MM/dd"), now.AddDays(6).ToString("M月d日（ddd）9:00-20:00"));
                        return true;
                    }))
                //.Field(nameof(MeetingDate))      
                //.Field(nameof(StartTime))
                //.Field(nameof(EndTime))
                .Field(new FieldReflector<FindMeetingTimesDialog>(nameof(Duration))
                    .SetType(null)
                    .SetFieldDescription("Step 2\n\n会議時間")
                    .SetDefine(async (state, field) =>
                    {
                        field
                            .AddDescription("PT30M", "30分")
                            .AddTerms("PT30M", "PT30M", "30分")
                            .AddDescription("PT1H", "60分")
                            .AddTerms("PT1H", "PT1H", "60分");
                        return true;
                    }))
                .Field(nameof(Attendees))     
                .OnCompletion(processMeetingTimesSearch)
                .Build();
        }

        private async Task ResumeAfterFormDialog(IDialogContext context, IAwaitable<FindMeetingTimesDialog> result)
        {
            try
            {
                var searchQuery = await result;
                //string startTime = DateTime.Now.Year.ToString() + "/" + searchQuery.MeetingDate + " " + searchQuery.StartTime + ":00";
                //string endTime = DateTime.Now.Year.ToString() + "/" + searchQuery.MeetingDate + " " + searchQuery.EndTime + ":00"; 
                string startTime = searchQuery.MeetingDateTime + " " + "09:00:00";
                string endTime = searchQuery.MeetingDateTime + " " + "20:00:00"; 

                string[] splited_attendees = searchQuery.Attendees.Split(',');
                List<string> attendees = new List<string>();
                foreach (string attendee in splited_attendees)
                {
                    attendees.Add(attendee);
                }

                string input_json = FindMeetingTimesJSONforRequest.GetJSON(startTime, endTime, searchQuery.Duration, attendees, RootDialog.AccessToken.EmailDomain);
                string response_json = await FindMeetingTimes.GetResultAsync(input_json, RootDialog.AccessToken.Token);
 
                RootDialog.CreateEventsList.Clear();
                RootDialog.CreateEventsList = GetFreeTimesList(response_json, 100.0);
                RootDialog.CreateEventsList.Sort();
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }
                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        public List<string> GetFreeTimesList(string response_json, double percent)
        {
            var result = JsonConvert.DeserializeObject<FindMeetingTimesJSONforResponse.FindMeetingTimesModel>(response_json);
            var eventsList = new List<string>();

            for (int i = 0; i < result.MeetingTimeSuggestions.Count; i++)
            {
                if (result.MeetingTimeSuggestions[i].Confidence == 100.0)
                {
                    var start_time = result.MeetingTimeSuggestions[i].MeetingTimeSlot.StartTimeSlot.DateTimer;
                    var end_time = result.MeetingTimeSuggestions[i].MeetingTimeSlot.EndTimeSlot.DateTimer;
                    var attendees = new List<string>();
                    for (int j = 0; j < result.MeetingTimeSuggestions[i].AttendeeAvailabilitys.Count; j++)
                    {
                        attendees.Add(result.MeetingTimeSuggestions[i].AttendeeAvailabilitys[j].Attendees.EmailAddress.Address);
                    }
                    eventsList.Add(EventsJSONforRequest.GetJSON(start_time.ToString(), end_time.ToString(), attendees, RootDialog.AccessToken.EmailDomain));
                }
            }
            return eventsList;
        }
    }
}