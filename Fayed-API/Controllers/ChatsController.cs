using BLL.DTOs.Chat;
using BLL.DTOs.Common;
using BLL.Managers;
using DAL.Models.Common;
using DAL.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private readonly IChatManager _chatManager;

        public ChatsController(IChatManager chatManager)
        {
            _chatManager = chatManager;
        }

        /// <summary>
        /// استرداد كافة المحادثات الخاصة بالمستخدم الحالي.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بإرجاع قائمة غير مقسمة (غير مصفحة) بجميع المحادثات التي يملكها المستخدم المُوثَّق.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;List&lt;ChatDto&gt;&gt;</c>؛ حيث تشير الخاصية <c>StatusCode</c> إلى حالة العملية
        /// وتحتوي <c>Data</c> على قائمة كائنات <c>ChatDto</c>.
        /// يتطلب هذا الإجراء وجود مستخدم مُوثَّق (JWT token) ويتم استخراج معرف المستخدم من الـ Claim الخاص بـ NameIdentifier.
        /// لا تدعم هذه النقطة التقسيم الصفحي (pagination).
        /// </remarks>
        /// <returns>قائمة بكائنات <c>ChatDto</c> مغلفة داخل <c>BaseResponse</c>.</returns>
        [HttpGet]
        public async Task<ActionResult<BaseResponse<List<ChatDto>>>> GetMyChats()
        {
            var userId = GetCurrentUserId();
            var response = await _chatManager.GetMyChatsAsync(userId);
            return Ok(response);
        }

        /// <summary>
        /// إنشاء محادثة جديدة مع مستخدم آخر.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بإنشاء محادثة جديدة بناءً على البيانات المرسلة في نص الطلب.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;ChatDetailsDto&gt;</c>؛ حيث تحتوي <c>Data</c> على تفاصيل المحادثة المُنشأة.
        /// يتطلب هذا الإجراء وجود مستخدم مُوثَّق ويتم استخراج معرف المستخدم الحالي من التوكن.
        /// قد يعيد الحالة 201 (Created) عند النجاح أو 400 (Bad Request) في حال وجود بيانات غير صالحة.
        /// </remarks>
        /// <param name="dto">كائن <c>CreateChatDto</c> يحتوي على بيانات المحادثة الجديدة (مثل معرف المستخدم المُستقبِل والرسالة الأولى).</param>
        /// <returns>تفاصيل المحادثة المُنشأة مغلفة داخل <c>BaseResponse</c>.</returns>
        [HttpPost]
        public async Task<ActionResult<BaseResponse<ChatDetailsDto>>> CreateChat([FromBody] CreateChatDto dto)
        {
            var userId = GetCurrentUserId();
            var response = await _chatManager.CreateChatAsync(userId, dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// استرداد تفاصيل محادثة محددة بواسطة معرفها.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بإرجاع تفاصيل محادثة معينة تتضمن الرسائل والأعضاء.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;ChatDetailsDto&gt;</c>.
        /// يتحقق الإجراء من أن المستخدم المُوثَّق هو عضو في المحادثة قبل إرجاع البيانات.
        /// في حال عدم وجود المحادثة أو عدم صلاحية الوصول، يتم إرجاع الحالة 404 (Not Found).
        /// </remarks>
        /// <param name="id">معرف المحادثة (Integer).</param>
        /// <returns>تفاصيل المحادثة المطلوبة مغلفة داخل <c>BaseResponse</c>.</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BaseResponse<ChatDetailsDto>>> GetChatDetails(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _chatManager.GetChatDetailsAsync(id, userId);
            return Ok(response);
        }

        /// <summary>
        /// إرسال رسالة جديدة داخل محادثة محددة.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بإضافة رسالة جديدة إلى المحادثة ذات المعرف المُعطى.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;MessageDto&gt;</c>؛ حيث تحتوي <c>Data</c> على كائن الرسالة المُنشأة.
        /// يتطلب الإجراء أن يكون المستخدم المُوثَّق عضواً في المحادثة.
        /// قد يعيد الحالة 201 (Created) عند نجاح الإرسال أو 400/404 في حال وجود خطأ في البيانات أو عدم وجود المحادثة.
        /// </remarks>
        /// <param name="id">معرف المحادثة المرسَل إليها الرسالة.</param>
        /// <param name="dto">كائن <c>SendMessageDto</c> يحتوي على محتوى الرسالة.</param>
        /// <returns>كائن الرسالة المُنشأة مغلف داخل <c>BaseResponse</c>.</returns>
        [HttpPost("{id:int}/messages")]
        public async Task<ActionResult<BaseResponse<MessageDto>>> SendMessage(int id, [FromBody] SendMessageDto dto)
        {
            var userId = GetCurrentUserId();
            var response = await _chatManager.SendMessageAsync(id, userId, dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// تعليم جميع الرسائل غير المقروءة في محادثة معينة كمقروءة.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بتحديث حالة الرسائل غير المقروءة إلى مقروءة للمستخدم الحالي داخل المحادثة المُحدَّدة.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;bool&gt;</c>؛ حيث تشير <c>Data</c> إلى نجاح العملية (true) أو فشلها (false).
        /// يتطلب الإجراء أن يكون المستخدم المُوثَّق عضواً في المحادثة.
        /// </remarks>
        /// <param name="id">معرف المحادثة المراد تحديث حالة القراءة فيها.</param>
        /// <returns>قيمة منطقية (bool) تشير إلى نجاح عملية التحديث مغلفة داخل <c>BaseResponse</c>.</returns>
        [HttpPut("{id:int}/read")]
        public async Task<ActionResult<BaseResponse<bool>>> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _chatManager.MarkMessagesAsReadAsync(id, userId);
            return Ok(response);
        }

        /// <summary>
        /// تحديث حالة محادثة محددة.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بتغيير حالة المحادثة (مثل نشط، مؤرشف، محظور) إلى الحالة المُرسلة في نص الطلب.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;bool&gt;</c>.
        /// يتطلب الإجراء أن يكون المستخدم المُوثَّق هو أحد أطراف المحادثة.
        /// الحالة الجديدة يتم تمريرها من التعداد <c>ChatStatus</c> عبر نص الطلب (JSON body).
        /// </remarks>
        /// <param name="id">معرف المحادثة المراد تحديث حالتها.</param>
        /// <param name="status">الحالة الجديدة للمحادثة من التعداد <c>ChatStatus</c>.</param>
        /// <returns>قيمة منطقية (bool) تشير إلى نجاح عملية التحديث مغلفة داخل <c>BaseResponse</c>.</returns>
        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<BaseResponse<bool>>> UpdateStatus(int id, [FromBody] ChatStatus status)
        {
            var userId = GetCurrentUserId();
            var response = await _chatManager.UpdateChatStatusAsync(id, userId, status);
            return Ok(response);
        }

        private int GetCurrentUserId()
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(claimValue, out int userId);
            return userId;
        }
    }
}
