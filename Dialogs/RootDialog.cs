using System;
using System.Threading.Tasks;
using BotAuth.AADv1;
using BotAuth.Dialogs;
using BotAuth.Models;
using BotAuth;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using GraphAPILibraries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public static AccessToken AccessToken = new AccessToken();
        public static List<string> CreateEventsList = new List<string>();
        public static string channelId = "";
        public static List<string> MenuList = new List<string>() { "1. ミーティングリクエストを送る", "2. 質問する", "3. 終了する" };

        // Step 1: Welcome Message
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        // Step 2: Authentication by Azure AD
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            channelId = message.ChannelId;
            // Skype for Business の場合は認証をスキップ
            if (channelId == "skypeforbusiness")
            {
                string menuMessage = "おつかれさまです。どのようなご用件でしょうか？番号を入力してください\n";
                menuMessage = menuMessage + "1. 質問する\n2. 終了する\n";
                await context.PostAsync(menuMessage);
                context.Wait(CallSfBMenuDialog);
            }
            else
            {
                await context.PostAsync("Bot をご利用の場合は、以下のリンクからログインしてください（別ウィンドウが表示されます）");

                AuthenticationOptions options = new AuthenticationOptions()
                {
                    Authority = ConfigurationManager.AppSettings["aad:Authority"],
                    ClientId = ConfigurationManager.AppSettings["aad:ClientId"],
                    ClientSecret = ConfigurationManager.AppSettings["aad:ClientSecret"],
                    ResourceId = "https://graph.microsoft.com",
                    RedirectUrl = ConfigurationManager.AppSettings["aad:Callback"],
                    UseMagicNumber = false,
                    MagicNumberView = "LoginSuccess.html"
                };

                await context.Forward(new AuthDialog(new ADALAuthProvider(), options), this.ResumeAfterAuthDialog, message, CancellationToken.None);

            }
        }

        // Step 3: Authentication Result
        private async Task ResumeAfterAuthDialog(IDialogContext context, IAwaitable<AuthResult> authResult)
        {
            var result_auth = await authResult;

            // Use token to call into service
            var json = await new HttpClient().GetWithAuthAsync(result_auth.AccessToken, "https://graph.microsoft.com/v1.0/me");
            if (json == null)
            {
                await context.PostAsync("認証に失敗しました。");
            }
            else
            {
                AccessToken.DisplayName = json.Value<string>("displayName");
                AccessToken.Spn = json.Value<string>("userPrincipalName");
                AccessToken.EmailDomain = AccessToken.Spn.Remove(0, AccessToken.Spn.IndexOf("@"));
                AccessToken.Token = result_auth.AccessToken;

                await ShowMenu(context);
            }
        }

        // Step 4: Main Menu
        private async Task ShowMenu(IDialogContext context)
        {

            //Show menues
            PromptDialog.Choice(context, this.CallMenuDialog, MenuList, $"{AccessToken.DisplayName}さん、おつかれさまです。どのようなご用件でしょうか？");
        }

        private async Task CallSfBMenuDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            //This method is resume after user choise menu
            var selectedMenu = await result;

            if (selectedMenu.Text == "1")
            {
                context.Call(new FAQDialog(), ResumeAfterFAQDialog);
            }
            else if (selectedMenu.Text == "2")
            {
                await context.PostAsync("ご利用ありがとうございました。\n\n 再び Bot をご利用になる場合は何か文章を入力してください。");
                context.Done<object>(null);
            }
        }

        // Step 5: Check clicked item
        private async Task CallMenuDialog(IDialogContext context, IAwaitable<string> result)
        {
            //This method is resume after user choise menu
            var selectedMenu = await result;

            if (selectedMenu == MenuList[0] || selectedMenu == "1") {
                context.Call(new FindMeetingTimesDialog(), ResumeAfterDialog);
            } else if(selectedMenu == MenuList[1] || selectedMenu == "2") {
                context.Call(new FAQDialog(), ResumeAfterFAQDialog);
            } else if(selectedMenu == MenuList[2] || selectedMenu == "3")
            {
                await context.PostAsync("ご利用ありがとうございました。\n\n 再び Bot をご利用になる場合は何か文章を入力してください。");
                context.Done<object>(null);
            }
        }

        private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result;
            List<string> listTitle = new List<string>();

            var resultMessage = context.MakeMessage();
            resultMessage.Text = $"空き時間が {CreateEventsList.Count} 件見つかりました。";
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();
            for (int i = 0; i < CreateEventsList.Count; i++)
            {
                var data = JsonConvert.DeserializeObject<EventsJSONforRequest.EventsModel>(CreateEventsList[i]);
                string title = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(data.StartTime.DateTime), TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")).ToString()
                                + "-" + TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(data.EndTime.DateTime), TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")).ToString()
                                + "\n\n";
                for (int j = 0; j < data.Attendees.Count; j++)
                {
                    title = title + data.Attendees[j].EmailAddress.Address + "\n\n";
                }
                listTitle.Add(title);

                if (channelId == "msteams")
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = title,
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "予約する",
                                DisplayText = "予約する",
                                Type = ActionTypes.MessageBack,
                                Text = $"{i}"
                            }
                        }
                    };
                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }
                else
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = title,
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "予約する",
                                Type = ActionTypes.PostBack,
                                Value = $"{i}"
                            }
                        }
                    };
                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }
            }

            await context.PostAsync(resultMessage);
            context.Wait(CreateEventReceivedAsync);
        }

        public virtual async Task CreateEventReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            await context.PostAsync("予約しています...");
            string response_json = await Events.GetResultAsync(CreateEventsList[Int32.Parse(message.Text)], AccessToken.Token);
            if (response_json != "failture")
            {
                await context.PostAsync("ミーティングリクエストを関係者に送付しました。\n\n結果は Outlook でご確認ください。");
            }
            else
            {
                await context.PostAsync("ミーティングリクエスト送付に失敗しました。");
            }
            await ShowMenu(context);
        }

        private async Task ResumeAfterFAQDialog(IDialogContext context, IAwaitable<object> result)
        {
            await ShowMenu(context);
        }
    }
}