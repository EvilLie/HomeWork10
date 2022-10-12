using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace HomeWork10
{
    class TelegramMessageClient
    {
        private MainWindow w;
        static string fileName = @"Your fileName";
        static string path = @"Your path";
        private ITelegramBotClient bot = new TelegramBotClient(File.ReadAllText(path));
        public ObservableCollection<MessageLog> BotMessageLog { get; set; }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                string s = message.Text; 
                WriteToFile(fileName, s);
                string text = $"{DateTime.Now.ToLongTimeString()}: {message.Chat.FirstName} {message.Chat.Id} {message.Text}";
                Debug.WriteLine($"{text} TypeMessage: {message.Type}");
                if (message.Text == null)
                {
                    return;
                }
                if(message.Text == "Show history")
                {
                    string[] history = ReadArrayFromFile(fileName);
                    foreach (string historyItem in history)
                    {
                        await bot.SendTextMessageAsync(message.Chat.Id, historyItem);
                    }
                    return;
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
            return;
        }
        public static void WriteToFile(string fileName,string s)
        {
            File.AppendAllText(fileName, s + "\n");
        }
        public static string[] ReadArrayFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                return File.ReadAllLines(fileName);
            }
            else
            {
                return new string[0];
            }
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
