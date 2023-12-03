using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.DB.Contexts;
using TicTacToe.HubService;
using TicTacToe.ViewModel;

namespace TicTacToe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly tictakContexts dbtictakContexts;
        public ChatController(tictakContexts dbtictakContexts)
        {
            this.dbtictakContexts = dbtictakContexts;
        }
        /// <summary>
        /// Chet My Chats
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getMyChats/{UserId}")]
        public async Task<IActionResult> getMyChats(long UserId)
        {
            try
            {
                var _result = await GetAllChatOfThatUserID(UserId);
                return Ok(new { Status = true, ChatList = _result });
            }
            catch (Exception ex)
            {
                // Log the exception and send the email
              
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        /// <summary>
        /// Chet My Chats
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetMessages/{ChatID}")]
        public async Task<IActionResult> GetMessages(string ChatID)
        {
            try
            {
                var _result = await GetMessagesByChatID(ChatID);
                return Ok(new { Status = true, Mesagess = _result });
            }
            catch (Exception ex)
            {
                // Log the exception and send the email
               
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        private async Task<List<GetAllMychatsWithNewMessage>> GetAllChatOfThatUserID(long UserId)
        {
            List<GetAllMychatsWithNewMessage> List = new List<GetAllMychatsWithNewMessage>();
            var ChatList = dbtictakContexts.TblPrivateChats.Where(x => x.Sender_UserId == UserId || x.ReciverId_UserId == UserId).ToList();

            //get new new Or Last message
            foreach (var i in ChatList.Where(x => x.Sender_UserId != x.ReciverId_UserId))
            {
                GetAllMychatsWithNewMessage obj = new GetAllMychatsWithNewMessage();
                obj.chatId = i.PrivateChatId;
                obj.SenderUserId = UserId == i.Sender_UserId ? i.ReciverId_UserId : i.Sender_UserId;
                obj.UserName = UserId == i.Sender_UserId ? i.ReciverName : i.SenderName;
                obj.fisrtMessage = i.LastMessageBody;
                obj.date = i.LastDateTime;
                List.Add(obj);
            }
            return List.OrderByDescending(x => x.date).ToList();

        }
        private async Task<List<MyMessages>> GetMessagesByChatID(string ChatId)
        {
            List<MyMessages> obj = new List<MyMessages>();
            obj = dbtictakContexts.TblPrivateMessages.Where(x => x.PrivateChatId == Convert.ToInt64(ChatId)).Select(x => new MyMessages
            {
                chatId = x.PrivateChatId,
                messageID = x.PrivateMessageId,
                message = x.MessageText,
                senderID = x.SenderUserId,
                date = x.Datetime,
            }).OrderBy(x => x.messageID).ToList();

            return obj;
        }
    }

    public class GetAllMychatsWithNewMessage
    {
        public string photoPath { get; set; }
        public long chatId { get; set; }
        public string UserName { get; set; }
        public long SenderUserId { get; set; }
        public string fisrtMessage { get; set; }
        public string ActualUserID { get; set; }
        public bool iscompany { get; set; }
        public bool? isOnline { get; set; }
        public DateTime? date { get; set; }

    }
}
