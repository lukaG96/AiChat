import React, { useEffect, useState, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import "./App.css";

interface ChatMessage {
  user: string;
  text: string;
  streaming?: boolean;
}

function AppStreaming() {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [message, setMessage] = useState("");

  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Use Nginx URL for frontend to backend
    const apiUrl = process.env.REACT_APP_API_URL || "/chatHubStreaming";

    const connect = new signalR.HubConnectionBuilder()
      .withUrl(apiUrl)  // <- proxy through nginx
      .withAutomaticReconnect()
      .build();

    // Normal messages
    connect.on("ReceiveMessage", (user, text) => {
      setMessages(prev => [...prev, { user, text }]);
    });

    // Streaming AI chunks
    connect.on("ReceiveStreamChunk", (user, chunk) => {
      setMessages(prev => {
        const updated = [...prev];
        const last = updated[updated.length - 1];

        if (last && last.user === user && last.streaming) {
          last.text += chunk;
        } else {
          updated.push({ user, text: chunk, streaming: true });
        }
        return [...updated];
      });
    });

    // Streaming complete
    connect.on("ReceiveStreamComplete", (user, fullText) => {
      setMessages(prev => {
        const updated = [...prev];
        const idx = updated.findIndex(m => m.user === user && m.streaming);
        if (idx !== -1) updated[idx] = { user, text: fullText, streaming: false };
        return updated;
      });
    });

    connect.start()
      .then(() => console.log("✅ Connected to SignalR hub"))
      .catch(err => console.error("SignalR connection error:", err));

    setConnection(connect);

    return () => {
      connect.stop();
    };
  }, []);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const sendMessage = async () => {
    if (connection && message.trim()) {
      try {
        // Send to backend
        await connection.invoke("SendMessage", "You", message);

        // Add to local messages
        setMessages(prev => [...prev, { user: "You", text: message }]);
        setMessage("");
      } catch (err) {
        console.error("Send message error:", err);
      }
    }
  };

  return (
    <div className="chat-container">
      <h2>AI Chat (Streaming)</h2>
      <div className="messages">
        {messages.map((m, i) => (
          <div key={i} className={m.user === "You" ? "user-msg" : "bot-msg"}>
            <strong>{m.user}:</strong> {m.text}
            {m.streaming && <span className="cursor">▌</span>}
          </div>
        ))}
        <div ref={messagesEndRef} />
      </div>
      <div className="input-area">
        <input
          value={message}
          onChange={e => setMessage(e.target.value)}
          placeholder="Type your message..."
          onKeyDown={e => e.key === "Enter" && sendMessage()}
        />
        <button onClick={sendMessage}>Send</button>
      </div>
    </div>
  );
}

export default AppStreaming;
