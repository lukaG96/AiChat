import React, { useEffect, useState, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import "./App.css";

interface ChatMessage {
  user: string;
  text: string;
  streaming?: boolean;
}

function App() {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [message, setMessage] = useState("");

  // Ref for auto-scrolling
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const connect = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5000/chatHubStreaming")
      .withAutomaticReconnect()
      .build();

    // Normal messages (user/system)
    connect.on("ReceiveMessage", (user, text) => {
      setMessages((prev) => [...prev, { user, text }]);
    });

    // Streaming chunks from AI
    connect.on("ReceiveStreamChunk", (user, chunk) => {
      setMessages((prev) => {
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
      setMessages((prev) => {
        const updated = [...prev];
        const idx = updated.findIndex((m) => m.user === user && m.streaming);
        if (idx !== -1) updated[idx] = { user, text: fullText, streaming: false };
        return updated;
      });
    });

    connect.start()
      .then(() => console.log("✅ Connected to SignalR hub"))
      .catch(console.error);

    setConnection(connect);

    // Clean up on unmount
    return () => {
      connect.stop();
    };
  }, []);

  useEffect(() => {
    // Auto-scroll to bottom on new message
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const sendMessage = async () => {
    if (connection && message.trim()) {
      // Send message to backend
      await connection.invoke("SendMessage", "User", message);
      setMessage("");
    }
  };

  return (
    <div className="chat-container">
      <h2>AI Chat (Streaming)</h2>
      <div className="messages">
        {messages.map((m, i) => (
          <div
            key={i}
            className={m.user === "You" ? "user-msg" : "bot-msg"}
          >
            <strong>{m.user}:</strong> {m.text}
            {m.streaming && <span className="cursor">▌</span>}
          </div>
        ))}
        <div ref={messagesEndRef} />
      </div>
      <div className="input-area">
        <input
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          placeholder="Type your message..."
          onKeyDown={(e) => e.key === "Enter" && sendMessage()}
        />
        <button onClick={sendMessage}>Send</button>
      </div>
    </div>
  );
}

export default App;
