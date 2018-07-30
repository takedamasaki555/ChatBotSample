using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;
using System.Net.Http;
using Newtonsoft.Json;
using CustomQnAMaker;

namespace ChatBot.Dialogs
{
    [Serializable]
    public class FAQDialog : IDialog<object>
    {
        public static string json = "";

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("質問を入力してください。\n\n質問を終了する場合は、\"/end\" と入力してください。");
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            if (message.Text == "/end")
            {
                context.Done<object>(null);
            }
            else
            {
                if (message.ChannelId == "skypeforbusiness")
                {
                    json = await GenerateAnswer.GetResultAsync(message.Text);
                    if (json != "failture")
                    {
                        var result = JsonConvert.DeserializeObject<QnAMakerResults>(json);
                        await ShowQuestions(context,result);
                    }
                }
                else
                {
                    await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);
                }
            }
        }

        private async Task ShowQuestions(IDialogContext context, QnAMakerResults result)
        {
            int i;
            string resultMessage = "番号を入力してください\n";

            for (i = 0; i < result.Answers.Count; i++)
            {
                resultMessage = resultMessage + i + 1 + ". " + result.Answers[i].Questions[0] + "\n";
            }
            resultMessage = resultMessage + i + 1 + ". 上記のどれでもない\n";
            await context.PostAsync(resultMessage);
            context.Wait(ShowAnswer);

        }

        private async Task ShowAnswer(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var num = await item;
            var result = JsonConvert.DeserializeObject<QnAMakerResults>(json);

            if (Int32.Parse(num.Text) >= 1 && Int32.Parse(num.Text) <= result.Answers.Count) {
                await context.PostAsync(result.Answers[Int32.Parse(num.Text)-1].Answer.ToString());
                await AfterAnswerAsync(context, item);
            } else if(Int32.Parse(num.Text) == result.Answers.Count+1)
            {
                await context.PostAsync("お役に立てず申し訳ございません。。");
                await AfterAnswerAsync(context, item);
            }
            else
            {
                await ShowQuestions(context, result);
            } 
            
        }

        private async Task AfterAnswerAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // wait for the next user message
            await context.PostAsync("質問を入力してください。\n\n質問を終了する場合は、\"/end\" と入力してください。");
            context.Wait(MessageReceivedAsync);
        }
    }

    [Serializable]
    public class BasicQnAMakerDialog : QnAMakerDialog
    {
        public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(ConfigurationManager.AppSettings["QnAAuthKey"], ConfigurationManager.AppSettings["QnAKnowledgebaseId"], "No good match in FAQ.", 0, 5, ConfigurationManager.AppSettings["QnAEndpointHostName"])))
        { }
    }
}