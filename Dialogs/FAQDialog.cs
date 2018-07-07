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
    public class FAQDialog : IDialog<object>
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
            context.Done<object>(null);
        }
    }
}