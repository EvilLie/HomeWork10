using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace HomeWork10
{
    class TelegramMessageClient
    {
        private MainWindow w;
        static string path = @"C:\Users\Артемий\Desktop\token.txt";
        private ITelegramBotClient bot = new TelegramBotClient(File.ReadAllText(path));
        public ObservableCollection<MessageLog> BotMessageLog { get; set; }
        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                string text = $"{DateTime.Now.ToLongTimeString()}: {message.Chat.FirstName} {message.Chat.Id} {message.Text}";
                Debug.WriteLine($"{text} TypeMessage: {message.Type}");
                if (message.Text == null)
                {
                    return Task.CompletedTask;
                }
                var messageText = message.Text;
                w.Dispatcher.Invoke(() =>
                {
                    BotMessageLog.Add(
                        new MessageLog
                        (
                            DateTime.Now.ToLongTimeString(), messageText, message.Chat.FirstName, message.Chat.Id
                        ));
                });
            }

            return Task.CompletedTask;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
        public void SendMessage(string Text, string Id)
        {
            long id = Convert.ToInt64(Id);
            bot.SendTextMessageAsync(id, Text);
        }
        public TelegramMessageClient(MainWindow W)
         {
            this.w = W;
            this.BotMessageLog = new ObservableCollection<MessageLog>();
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, 
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}
