using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using ContosoElectronics;
using System.Threading;

namespace XamarinCustomerService
{

    [LuisModel("LUIS_APP_ID", "LUIS_APP_SECRET")]
    [Serializable]
    public class CustomerServiceLogic : LuisDialog<object>
    {
        static string LUIS_APP_ID = "LUIS_APP_ID";
        static string LUIS_APP_PWS = "LUIS_APP_SECRET";
        string UserID = "";
        string UserName = "";
        string ChannelID = "";
        static LuisModelAttribute LuisModel = new LuisModelAttribute(LUIS_APP_ID, LUIS_APP_PWS);
        LuisService LuisService = new LuisService(LuisModel);
        string userIssue;
        List<string> customerSupportKeywords = new List<string>(new string[] { "password", "cancel", "login", "log", "create", "return", "policy" });


        [LuisIntent("Gratitude")]
        public async Task Gratitude(IDialogContext context, LuisResult result)
        {
            
            string message = "Your welcome!";
            await context.PostAsync(message);
            context.Done(true);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            if(result.Query.ToLower() == "ok")
            {
                PromptDialog.Text(context, HelpResponse, "Is there anything else I can help you with today?");
                return;
            }
            string message = "Sorry, I do not understand. Try asking me for order information or customer service help";
            await context.PostAsync(message);
            context.Done(true);
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var item = await activity;
            PromptDialog.Text(context, GreetingResponse, "Hi there! I'm very energized today, How are you doing?");

            UserID = item.From.Id;
            UserName = item.From.Name;
            ChannelID = item.ChannelId;


        }
        [LuisIntent("Leave")]
        public async Task Leave(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            string message = "OK. Good bye";
            await context.PostAsync(message);
            context.Done(true);

        }
        [LuisIntent("happiness")]
        public virtual async Task Happy(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(context, HelpResponse, "Alright. Is there anything else I can help you with today?");

        }

        [LuisIntent("Relationship")]
        public virtual async Task Relationship(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(context, AdminMode, "Uhmmm... Why do you care?");

        }

        private async Task AdminMode(IDialogContext context, IAwaitable<string> result)
        {
            var item = await result;
            string reply = item.ToString().ToLower();

            if(reply == "i'm your boss!")
            {
                Storage storage = new Storage();
                List<string> Users = storage.GetUsers();
                string UsersList = "";
                
                
                await context.PostAsync(string.Format("Welcome root!. So far {0} different users talked to me.",Users.Count));
                await context.PostAsync(string.Format("Here are the names:"));
                foreach (string user in Users)
                {
                    await context.PostAsync(user);
                }
            }
            else
                await context.PostAsync("I'm sorry, You are not my boss. Try again later!");
            context.Done(result);
        }
        [LuisIntent("Sadness")]
        public virtual async Task Sad(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Oh. Sorry to hear that. Is there anything else I can help you with today?");
            context.Done(result);
        }

        private async Task GreetingResponse(IDialogContext context, IAwaitable<string> result)
        {
            var item = await result;

            //Get intent from the LUIS service
            LuisResult Lresult = await LuisService.QueryAsync(item,CancellationToken.None);

            if(Lresult.Intents.Count > 0 )
            {
                if(Lresult.Intents[0].Intent == "happiness")
                    PromptDialog.Text(context, GetUserName, "Glad you are doing ok. What is your name?");
                else if(Lresult.Intents[0].Intent == "Sadness")
                    PromptDialog.Text(context, GetUserName, "Sorry to hear that. What is your name?");
                else
                    PromptDialog.Text(context, GetUserName, "Ok.What is your name?");
            }

        }

