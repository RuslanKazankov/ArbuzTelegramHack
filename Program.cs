
using ArbuzTelegramHack.Models.TelegramApi;
using System;
using TL;
using WTelegram;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using TL.Methods;

namespace ArbuzTelegramHack
{
    internal class Program
    {
        static Random rand = new Random();
        static WTelegram.Client Client;
        static User My;
        static readonly Dictionary<long, User> Users = new();
        static readonly Dictionary<long, ChatBase> Chats = new();
        static InputUserBase? arbuzBot = null;
        static InputPeer? arbuzBotPeer = null;

        static async Task Main(string[] args)
        {
            Client = new WTelegram.Client(Config);
            using (Client)
            {
                Client.OnUpdate += Client_OnUpdate;
                My = await Client.LoginUserIfNeeded();
                Users[My.id] = My;
                Console.WriteLine($"We are logged-in as {My} (id {My.id})");
                var dialogs = await Client.Messages_GetAllDialogs(); // dialogs = groups/channels/users
                dialogs.CollectUsersChats(Users, Chats);
                while (true)
                {
                    Console.Read();
                }
            }
            //Messages_Chats allChats = await client.Messages_GetAllChats();
            //foreach (var chat in chats)
            //{
            //    await Console.Out.WriteLineAsync(chat.Key + ": " + chat.Value.ID + " " + chat.Value.Title);
            //}

            //Messages_MessagesBase zhoperChat = await client.Messages_GetHistory(chats[1201998036]);
            //foreach (MessageBase? mess in zhoperChat.Messages)
            //{
            //    if (mess is Message msg)
            //    {
            //        await Console.Out.WriteLineAsync(msg.message);
            //    }
            //}

        }
        private static async Task Client_OnUpdate(UpdatesBase updates)
        {
            updates.CollectUsersChats(Users, Chats);
            if (updates is UpdateShortMessage usm && !Users.ContainsKey(usm.user_id))
                (await Client.Updates_GetDifference(usm.pts - usm.pts_count, usm.date, 0)).CollectUsersChats(Users, Chats);
            else if (updates is UpdateShortChatMessage uscm && (!Users.ContainsKey(uscm.from_id) || !Chats.ContainsKey(uscm.chat_id)))
                (await Client.Updates_GetDifference(uscm.pts - uscm.pts_count, uscm.date, 0)).CollectUsersChats(Users, Chats);
            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case UpdateNewMessage unm: await HandleMessage(unm.message); break;
                    //case UpdateEditMessage uem: await HandleMessage(uem.message, true); break;
                    //// Note: UpdateNewChannelMessage and UpdateEditChannelMessage are also handled by above cases
                    //case UpdateDeleteChannelMessages udcm: Console.WriteLine($"{udcm.messages.Length} message(s) deleted in {Chat(udcm.channel_id)}"); break;
                    //case UpdateDeleteMessages udm: Console.WriteLine($"{udm.messages.Length} message(s) deleted"); break;
                    //case UpdateUserTyping uut: Console.WriteLine($"{User(uut.user_id)} is {uut.action}"); break;
                    //case UpdateChatUserTyping ucut: Console.WriteLine($"{Peer(ucut.from_id)} is {ucut.action} in {Chat(ucut.chat_id)}"); break;
                    //case UpdateChannelUserTyping ucut2: Console.WriteLine($"{Peer(ucut2.from_id)} is {ucut2.action} in {Chat(ucut2.channel_id)}"); break;
                    //case UpdateChatParticipants { participants: ChatParticipants cp }: Console.WriteLine($"{cp.participants.Length} participants in {Chat(cp.chat_id)}"); break;
                    //case UpdateUserStatus uus: Console.WriteLine($"{User(uus.user_id)} is now {uus.status.GetType().Name[10..]}"); break;
                    //case UpdateUserName uun: Console.WriteLine($"{User(uun.user_id)} has changed profile name: {uun.first_name} {uun.last_name}"); break;
                    //case UpdateUser uu: Console.WriteLine($"{User(uu.user_id)} has changed infos/photo"); break;
                    default:
                        // Console.WriteLine(update.GetType().Name);
                        break; // there are much more update types than the above example cases
                }
        }

        private static Task HandleMessage(MessageBase messageBase, bool edit = false)
        {
            if (edit) Console.Write("(Edit): ");
            switch (messageBase)
            {
                case Message m:
                    //Console.WriteLine($"{Peer(m.from_id) ?? m.post_author} in {Peer(m.peer_id)} >\n#########\n{m.message}\n########");
                    string mess = m.message;
                    //if (arbuzBot == null)
                    //{
                    //    arbuzBot = new InputUserFromMessage() { msg_id = 165162 };
                    //    arbuzBotPeer = new InputPeerUserFromMessage() { msg_id = 165162 };
                    //    Console.WriteLine("Произошла регистрация бота. msg_id = " + 165162);
                    //}
                    if (mess.Contains("_receipt_") && mess.Contains("ref_"))
                    {
                        //StringBuilder arbuzUrl = new StringBuilder("http://t.me/wmclick_bot/click?startapp=ref");
                        StringBuilder tgUrl = new StringBuilder("tg://resolve?domain=wmclick_bot&appname=click&startapp=ref");
                        StringBuilder botParams = new StringBuilder("ref");
                        int index = mess.IndexOf("_receipt_") - 1;
                        while (mess[index] != '_')
                        {
                            index--;
                        }
                        while (index < mess.Length && mess[index] != ' ' && mess[index] != '\n')
                        {
                            tgUrl.Append(mess[index]);
                            botParams.Append(mess[index]);
                            index++;
                        }
                        Process.Start(new ProcessStartInfo(tgUrl.ToString()) { UseShellExecute = true });
                        //if (arbuzBot != null)
                        //{
                        //    Console.WriteLine("Попытка использовать Client.Messages_StartBot()");
                        //    Client.Messages_StartBot(arbuzBot, My.ToInputPeer(), rand.NextInt64(), botParams.ToString());
                        //}
                        
                        Console.WriteLine($"{Peer(m.from_id) ?? m.post_author} in {Peer(m.peer_id)} >\n#########\n{m.message}\n########");
                        //Process.Start("C:\\Users\\kazan\\AppData\\Roaming\\Telegram Desktop\\Telegram.exe", tgUrl.ToString());

                    }
                    break;
                case MessageService ms: //Console.WriteLine($"{Peer(ms.from_id)} in {Peer(ms.peer_id)} [{ms.action.GetType().Name[13..]}]");
                    break;
            }
            return Task.CompletedTask;
        }

        private static string User(long id) => Users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
        private static string? Chat(long id) => Chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
        private static string? Peer(Peer peer) => peer is null ? null : peer is PeerUser user ? User(user.user_id)
            : peer is PeerChat or PeerChannel ? Chat(peer.ID) : $"Peer {peer.ID}";

        static string Config(string what)
        {
            switch (what)
            {
                case "api_id": return TelegramApiConfig.AppId.ToString();
                case "api_hash": return TelegramApiConfig.ApiHash;
                case "phone_number": Console.Write("Phone: "); return Console.ReadLine();
                case "verification_code": Console.Write("Code: "); return Console.ReadLine();
                case "first_name": Console.Write("First name: "); return Console.ReadLine();      // if sign-up is required
                case "last_name": Console.Write("Last name: "); return Console.ReadLine();        // if sign-up is required
                case "password": Console.Write("Password: "); return Console.ReadLine();     // if user has enabled 2FA
                default: return null;                  // let WTelegramClient decide the default config
            }
        }
    }
}
