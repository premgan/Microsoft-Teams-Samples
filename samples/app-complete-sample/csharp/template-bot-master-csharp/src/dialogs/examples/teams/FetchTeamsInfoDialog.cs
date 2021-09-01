﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Fetch Teams Info Dialog Class main purpose of this dialog class is to display Team Name, TeamId and AAD GroupId.
    /// </summary>
    [Serializable]
    public class FetchTeamsInfoDialog : ComponentDialog
    {
        public FetchTeamsInfoDialog() : base(nameof(FetchTeamsInfoDialog))
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
            var team = stepContext.Context.Activity.GetChannelData<TeamsChannelData>().Team;

            if (team != null)
            {
                var connectorClient = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl), ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);

                // Handle for channel conversation, AAD GroupId only exists within channel
                //TeamDetails teamDetails = await connectorClient.GetTeamsConnectorClient().Teams.FetchTeamDetailsAsync(team.Id);

                var message = stepContext.Context.Activity;
                //message.Text = GenerateTable(teamDetails);

                await stepContext.Context.SendActivityAsync(message);
            }
            else
            {
                // Handle for 1 to 1 bot conversation
                await stepContext.Context.SendActivityAsync(Strings.TeamInfo1To1ConversationError);
            }

            //Set the Last Dialog in Conversation Data
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogFetchTeamInfoDialog);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        /// <summary>
        /// Generate HTML dynamically to show TeamId, TeamName and AAD GroupId in table format 
        /// </summary>
        /// <param name="teamDetails"></param>
        /// <returns></returns>
        private string GenerateTable(TeamDetails teamDetails)
        {
            if (teamDetails == null)
            {
                return string.Empty;
            }

            string tableHtml = $@"<table border='1'>
                                    <tr><td> Team id </td><td>{HttpUtility.HtmlEncode(teamDetails.Id)}</td><tr>
                                    <tr><td> Team name </td><td>{HttpUtility.HtmlEncode(teamDetails.Name)}</td></tr>
                                    <tr><td> AAD group id </td><td>{HttpUtility.HtmlEncode(teamDetails.AadGroupId)}</td><tr>
                                  </table>";
            return tableHtml;
        }
    }
}