        private async Task GetUserName(IDialogContext context, IAwaitable<string> result)
        {
            bool UserFound = false;
            string FirstName = "";
            string LastName = "";
           
            var item = await result;
            string Name = item;
            try
            {
                LuisResult Lresult = await LuisService.QueryAsync(item, CancellationToken.None);

                if(Lresult.TopScoringIntent.Intent == "ProvideName")
                {
                    EntityRecommendation FirstNameE = new EntityRecommendation();
                    EntityRecommendation LastNameE = new EntityRecommendation();
                    if (Lresult.TryFindEntity("PersonsName::FirstName",out FirstNameE))
                    {
                        FirstName = FirstNameE.Entity;
                    }
                    if (Lresult.TryFindEntity("PersonsName::LastName", out LastNameE))
                    {
                        LastName = LastNameE.Entity;
                    }
                    if(!string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName))
                    {
                        Name = FirstName + " " + LastName;
                        Name = Name.Trim();
                    }
                }
                else
                {
                    Name = item.ToLower();
                    Name = Name.Replace("my name is", "");
                    Name = Name.Replace("i'm", "");
                    Name = Name.Replace("i am", "");
                }
                
                Storage storage = new Storage();

                Users user = storage.GetUser(UserID, Name.Trim());
                if (user != null)
                    UserFound = true;
                else
                    storage.AddUser(UserID, Name.Trim(), "");
            }
            catch(Exception Ex)
            {
                await context.PostAsync("Oops! I'm having trouble storing your name. I think I'm getting old!");
            }
            if(UserFound)
            {
                await context.PostAsync("Welcome back " + Name + ". How can I help you today?");
            }
            else
                await context.PostAsync("Great "+ Name + ". How can I help you today? Want to check order status, cancel an order or login?");
            context.Done(true);
        }
        private async Task HelpResponse(IDialogContext context, IAwaitable<string> result)
        {
            var item = await result;

            //Get intent from the LUIS service
            LuisResult Lresult = await LuisService.QueryAsync(item, CancellationToken.None);

            if (Lresult.Intents.Count > 0)
            {
                if (Lresult.Intents[0].Intent == "Acceptance")
                    await context.PostAsync("What can I do for you?");
                else if (Lresult.Intents[0].Intent == "Rejection")
                    await context.PostAsync("OK. I will be here in case you changed your mind");
                else
                    await context.PostAsync("I'm sorry. Can you repeat that again?");
            }

            context.Done(true);
        }
        #region order
        [LuisIntent("OrderStatus")]
        public async Task OrderStatus(IDialogContext context, LuisResult result)
        {
            var entities = result.Entities;
            bool hasOrderNumber = false;
            int orderNumber = 0;
            int tempOrder = 0;
            foreach (var entity in entities)
            {
                if (entity.Type == "OrderNumber" || entity.Type == "OrderKeyword")
                {
                    if (int.TryParse(entity.Entity, out tempOrder))
                    {
                        hasOrderNumber = true;
                        orderNumber = tempOrder;
                    }
                }
            }

            if (hasOrderNumber == true)
            {

                if (IsOdd(Convert.ToInt32(orderNumber)))
                {
                    await context.PostAsync("It looks like order " + orderNumber.ToString() + " will be delivered " + DateTime.Now.AddDays(2));
                }
                else
                {
                    await context.PostAsync("Hooray! It looks like " + orderNumber.ToString() + " is scheduled for delivery today!");
                }
            }
            else
            {
                PromptDialog.Text(context, OrderNumberAdded, "Please enter a valid order number to be able to help you.");
            }
        }


        private async Task OrderNumberAdded(IDialogContext context, IAwaitable<string> result)
        {
            var orderNumber = await result;
            var deliveryDate = checkOrderStatus(Convert.ToInt32(orderNumber));

            if (deliveryDate.Date == DateTime.Now.Date)
            {
                await context.PostAsync("It looks like your order will be delivered today!");
                context.Done(result);
            }
            else
            {
                await context.PostAsync("Your order is scheduled for delivery on " + deliveryDate.ToLongDateString());
                context.Done(result);
            }

        }

        private DateTime checkOrderStatus(int orderNumber)
        {
            if (IsOdd(Convert.ToInt32(orderNumber)))
            {
                return DateTime.Now.AddDays(2);
            }
            else
            {
                return DateTime.Now;
            }
        }

        #endregion

