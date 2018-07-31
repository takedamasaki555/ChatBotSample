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
        public static List<string> MenuList = new List<string>() { "1. �~�[�e�B���O���N�G�X�g�𑗂�", "2. ���₷��", "3. �I������" };

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
            // Skype for Business �̏ꍇ�͔F�؂��X�L�b�v
            if (channelId == "skypeforbusiness")
            {
                string menuMessage = "�����ꂳ�܂ł��B�ǂ̂悤�Ȃ��p���ł��傤���H�ԍ�����͂��Ă�������\n";
                menuMessage = menuMessage + "1. ���₷��\n2. �I������\n";
                await context.PostAsync(menuMessage);
                context.Wait(CallSfBMenuDialog);
            }
            else
            {
                await context.PostAsync("Bot �������p�̏ꍇ�́A�ȉ��̃����N���烍�O�C�����Ă��������i�ʃE�B���h�E���\������܂��j");

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
                await context.PostAsync("�F�؂Ɏ��s���܂����B");
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
            PromptDialog.Choice(context, this.CallMenuDialog, MenuList, $"{AccessToken.DisplayName}����A�����ꂳ�܂ł��B�ǂ̂悤�Ȃ��p���ł��傤���H");
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
                await context.PostAsync("�����p���肪�Ƃ��������܂����B\n\n �Ă� Bot �������p�ɂȂ�ꍇ�͉������͂���͂��Ă��������B");
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
                await context.PostAsync("�����p���肪�Ƃ��������܂����B\n\n �Ă� Bot �������p�ɂȂ�ꍇ�͉������͂���͂��Ă��������B");
                context.Done<object>(null);
            }
        }

        private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result;
            List<string> listTitle = new List<string>();

            var resultMessage = context.MakeMessage();
            resultMessage.Text = $"�󂫎��Ԃ� {CreateEventsList.Count} ��������܂����B";
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
                                Title = "�\�񂷂�",
                                DisplayText = "�\�񂷂�",
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
                                Title = "�\�񂷂�",
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

            await context.PostAsync("�\�񂵂Ă��܂�...");
            string response_json = await Events.GetResultAsync(CreateEventsList[Int32.Parse(message.Text)], AccessToken.Token);
            if (response_json != "failture")
            {
                await context.PostAsync("�~�[�e�B���O���N�G�X�g���֌W�҂ɑ��t���܂����B\n\n���ʂ� Outlook �ł��m�F���������B");
            }
            else
            {
                await context.PostAsync("�~�[�e�B���O���N�G�X�g���t�Ɏ��s���܂����B");
            }
            await ShowMenu(context);
        }

        private async Task ResumeAfterFAQDialog(IDialogContext context, IAwaitable<object> result)
        {
            await ShowMenu(context);
        }
    }
}