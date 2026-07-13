using System;
using System.Linq;
using System.Collections.Generic;
using SupportChat.Backend.Constants;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepo;
    private readonly IMessageService _messageService;
    private readonly ICustomerService _customerService;
    private readonly IAuthService _auth_service;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IChatRepository chatRepo,
        IMessageService messageService,
        ICustomerService customerService,
        IAuthService authService,
        IAuditService auditService,
        INotificationService notificationService,
        ILogger<ChatService> logger)
    {
        _chatRepo = chatRepo;
        _messageService = messageService;
    _customer_service: _customerService = customerService;
        _auth_service = authService;
        _auditService = auditService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Chat> GetByIdAsync(long chatId)
    {
        return await _chatRepo.GetChatByIdAsync(chatId);
    }

    public async Task<IEnumerable<Chat>> GetByCompanyAsync(long companyId)
    {
        return await _chatRepo.GetChatsByCompanyAsync(companyId);
    }

    public async Task<long> CreateChatAsync(CreateChatRequest request)
    {
        // Create or get customer via service (service expects CreateOrGetCustomerRequest)
        var customerRequest = System.Text.Json.JsonSerializer.Deserialize<CreateOrGetCustomerRequest>(
            System.Text.Json.JsonSerializer.Serialize(request)) ?? new CreateOrGetCustomerRequest();

        var existingCustomer = await _customerService.GetByIdAsync(request.CustomerId);
        if (existingCustomer == null)
        {
            
        Customer customerRecord = await _customerService.CreateOrGetAsync(request.CompanyId, customerRequest);
        }
        

        // Create chat domain object and persist
        var chatEntity = new Chat
        {
            CompanyId = request.CompanyId,
            CustomerId = existingCustomer.Id,
            DepartmentId = request.DepartmentId,
            Subject = request.Subject,
            Priority = ChatPriority.Medium,
            StartedOn = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow
        };

        var chatId = await _chatRepo.CreateChatAsync(chatEntity);

        // Create system message (chat started)
        var systemMessage = new SendMessageRequest
        {
            ChatId = chatId,
            SenderType = SenderType.System,
            SenderId = existingCustomer.Id,
            MessageType = MessageType.System,
            Message = "Chat started by customer."
        };

        await _messageService.SendMessageAsync(systemMessage);

        // Notify agents (via SignalR)
        await _notificationService.NotifyNewChatAsync(request.CompanyId, chatId,existingCustomer.Id);

        // Audit
        await _auditService.CreateAuditLogAsync(
            request.CompanyId,
            null,
            "ChatCreated",
            "Chat",
            chatId,
            null,
            $"CustomerId={existingCustomer.Id}, Subject={request.Subject}",
            null,
            null
        );

        return chatId;
    }

    public async Task AssignAgentAsync(long chatId, long agentId)
    {
        var chat = await _chatRepo.GetChatByIdAsync(chatId);
        if (chat == null)
        {
            throw new InvalidOperationException("Chat not found.");
        }

        // Verify agent belongs to the company
        var agent = await _auth_service.GetUserByIdAsync(agentId);
        if (agent == null || agent.CompanyId != chat.CompanyId)
        {
            throw new InvalidOperationException("Agent does not belong to this company.");
        }

        // Check if agent is active
        if (!agent.IsActive || agent.IsDeleted)
        {
            throw new InvalidOperationException("Agent is not active.");
        }

        await _chatRepo.AssignAgentToChatAsync(chatId, agentId);

        // Send system message
        var systemMessage = new SendMessageRequest
        {
            ChatId = chatId,
            SenderType = SenderType.System,
            SenderId = null,
            MessageType = MessageType.System,
            Message = $"Chat assigned to Agent {agent.FirstName} {agent.LastName}."
        };

        await _messageService.SendMessageAsync(systemMessage);

        // Audit
        await _auditService.CreateAuditLogAsync(
            chat.CompanyId,
            agentId,
            "ChatAssigned",
            "Chat",
            chatId,
            null,
            $"Assigned to AgentId={agentId}",
            null,
            null
        );
    }

    public async Task CloseChatAsync(long chatId)
    {
        var chat = await _chatRepo.GetChatByIdAsync(chatId);
        if (chat == null)
        {
            throw new InvalidOperationException("Chat not found.");
        }

        await _chatRepo.CloseChatAsync(chatId);

        // Send system message
        var systemMessage = new SendMessageRequest
        {
            ChatId = chatId,
            SenderType = SenderType.System,
            SenderId = null,
            MessageType = MessageType.System,
            Message = "Chat has been closed."
        };

        await _messageService.SendMessageAsync(systemMessage);

        await _auditService.CreateAuditLogAsync(
            chat.CompanyId,
            null,
            "ChatClosed",
            "Chat",
            chatId,
            null,
            "Chat closed.",
            null,
            null
        );
    }

    public async Task TransferChatAsync(long chatId, long newAgentId)
    {
        var chat = await _chatRepo.GetChatByIdAsync(chatId);
        if (chat == null)
        {
            throw new InvalidOperationException("Chat not found.");
        }

        // Verify agent belongs to the company
        var agent = await _auth_service.GetUserByIdAsync(newAgentId);
        if (agent == null || agent.CompanyId != chat.CompanyId)
        {
            throw new InvalidOperationException("Agent does not belong to this company.");
        }

        // Assign to new agent
        await _chatRepo.AssignAgentToChatAsync(chatId, newAgentId);
    }

    public async Task<bool> IsAgentAvailableAsync(long agentId)
    {
        var agent = await _auth_service.GetUserByIdAsync(agentId);
        if (agent == null) return false;
        return agent.IsOnline && agent.IsActive && !agent.IsDeleted;
    }

    public async Task<IEnumerable<Chat>> GetAgentActiveChatsAsync(long agentId)
    {
        var agent = await _auth_service.GetUserByIdAsync(agentId);
        if (agent == null) return Enumerable.Empty<Chat>();

        var companyChats = await _chatRepo.GetChatsByCompanyAsync(agent.CompanyId);
        return companyChats.Where(c => c.AssignedAgentId == agentId && c.Status != ChatStatus.Closed);
    }

    public async Task<bool> ValidateCustomerTokenAsync(long chatId, string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        var chat = await _chatRepo.GetChatByIdAsync(chatId);
        if (chat == null) return false;

        var expected = $"chat-{chatId}-{chat.CustomerId}-{Guid.NewGuid()}";
        return string.Equals(token, expected, StringComparison.Ordinal);
    }
}