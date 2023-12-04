using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq;
using TicTacToe.DB;
using TicTacToe.DB.Contexts;

namespace TicTacToe.HubService
{
    public class MailHub:Hub
    {
        private readonly tictakContexts dbtictakContexts;
        public MailHub(tictakContexts dbtictakContexts)
        {
            this.dbtictakContexts = dbtictakContexts;
        }
        #region SignalR

        #region Connections
        //Create ConnectionIDs 
        public async Task OpenNewPage(string currentUserId, string userName, string brwserInfo)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, currentUserId);
            await CreateConnection(currentUserId, Context.ConnectionId, userName, brwserInfo);
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        }
        //Remove ConnectionID when leave page
        public async Task LeavePage(string currentUserId)
        {
            await RemoveConnectionByConnectionID(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentUserId);
            //await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserSignalRId);
        }
        //Remove ConnectionIDs when user logout
        public async Task LeaveApplication(string currentUserId, string brwserInfo)
        {
            var ConnectionIDs = GetAllConnectionOfThatUserID(currentUserId).Result;
            if (ConnectionIDs != null)
            {
                foreach (var COn in ConnectionIDs)
                {
                    if (!string.IsNullOrEmpty(COn))
                        await Groups.RemoveFromGroupAsync(COn, currentUserId);
                }
            }
            await RemoveAllConnectionOfThatUserID(currentUserId, brwserInfo);

        }
        #endregion

        #region Game 
        public async Task CreateGameBoard(long FromUserId,string FromUserName,long ToUserId )
        {
           //create board Req
           TblGameBoard obj = new TblGameBoard {
           FromUserId = FromUserId,
           ToUserId = ToUserId ,
           Status = "pennding",
           CreatedDate = DateTime.Now,
           };
            dbtictakContexts.TblGameBoard.Add(obj);
            dbtictakContexts.SaveChanges();

            //send notification to opponent
            var ConIds = GetAllConnectionOfThatUserID(ToUserId.ToString()).Result;
            if (ConIds?.Count > 0)
            {
                foreach (var Con in ConIds)
                {
                    await Clients.Client(Con).SendAsync("ReceiveGameReq", FromUserName + "Wants to Play", obj.Id/*GameId*/,FromUserId);
                    
                }
            }

            //notifay me
            await Clients.Caller.SendAsync("GameReq_Sent_notification", "Reqiuest has been sent");
        }

        public async Task AcceptOrReject(long GameId,bool Status)
        {
           if( dbtictakContexts.TblGameBoard.Where(x => x.Id == GameId).Any())
            {
             var game =   dbtictakContexts.TblGameBoard.Where(x => x.Id == GameId).FirstOrDefault();
                game.IsAccepted = Status;
                game.Status = Status ? "Accepted" : "Rejected";
                dbtictakContexts.TblGameBoard.Update(game);
                dbtictakContexts.SaveChanges();
                // aponant
                var aponanName = dbtictakContexts.Tbl_User.Where(x=>x.Id == game.ToUserId).AsNoTracking().FirstOrDefault().UserName;

                //notification 
                var ConIds = GetAllConnectionOfThatUserID(game.FromUserId.ToString()).Result;
                if (ConIds?.Count > 0)
                {
                    foreach (var Con in ConIds)
                    {
                        await Clients.Client(Con).SendAsync("GameReqStatusNotification",game.Id ,game.Status, aponanName,game.ToUserId);

                    }


                }

            }

        }

        public async Task GameMove(string[] board,string player,string FromUserId,long ToUserId)
        {
            var ConIds = GetAllConnectionOfThatUserID(ToUserId.ToString()).Result;
            if (ConIds?.Count > 0)
            {
                foreach (var Con in ConIds)
                {
                    await Clients.Client(Con).SendAsync("opponentMove",board,player);
              
                }
            }

        }
        #endregion

        #region Message
        public async Task SendPrivateMessage(long senderID, long recipientUserId, string message)
        {


          

            // Get chat id (if already Exsit otherwise Create New One)
            var PrivatechatId = GetChatId(senderID, recipientUserId, message).Result;
            // Ensure that the sender and recipient are in the same private chat
           
                var messageResult = AddMessage(senderID, recipientUserId, message, PrivatechatId).Result;
                if (messageResult != null)
                {
                    if (GetAllConnectionOfThatUserID(recipientUserId.ToString()) != null) //mean if this person is online
                    {
                        // get All Connection Ids Of senders
                        var ConIds = GetAllConnectionOfThatUserID(recipientUserId.ToString()).Result;
                        if (ConIds?.Count > 0)
                        {
                            foreach (var Con in ConIds)
                            {
                                await Clients.Client(Con).SendAsync("ReceivePrivateMessage", messageResult.messageID, messageResult.message, messageResult.chatId, messageResult.senderID, messageResult.date);
                                await Clients.Client(Con).SendAsync("NotifayMe", "you have New Message :" + message);
                            }
                        }
                    }

                    await Clients.Caller.SendAsync("SendMeasseNotifayMe", "Message has been Sent",  messageResult.messageID, messageResult.message, messageResult.chatId, messageResult.senderID, messageResult.date);

                }
        
        }
        #endregion

        #endregion


        #region DbFunctions

        #region ConnectionLogic
        #region Create Connection
        public async Task<bool> CreateConnection(string UserID, string ConnectionID, string UserName, string brwserInfo)
        {
            if (string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(ConnectionID)) { return false; }

            else
            {
                //just check the dublication 
                if (!dbtictakContexts.TblSignalRConnection.Where(x => x.UserID == UserID && x.SignalRConnectionID == ConnectionID).Any())
                {
                    //create connection
                    TblSignalRConnection obj = new TblSignalRConnection();
                    obj.UserID = UserID;
                    obj.brwserInfo = brwserInfo;
                    obj.SignalRConnectionID = ConnectionID;
                    //obj.UserType = type;
                    obj.UserName = UserName;
                    await dbtictakContexts.TblSignalRConnection.AddAsync(obj);
                    await dbtictakContexts.SaveChangesAsync();
                    return true;

                }
                return false;
            }



        }
        #endregion
        #region Get SignalR Connections
        public async Task<string?> GetConnectionById(string UserId)
        {
            if (UserId != null)
            {
                return await dbtictakContexts.TblSignalRConnection.Where(x => x.UserID == UserId).Select(x => x.SignalRConnectionID).AsNoTracking().FirstOrDefaultAsync();
            }
            else { return "UserId is Null"; }
        }
        public async Task<List<string?>?> GetAllConnectionOfThatUserID(string UserId)
        {
            if (UserId != null)
            {
                return await dbtictakContexts.TblSignalRConnection.Where(x => x.UserID == UserId).Select(x => x.SignalRConnectionID).AsNoTracking().ToListAsync();
            }
            else { return null; }
        }
        public async Task<connectionData?> GetConnectionIdByConnectionId(string Conid)
        {
            connectionData obj = new connectionData();

            return await dbtictakContexts.TblSignalRConnection.Where(x => x.SignalRConnectionID == Conid).Select(x => new connectionData
            {
                ConnectionId = x.SignalRConnectionID,
                userId = x.UserID,
            }).AsNoTracking().FirstOrDefaultAsync();

        }
        #endregion
        #region Remove SignalR Connections
        public async Task RemoveConnectionByUserId(string UserID, string type)
        {
            if ((!string.IsNullOrEmpty(UserID)) && await dbtictakContexts.TblSignalRConnection.Where(x => x.UserID == UserID && x.UserType == type).AnyAsync())
            {
                var obj = await dbtictakContexts.TblSignalRConnection.Where(x => x.UserID == UserID).FirstOrDefaultAsync();

                if (obj != null)
                {
                    dbtictakContexts.TblSignalRConnection.Remove(obj);
                    await dbtictakContexts.SaveChangesAsync();
                }
            }

        }
        public async Task RemoveConnectionByConnectionID(string connectionID)
        {

            var obj = await dbtictakContexts.TblSignalRConnection.Where(x => x.SignalRConnectionID == connectionID).FirstOrDefaultAsync();
            if (obj != null)
            {
                dbtictakContexts.TblSignalRConnection.Remove(obj);
                dbtictakContexts.SaveChanges();

            }



        }
        public async Task RemoveAllConnectionOfThatUserID(string userId, string brwserInfo)
        {
            var ConnectionList = await dbtictakContexts.TblSignalRConnection.Where(x => x.UserID == userId && x.brwserInfo == brwserInfo).ToListAsync();
            if (ConnectionList != null)
            {
                dbtictakContexts.TblSignalRConnection.RemoveRange(ConnectionList);
                dbtictakContexts.SaveChanges();
            }



        }
        #endregion
        #endregion
        //Mesage Logics
        #region MessageLogics

        #region Chat
        public async Task<long> GetChatId(long user1Id, long user2Id, string LastMessage) //user1 is sender
        {

            var User1Name = dbtictakContexts.Tbl_User.Where(x => x.Id == user1Id).FirstOrDefault()?.UserName;
            var User2Name = dbtictakContexts.Tbl_User.Where(x => x.Id == user2Id).FirstOrDefault()?.UserName;


            if (dbtictakContexts.TblPrivateChats.Any(c => (c.Sender_UserId == user1Id && c.ReciverId_UserId == user2Id) || (c.Sender_UserId == user2Id && c.ReciverId_UserId == user1Id)))
            {
                var chat = dbtictakContexts.TblPrivateChats.SingleOrDefault(c => (c.Sender_UserId == user1Id && c.ReciverId_UserId == user2Id) || (c.Sender_UserId == user2Id && c.ReciverId_UserId == user1Id));
                
                //update the message
                chat.LastMessageBody = LastMessage;
                chat.LastDateTime = DateTime.Now;
                dbtictakContexts.TblPrivateChats.Update(chat);
                dbtictakContexts.SaveChanges();
                // If a chat is found, return the ChatId; otherwise, return null
                return chat.PrivateChatId;
            }
            else
            {
                var newChat = new TblPrivateChats
                {
                    Sender_UserId = user1Id,
                    ReciverId_UserId = user2Id,
                    SenderName = User1Name, //send from
                    ReciverName = User2Name, //send send to
                    LastDateTime = DateTime.Now 
                    ,LastMessageBody = LastMessage
                };

                dbtictakContexts.TblPrivateChats.Add(newChat);
                dbtictakContexts.SaveChanges();

                return newChat.PrivateChatId;
            }

            // For this example, we'll assume that there is no existing chat


        }
        #endregion

        #region Message Text
        public async Task<MyMessages> AddMessage(long SenderID, long reciverId, string message, long chatId)
        {
            TblPrivateMessages obj = new TblPrivateMessages()
            {
                MessageText = message,
                ReceiverUserId = reciverId,
                SenderUserId = SenderID,
                IsDeleted = false,
                IsSeen = false,
                Datetime = DateTime.Now,
                PrivateChatId = chatId,
            };
            dbtictakContexts.TblPrivateMessages.Add(obj);
            dbtictakContexts.SaveChanges();
            MyMessages messageObj = new MyMessages()
            {
                message = obj.MessageText,
                chatId = obj.PrivateChatId,
                date = DateTime.Now,
                messageID = obj.PrivateMessageId,
                senderID = obj.SenderUserId,
            };


            return messageObj;
        }
        #endregion

        #endregion
        #endregion
    }
    #region Hub Models
    public class connectionData
    {
        public string? ConnectionId { get; set; }
        public string? userId { get; set; }
    }
    public class MyMessages
    {
        public long messageID { get; set; }
        public string? message { get; set; }
        public long chatId { get; set; }
        public long? senderID { get; set; }
        public DateTime? date { get; set; }
     
    }
    #endregion

}
