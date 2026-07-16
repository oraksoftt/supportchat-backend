using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using SupportChat.Backend.Constants;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Models.Responses;
using SupportChat.Backend.Services.Interfaces;
using SupportChat.Backend.Models.DTOs;

namespace SupportChat.Backend.Hubs;

public class ChatHub : Hub
{
    private readonly IAuthService _authService;
    private readonly IChatService _chatService;
    private readonly IMessageService _messageService;
    private readonly INotificationService _notificationService;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IAuthService authService,
        IChatService chatService,
        IMessageService messageService,
        INotificationService notificationService,
        IConnectionManager connectionManager,
        ILogger<ChatHub> logger)
    {
        _authService = authService;
        _chatService = chatService;
        _messageService = messageService;
        _notificationService = notificationService;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    private bool IsAgentConnection => Context.User?.Identity?.IsAuthenticated ?? false;

    private long? TryGetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private long? TryGetCompanyId()
    {
        var companyIdClaim = Context.User?.FindFirst("CompanyId")?.Value;
        return long.TryParse(companyIdClaim, out var companyId) ? companyId : null;
    }

    private long GetUserId() => TryGetUserId() ?? throw new HubException("User not authenticated.");
    private long GetCompanyId() => TryGetCompanyId() ?? throw new HubException("Company context missing.");

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;

        try
        {
            if (IsAgentConnection)
            {
                var userId = GetUserId();
                var companyId = GetCompanyId();

                var valid = await _authService.ValidateUserAsync(userId);
                if (!valid)
                {
                    _logger.LogWarning("Rejected connection for inactive agent {UserId}", userId);
                    Context.Abort();
                    return;
                }

                await _connectionManager.AddConnectionAsync(userId, connectionId);
                try { await _authService.SetOnlineStatusAsync(userId, true); } catch { }

                await Groups.AddToGroupAsync(connectionId, $"company-{companyId}");

                if (_connectionManager is Services.ConnectionManager concreteManager)
                {
                    try { concreteManager.AddUserToCompany(companyId, userId); }
                    catch (Exception ex) { _logger.LogDebug(ex, "AddUserToCompany failed"); }
                }

                _logger.LogInformation("Agent {UserId} connected. ConnectionId: {ConnectionId}", userId, connectionId);
                _ = Clients.Group($"company-{companyId}").SendAsync("UserOnline", userId);
            }
            else
            {
                _logger.LogInformation("Anonymous customer client connected. ConnectionId: {ConnectionId}", connectionId);
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during OnConnectedAsync for Connection: {ConnectionId}", connectionId);
            await Clients.Caller.SendAsync("ConnectionError", "Failed to establish a valid connection socket.");
            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var currentConnId = Context.ConnectionId;

            if (IsAgentConnection)
            {
                var userId = TryGetUserId();
                var companyId = TryGetCompanyId();

                await _connectionManager.RemoveConnectionAsync(currentConnId);

                if (userId.HasValue && companyId.HasValue)
                {
                    var hasOtherConnections = await HasOtherConnections(userId.Value, companyId.Value);
                    if (!hasOtherConnections)
                    {
                        await _authService.SetOnlineStatusAsync(userId.Value, false);

                        if (_connectionManager is SupportChat.Backend.Services.ConnectionManager concreteManager)
                        {
                            concreteManager.RemoveUserFromCompany(companyId.Value, userId.Value);
                        }

                        await Clients.Group($"company-{companyId.Value}").SendAsync("UserOffline", userId.Value);
                    }
                    _logger.LogInformation("Agent {UserId} completely disconnected.", userId);
                }
            }
            else
            {
                await _connectionManager.RemoveConnectionAsync(currentConnId);
                _logger.LogInformation("Anonymous customer disconnected: {ConnectionId}", currentConnId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clean-up context inside OnDisconnectedAsync.");
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> HasOtherConnections(long userId, long companyId)
    {
        var currentConnId = Context.ConnectionId;
        try
        {
            var ids = await _connectionManager.GetConnectionIdsByUserAsync(userId);
            if (ids != null && ids.Any())
                return ids.Any(id => id != currentConnId && !string.IsNullOrEmpty(id));
        }
        catch { }

        try
        {
            var allConnections = await _connectionManager.GetConnectionIdsByCompanyAsync(companyId);
            return allConnections.Any(cid => cid != currentConnId && cid != null);
        }
        catch
        {
            return false;
        }
    }

    public async Task JoinChatRoom(long chatId)
    {
        if (!IsAgentConnection)
            throw new HubException("Access Denied. Only agents can manually join generic chat structures.");

        try
        {
            var userId = GetUserId();
            var companyId = GetCompanyId();
            var chat = await _chatService.GetByIdAsync(chatId);

            if (chat == null) throw new HubException("Chat not found.");
            if (chat.CompanyId != companyId) throw new HubException("Permission Denied.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{chatId}");
            _logger.LogInformation("Agent {UserId} joined chat room {ChatId}", userId, chatId);
        }
        catch (Exception ex) when (ex is not HubException)
        {
            _logger.LogError(ex, "Error joining chat room {ChatId}", chatId);
            throw new HubException("Failed to join chat room.");
        }
    }

    public async Task JoinCustomerChat(long chatId, long customerId)
    {
        try
        {
            _logger.LogInformation("JoinCustomerChat called ChatId:{ChatId} CustomerId:{CustomerId}", chatId, customerId);

            if (customerId <= 0) throw new HubException("Invalid customer ID.");

            var chat = await _chatService.GetByIdAsync(chatId);
            _logger.LogInformation("Database Chat CustomerId:{CustomerId}", chat?.CustomerId);

            if (chat == null) throw new HubException("Chat not found.");
            if (chat.CustomerId != customerId) throw new HubException("Unauthorized.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{chatId}");
            _logger.LogInformation("Customer context assigned validation success for Chat {ChatId}", chatId);
        }
        catch (Exception ex) when (ex is not HubException)
        {
            _logger.LogError(ex, "Error in JoinCustomerChat for ID {CustomerId}", customerId);
            throw new HubException("Failed to register customer live stream space.");
        }
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        try
        {
            _logger.LogInformation("Authenticated={Auth}, UserId={UserId}, CompanyId={CompanyId}",
                                    Context.User?.Identity?.IsAuthenticated,
                                    Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                                    Context.User?.FindFirst("CompanyId")?.Value);

            long? senderId = null;
            long companyId;

            if (dto.SenderType == SenderType.Agent)
            {
                if (!IsAgentConnection) throw new HubException("Invalid system configuration permissions.");
                senderId = GetUserId();
                companyId = GetCompanyId();
            }
            else if (dto.SenderType == SenderType.Customer)
            {
                var chat = await _chatService.GetByIdAsync(dto.ChatId) ?? throw new HubException("Chat target unavailable.");
                companyId = chat.CompanyId;
                senderId = chat.CustomerId;
            }
            else
            {
                throw new HubException("Invalid sender type mapping structure.");
            }

            var request = new SendMessageRequest
            {
                ChatId = dto.ChatId,
                SenderType = dto.SenderType,
                SenderId = senderId,
                MessageType = dto.MessageType,
                Message = dto.Message
            };

            var message = await _messageService.SendMessageAsync(request);

            if (message.CreatedOn.Kind == DateTimeKind.Unspecified)
            {
                message.CreatedOn = DateTime.SpecifyKind(message.CreatedOn, DateTimeKind.Utc);
            }

            await Clients.Group($"chat-{dto.ChatId}").SendAsync("ReceiveMessage", message);

            await Clients.Group($"company-{companyId}")
                .SendAsync("NewMessage", new { ChatId = dto.ChatId, Message = message });
        }
        catch (Exception ex) when (ex is not HubException)
        {
            _logger.LogError(ex, "Error occurred processing streaming message output.");
            throw new HubException("Failed to send system event message update.");
        }
    }

    public async Task Typing(long chatId, bool isTyping)
    {
        try
        {
            long trackingId = IsAgentConnection ? GetUserId() : 0;
            await Clients.Group($"chat-{chatId}").SendAsync("Typing", trackingId, isTyping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing real-time typing events.");
        }
    }

    public async Task MarkAsSeen(long messageId)
    {
        try
        {
            await _messageService.MarkMessageAsSeenAsync(messageId);
            await Clients.Group($"message-{messageId}").SendAsync("MessageSeen", messageId);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message visibility state changes.");
        }
    }

    public async Task CloseChat(long chatId,long agentId)
    {
        var userId = GetUserId();

        if (!IsAgentConnection) throw new HubException("Unauthorized strategy call.");

        try
        {
            await _chatService.CloseChatAsync(chatId, userId);
            await Clients.Group($"chat-{chatId}").SendAsync("ChatClosed", chatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error altering chat room close state.");
            throw new HubException("Failed to cleanly update record close states.");
        }
    }
    public async Task AssignChat(long chatId, long agentId)
    {
        if (!IsAgentConnection)
            throw new HubException("Unauthorized.");

        await _chatService.AssignAgentAsync(chatId, agentId);

        await Clients.Group($"chat-{chatId}").SendAsync("ChatAssigned", chatId, agentId);
    }
    public async Task LeaveChatRoom(long chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat-{chatId}");
        _logger.LogInformation("Connection {ConnectionId} left chat {ChatId}", Context.ConnectionId, chatId);
    }
}


 