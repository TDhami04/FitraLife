// ==================== CHAT WIDGET FUNCTIONALITY ====================
const chatToggle = document.getElementById('chatToggle');
const chatWindow = document.getElementById('chatWindow');
const chatBody = document.getElementById('chatBody');
const chatInput = document.getElementById('chatInput');
const sendBtn = document.getElementById('sendMessageBtn');
const clearBtn = document.getElementById('clearChatBtn');
const minimizeBtn = document.getElementById('minimizeChatBtn');
const sendIcon = document.getElementById('sendIcon');
const sendSpinner = document.getElementById('sendSpinner');
const historyBtn = document.getElementById('historyBtn');
const chatHistoryView = document.getElementById('chatHistoryView');
const historyList = document.getElementById('historyList');
const newChatBtn = document.getElementById('newChatBtn');

let isOpen = false;
let currentSessionId = null;
let chatMessages = [];
let sessions = [];

// Toggle chat window
chatToggle.addEventListener('click', () => {
    isOpen = !isOpen;
    chatToggle.classList.toggle('active', isOpen);
    chatWindow.classList.toggle('show', isOpen);

    if (isOpen) {
        if (!currentSessionId) {
            // Try to load last session or show new chat
            loadSessions();
        }
        chatInput.focus();
    }
});

// Minimize chat
minimizeBtn.addEventListener('click', () => {
    isOpen = false;
    chatToggle.classList.remove('active');
    chatWindow.classList.remove('show');
});

// Toggle History View
if (historyBtn) {
    historyBtn.addEventListener('click', () => {
        chatHistoryView.classList.toggle('d-none');
        if (!chatHistoryView.classList.contains('d-none')) {
            loadSessions();
        }
    });
}

// New Chat
if (newChatBtn) {
    newChatBtn.addEventListener('click', () => {
        currentSessionId = null;
        chatMessages = [];
        renderChatMessages();
        chatHistoryView.classList.add('d-none');
        // Deselect all in list
        document.querySelectorAll('.history-item').forEach(el => el.classList.remove('active'));
    });
}

// Load Sessions
async function loadSessions() {
    try {
        const response = await fetch('/api/Chat/sessions');
        if (response.ok) {
            sessions = await response.json();
            renderSessions();
        }
    } catch (error) {
        console.error('Failed to load sessions:', error);
    }
}

// Render Sessions List
function renderSessions() {
    if (!historyList) return;
    
    if (sessions.length === 0) {
        historyList.innerHTML = '<div class="text-center p-3 text-muted">No chat history</div>';
        return;
    }

    historyList.innerHTML = sessions.map(session => `
        <div class="history-item ${session.id === currentSessionId ? 'active' : ''}" onclick="loadSession(${session.id})">
            <div class="d-flex justify-content-between align-items-center">
                <div class="text-truncate fw-bold" style="max-width: 180px;">${escapeHtml(session.title)}</div>
                <button class="btn btn-sm btn-link text-danger p-0" onclick="deleteSession(event, ${session.id})">×</button>
            </div>
            <small class="text-muted">${new Date(session.createdAt).toLocaleDateString()}</small>
        </div>
    `).join('');
}

// Load Specific Session
async function loadSession(id) {
    currentSessionId = id;
    if (chatHistoryView) chatHistoryView.classList.add('d-none');
    
    try {
        const response = await fetch(`/api/Chat/session/${id}`);
        if (response.ok) {
            chatMessages = await response.json();
            renderChatMessages();
            // Update active state in list
            renderSessions();
        }
    } catch (error) {
        console.error('Failed to load session:', error);
    }
}

// Delete Session
async function deleteSession(event, id) {
    event.stopPropagation();
    if (!confirm('Delete this chat?')) return;

    try {
        const response = await fetch(`/api/Chat/session/${id}`, { method: 'DELETE' });
        if (response.ok) {
            if (currentSessionId === id) {
                currentSessionId = null;
                chatMessages = [];
                renderChatMessages();
            }
            loadSessions();
        }
    } catch (error) {
        console.error('Failed to delete session:', error);
    }
}

