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

let isOpen = false;
let chatHistory = [];

// Toggle chat window
chatToggle.addEventListener('click', () => {
    isOpen = !isOpen;
    chatToggle.classList.toggle('active', isOpen);
    chatWindow.classList.toggle('show', isOpen);

    if (isOpen) {
        loadChatHistory();
        chatInput.focus();
    }
});

// Minimize chat
minimizeBtn.addEventListener('click', () => {
    isOpen = false;
    chatToggle.classList.remove('active');
    chatWindow.classList.remove('show');
});

// Load chat history
async function loadChatHistory() {
    try {
        const response = await fetch('/api/Chat/history');
        if (response.ok) {
            chatHistory = await response.json();
            renderChatHistory();
        }
    } catch (error) {
        console.error('Failed to load chat history:', error);
    }
}

// Render chat messages
function renderChatHistory() {
    if (chatHistory.length === 0) {
        chatBody.innerHTML = `
            <div class="chat-welcome text-center py-4">
                <div class="mb-3">🤖</div>
                <h6>Hello! I'm your AI fitness assistant</h6>
                <p class="small text-muted">Ask me anything about workouts, nutrition, or wellness!</p>
            </div>`;
        return;
    }

    chatBody.innerHTML = chatHistory.map(msg => {
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

    // Add user message to UI
    chatHistory.push({ role: 'user', content: message, timestamp: new Date() });
    renderChatHistory();
    chatInput.value = '';

    try {
        const response = await fetch('/api/Chat/send', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ message: message })
        });

        if (response.ok) {
            const data = await response.json();
            chatHistory.push({ role: 'assistant', content: data.response, timestamp: data.timestamp });
            renderChatHistory();
        } else {
            throw new Error('Failed to send message');
        }
    } catch (error) {
        console.error('Error sending message:', error);
        chatHistory.push({
            role: 'assistant',
            content: '⚠️ Sorry, I encountered an error. Please try again.',
            timestamp: new Date()
        });
        renderChatHistory();
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

// Clear chat
clearBtn.addEventListener('click', async () => {
    if (confirm('Clear chat history?')) {
        try {
            await fetch('/api/Chat/clear', { method: 'POST' });
            chatHistory = [];
            renderChatHistory();
        } catch (error) {
            console.error('Failed to clear chat:', error);
        }
    }
});

// Helper functions
function scrollToBottom() {
    chatBody.scrollTop = chatBody.scrollHeight;
}

function formatAiMessage(text) {
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
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}