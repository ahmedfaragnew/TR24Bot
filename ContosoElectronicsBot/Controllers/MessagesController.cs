using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using XamarinCustomerService;
using System.Diagnostics;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace ContosoElectronics
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            
            

            if (activity != null)
            {
                // one of these will have an interface and process it
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        try
                        {
                            //BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                            //userData.SetProperty<bool>("SentGreeting", true);
                            //await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            //string UserID = activity.From.
                            //string UserName = activity.From.Name;
                            await Conversation.SendAsync(activity, () => new CustomerServiceLogic());
                        }
                        catch(Exception Ex)
                        {
                            if (Ex.GetType() == typeof(InvalidNeedException))
                            {
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                Activity reply = activity.CreateReply("Sorry, I'm having some difficulties here. I have to reboot myself. Lets start over.");
                                await connector.Conversations.ReplyToActivityAsync(reply);
                                StateClient stateClient = activity.GetStateClient();
                                await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                            }
                            else if(Ex.GetType() == typeof(InvalidIntentHandlerException))
                            {
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                Activity reply = activity.CreateReply("Sorry, I'm having some difficulties here. I have to reboot myself. Lets start over.");
                                Trace.TraceInformation(string.Format("Invalid intent used '{0}'.", Ex.Message));
                                await connector.Conversations.ReplyToActivityAsync(reply);
                                StateClient stateClient = activity.GetStateClient();
                                await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id); 
                            }
                        }
                        break;
                    case ActivityTypes.Ping:
                        Trace.TraceInformation(string.Format("{0} is pinging", activity.From));
                        break;
                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

       
    }
}