// Render chat messages
function renderChatMessages() {
    if (chatMessages.length === 0) {
        chatBody.innerHTML = `
            <div class="chat-welcome text-center py-4">
                <div class="mb-3">🤖</div>
                <h6>Hello! I'm your AI fitness assistant</h6>
                <p class="small text-muted">Ask me anything about workouts, nutrition, or wellness!</p>
            </div>`;
        return;
    }

    chatBody.innerHTML = chatMessages.map(msg => {
        if (msg.role === 'user') {
            return `
                <div class="chat-message user-message">
                    <div class="message-bubble">${escapeHtml(msg.content)}</div>
                </div>`;
        } else {
            return `
                <div class="chat-message ai-message">
                    <div class="message-avatar">🤖</div>
                    <div class="message-bubble">${formatAiMessage(msg.content)}</div>
                </div>`;
        }
    }).join('');

    scrollToBottom();
}

// Send message
async function sendMessage() {
    const message = chatInput.value.trim();
    if (!message) return;

    // Disable input
    chatInput.disabled = true;
    sendBtn.disabled = true;
    sendIcon.classList.add('d-none');
    sendSpinner.classList.remove('d-none');

    // Add user message to UI immediately
    chatMessages.push({ role: 'user', content: message, timestamp: new Date() });
    renderChatMessages();
    chatInput.value = '';

    try {
        const response = await fetch('/api/Chat/send', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ 
                message: message,
                sessionId: currentSessionId 
            })
        });

        if (response.ok) {
            const data = await response.json();
            currentSessionId = data.sessionId; // Update session ID if new
            chatMessages.push({ role: 'assistant', content: data.response, timestamp: new Date() });
            renderChatMessages();
            // Refresh list if it was a new session
            if (sessions.length === 0 || sessions[0].id !== currentSessionId) {
                loadSessions();
            }
        } else {
            throw new Error('Failed to send message');
        }
    } catch (error) {
        console.error('Error sending message:', error);
        chatMessages.push({
            role: 'assistant',
            content: '⚠️ Sorry, I encountered an error. Please try again.',
            timestamp: new Date()
        });
        renderChatMessages();
    } finally {
        // Re-enable input
        chatInput.disabled = false;
        sendBtn.disabled = false;
        sendIcon.classList.remove('d-none');
        sendSpinner.classList.add('d-none');
        chatInput.focus();
    }
}

// Send on Enter
chatInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
});

sendBtn.addEventListener('click', sendMessage);

// Quick prompts
document.querySelectorAll('.quick-prompt-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        chatInput.value = btn.dataset.prompt;
        sendMessage();
    });
});

// Clear chat (Current Session)
clearBtn.addEventListener('click', async () => {
    if (currentSessionId) {
        if (confirm('Delete this chat session?')) {
            await deleteSession({ stopPropagation: () => {} }, currentSessionId);
        }
    } else {
        chatMessages = [];
        renderChatMessages();
    }
});

// Helper functions
function scrollToBottom() {
    chatBody.scrollTop = chatBody.scrollHeight;
}

function formatAiMessage(text) {
    if (!text) return '';
    // Escape HTML first for security
    let formatted = escapeHtml(text);

    // Convert **bold** text
    formatted = formatted.replace(/\*\*(.*?)\*\*/g, '<strong class="ai-bold">$1</strong>');

    // Convert *italic* text
    formatted = formatted.replace(/\*(.*?)\*/g, '<em class="ai-italic">$1</em>');

    // Convert line breaks to <br> tags
    formatted = formatted.replace(/\n/g, '<br>');

    // Convert numbered lists (1. Item)
    formatted = formatted.replace(/^(\d+)\.\s+(.+)$/gm, '<div class="ai-list-item"><span class="ai-list-number">$1.</span> $2</div>');

    // Convert bullet points (- Item or • Item)
    formatted = formatted.replace(/^[-•]\s+(.+)$/gm, '<div class="ai-list-item"><span class="ai-bullet">•</span> $2</div>');

    // Convert headers (## Header)
    formatted = formatted.replace(/^##\s+(.+)$/gm, '<div class="ai-header">$1</div>');

    // Convert code blocks (`code`)
    formatted = formatted.replace(/`([^`]+)`/g, '<code class="ai-code">$1</code>');

    return formatted;
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}