        [LuisIntent("CustomerService")]
        public async Task customerServiceRequest(IDialogContext context, LuisResult result)
        {
            foreach (var entity in result.Entities)
            {
                if (entity.Type == "ServiceKeyword" )
                {
                    switch (entity.Entity.ToLower())
                    {
                        case "password":
                            PromptDialog.Text(context, SupportUsernameEntered, "What is the email address associated with your account?");
                            break;
                        case "cancel":
                            PromptDialog.Text(context, SupportCancelOrderEntered, "I can help with that. What was the order number please?");
                            break;
                        case "login":
                            PromptDialog.Text(context, SupportLoginError, "Please provide the username that is associated with your account");
                            break;
                        case "log":
                            PromptDialog.Text(context, SupportLoginError, "Please provide the username that is associated with your account");
                            break;
                        case "sign":
                            await context.PostAsync("You can sign up for an account at https://xamarin.com");
                            context.Done(true);
                            break;
                        case "create":
                            await context.PostAsync("You can create an account at https://xamarin.com");
                            context.Done(true);
                            break;
                        case "return":
                        case "policy":
                            await context.PostAsync("You can return any item as long as its never used!");
                            context.Done(true);
                            break;
                        default:
                            await context.PostAsync("I'm sorry I don't understand. Can you clarify please?");
                            context.Done(true);
                            break;
                    }
                }
            }
            if (result.Entities.Count <= 0)
            {
                await context.PostAsync("I'm sorry I don't understand. Can you clarify please?");
                context.Done(true);
            }
        }

        private async Task SupportLoginError(IDialogContext context, IAwaitable<string> result)
        {
            var item = await result;

            if (IsOdd(item.Length))
            {
                PromptDialog.Choice(context, ContactMethodSelected, new string[] { "Phone", "SMS" }, "We need to verify your account, please select a contact method");
            }
            else
            {
                await context.PostAsync("Account was not found, please sign up for a new account");
                context.Done(true);
            }
        }

        private async Task ContactMethodSelected(IDialogContext context, IAwaitable<string> result)
        {
            var method = await result;
            await context.PostAsync("We will contact you via " + method + " to verify your login account");
            context.Done(true);
        }

        private async Task SupportCancelOrderEntered(IDialogContext context, IAwaitable<string> result)
        {
            var orderNumber = await result;
            var deliveryDate = checkOrderStatus(Convert.ToInt32(orderNumber));

            if (deliveryDate.Date == DateTime.Now.Date)
            {
                await context.PostAsync("I'm sorry! You order # " +orderNumber + " is already shipped and on its way to you. Please call us back when it arrives to cancel it");
                context.Done(result);
            }
            else
            {
                PromptDialog.Choice(context, ConfirmCancelation, new string[] { "Yes", "No" }, "Order # " + orderNumber + " is found and can be canceled in the next 2 hours and 23 minutes, Do you want to cancel it now?");
            }
        }

        private async Task SupportUsernameEntered(IDialogContext context, IAwaitable<string> result)
        {
            var item = await result;
            {
                if (!IsOdd(item.Length))
                {
                    await context.PostAsync("Account was not found, please sign up for a new account");
                    context.Done(true);
                }
                else
                {
                    await context.PostAsync("Thanks! We have sent a reset link to your email address!");
                    context.Done(true);
                }
            }
        }

        private async Task AfterDescriptionIssues(IDialogContext context, IAwaitable<string> result)
        {
            userIssue = await result;
            PromptDialog.Choice(context, AfterUserHasChosenAsync, new string[] { "Call", "Text", "SMS" }, "How would you like to be contacted?");
        }

        private async Task ConfirmCancelation(IDialogContext context, IAwaitable<string> result)
        {
            string userChoice = await result;

            switch (userChoice)
            {
                case "Yes":
                    await context.PostAsync("You order has been canceled successfully! You will receive full refund in 2 business days max.");
                    context.Done(result);
                    break;
                case "No":
                    await context.PostAsync("Great. Your order is still active and scheduled to be shipped soon.");
                    context.Done(true);
                    break;
            }
        }

        private async Task AfterUserHasChosenAsync(IDialogContext context, IAwaitable<string> result)
        {
            string userChoice = await result;

            switch (userChoice)
            {
                case "Call":
                    PromptDialog.Number(context, UserProvidesNumber, "What is your phone number?", "Please enter a valid phone number", 3);
                    break;
                case "Text":
                    await context.PostAsync("Let's start a chat session!");
                    context.Done(true);
                    break;
                case "SMS":
                    await context.PostAsync("Please text us at 503-555-5555");
                    context.Done(true);
                    break;
            }

        }

        private async Task UserProvidesNumber(IDialogContext context, IAwaitable<long> result)
        {
            long phoneNumber = await result;
            await context.PostAsync("We will call you at " + phoneNumber.ToString());

            context.Done(true);
        }


        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

    }

}