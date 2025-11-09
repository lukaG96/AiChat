import React, { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import "./App.css";

function App() {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [messages, setMessages] = useState<{ user: string; text: string }[]>([]);
  const [message, setMessage] = useState("");

  useEffect(() => {
    const apiUrl = process.env.REACT_APP_API_URL || "/chatHub";
    const connect = new signalR.HubConnectionBuilder()
      .withUrl(apiUrl)
      .withAutomaticReconnect()
      .build();

    connect.on("ReceiveMessage", (user, text) => {
      setMessages((prev) => [...prev, { user, text }]);
    });

    connect.start()
      .then(() => console.log("✅ Connected to SignalR hub"))
      .catch(console.error);

    setConnection(connect);
  }, []);

  const sendMessage = async () => {
    if (connection && message.trim()) {
      await connection.invoke("SendMessage", "User", message);
      setMessages((prev) => [...prev, { user: "You", text: message }]);
      setMessage("");
    }
  };

  return (
    <div className="chat-container">
      <h2>AI Chat</h2>
      <div className="messages">
        {messages.map((m, i) => (
          <div key={i} className={m.user === "You" ? "user-msg" : "bot-msg"}>
            <strong>{m.user}:</strong> {m.text}
          </div>
        ))}
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
