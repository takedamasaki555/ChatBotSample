using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;

namespace ChatBot.Dialogs
{
    [Serializable]
    public class FAQDialog : IDialog<object>
    {
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
            } else {
                await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);
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