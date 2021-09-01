﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is DeepLink Dialog Class. Main purpose of this class is to show Deep link from Bot to Tab example
    /// </summary>
    public class DeepLinkStaticTabDialog : ComponentDialog
    {
        private const string TabEntityID = "statictab";
        private const string TabConfigEntityID = "configTab";
        private string BotId { get; set; }
        private bool IsChannelUser { get; set; } = false;
        private string ChannelId { get; set; }
        private string TabUrl { get; set; }
        private string ButtonCaption { get; set; }
        private string DeepLinkCardTitle { get; set; }

        public DeepLinkStaticTabDialog() : base(nameof(DeepLinkStaticTabDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginFormflowAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginFormflowAsync(
WaterfallStepContext stepContext,
CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }
            BotId = ConfigurationManager.AppSettings["MicrosoftAppId"];

            GetChannelID(stepContext);

            var message = CreateDeepLinkMessage(stepContext);

            //Set the Last Dialog in Conversation Data
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogDeepLinkStaticTabDialog);
            await stepContext.Context.SendActivityAsync(message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        #region Create Deep Link Tab Card
        private IMessageActivity CreateDeepLinkMessage(WaterfallStepContext context)
        {
            var message = context.Context.Activity;
            if (IsChannelUser)
            {
                TabUrl = GetConfigTabDeepLinkURL(ChannelId);
                ButtonCaption = Strings.DeepLinkCardConfigButtonCaption;
                DeepLinkCardTitle = Strings.DeepLinkCardConfigTitle;
            }
            else
            {
                TabUrl = GetStaticTabDeepLinkURL();
                ButtonCaption = Strings.DeepLinkCard1To1ButtonCaption;
                DeepLinkCardTitle = Strings.DeepLinkCard1To1Title;
            }
            message.Attachments = new List<Attachment> {
            new HeroCard
            {
                Title = DeepLinkCardTitle,
                Buttons = new List<CardAction>
                {
                   new CardAction(ActionTypes.OpenUrl, ButtonCaption, value: TabUrl),
                }
            }.ToAttachment()
        };
            return message;
        }
        private string GetStaticTabDeepLinkURL()
        {
            //Example -  BaseURL + 28:BotId + TabEntityId (set in the manifest) + ?conversationType=chat
            return "https://teams.microsoft.com/l/entity/28:" + BotId + "/" + TabEntityID + "?conversationType=chat";
        }

        private string GetConfigTabDeepLinkURL(string channelId)
        {
            //Example -  BaseURL + BotId + TabConfigEntityId (e.g. entityId: "configTab" : it should be same which we have set at the time of Tab Creation like below) + ?context= + {"channelId":"19:47051e5643ed49b58665e1250b6db460@thread.skype"} (should be encoded)
            //microsoftTeams.settings.setSettings({ suggestedDisplayName: "Bot Info", contentUrl: createTabUrl(), entityId: "configTab" });

            channelId = channelId.Replace("19:", "19%3a")
                                     .Replace("@thread.skype", "%40thread.skype");

            return "https://teams.microsoft.com/l/entity/" + BotId + "/" + TabConfigEntityID + "?context=%7B%22channelId%22%3A%22" + channelId + "%22%7D";
        }

        private void GetChannelID(WaterfallStepContext context)
        {

            IsChannelUser = false;

            if (context.Context.Activity.ChannelData != null)
            {
                ChannelId = context.Context.Activity.ChannelId;

                if (!string.IsNullOrEmpty(ChannelId))
                {
                    IsChannelUser = true;
                }
                else
                {
                    IsChannelUser = false;
                }
            }
        }
        #endregion
    }
}