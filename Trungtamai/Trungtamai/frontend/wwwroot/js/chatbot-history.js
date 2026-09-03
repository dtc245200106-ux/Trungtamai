function escapeChatbotHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function formatChatbotResponse(value) {
  const lines = String(value ?? "")
    .replaceAll("\r\n", "\n")
    .split("\n");

  return lines
    .map((line) => {
      const trimmed = line.trim();
      const rendered = escapeChatbotHtml(line)
        .replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>")
        .replace(/\*(.+?)\*/g, "<em>$1</em>");

      if (/^\s*(#{1,3}\s+|[A-ZÀ-Ỹ][^.!?]{2,}:\s*$)/u.test(line) || /^\s*(BÀI TẬP|Bài tập|MỤC TIÊU|Mục tiêu|THỜI LƯỢNG|Thời lượng|ĐỀ BÀI|Đề bài|YÊU CẦU|Yêu cầu|ĐÁP ÁN GỢI Ý|Đáp án gợi ý|GỢI Ý CHẤM ĐIỂM|Gợi ý chấm điểm).*$/u.test(trimmed)) {
        return `<div class="chatbot-heading">${rendered.replace(/^\s*#{1,3}\s+/u, "")}</div>`;
      }

      if (/^\s*([-*]|\d+[.)])\s+/u.test(line)) {
        return `<div class="chatbot-list-item">${rendered}</div>`;
      }

      if (!trimmed) return "&nbsp;";
      return rendered;
    })
    .join("<br>");
}

function themTinNhanChatbot(noiDung, nguoiGui) {
  const messages = document.getElementById("oChatMessages");
  if (!messages) return;

  const row = document.createElement("div");
  const bubble = document.createElement("div");

  if (nguoiGui === "nguoiDung") {
    row.className = "flex items-start gap-2 justify-end";
    bubble.className =
      "bg-primary text-white rounded-xl rounded-tr-none px-3 py-2 text-sm max-w-[80%] whitespace-pre-wrap break-words";
    bubble.textContent = String(noiDung ?? "");
  } else {
    row.className = "flex items-start gap-2";
    row.innerHTML =
      '<div class="w-7 h-7 rounded-full bg-primary text-white flex items-center justify-center shrink-0"><span class="material-symbols-outlined text-[16px]">smart_toy</span></div>';
    bubble.className =
      "bg-white border border-outline-variant rounded-xl rounded-tl-none px-3 py-2 text-sm max-w-[80%] whitespace-pre-wrap break-words leading-6 chatbot-response";
    bubble.innerHTML = formatChatbotResponse(noiDung);
  }

  row.appendChild(bubble);
  messages.appendChild(row);
  messages.scrollTop = messages.scrollHeight;
}

const chatbotStyle = document.createElement("style");
chatbotStyle.textContent = `
  .chatbot-response { overflow-wrap: anywhere; line-height: 1.7; }
  .chatbot-heading { display: block; margin-top: 0.7rem; margin-bottom: 0.2rem; color: #091426; font-weight: 700; }
  .chatbot-list-item { display: block; padding-left: 1rem; margin-bottom: 0.15rem; position: relative; }
  .chatbot-list-item::before { content: "•"; position: absolute; left: 0.2rem; color: #0b1736; }
  .chatbot-response strong { color: #0b1736; }
  .chatbot-response em { font-style: italic; color: #3b4657; }
`;
document.head.appendChild(chatbotStyle);

document.addEventListener("DOMContentLoaded", async () => {
  const messages = document.getElementById("oChatMessages");
  if (!messages || typeof themTinNhanChatbot !== "function") return;

  try {
    const response = await fetch("/api/chatbot/history", {
      credentials: "same-origin",
    });
    if (!response.ok) return;

    const history = await response.json();
    if (!history.length) return;

    messages.innerHTML = "";
    history.forEach((item) => {
      themTinNhanChatbot(item.userMessage, "nguoiDung");
      themTinNhanChatbot(item.botResponse, "bot");
    });
  } catch (error) {
    console.error("Lỗi tải lịch sử chatbot:", error);
  }
});
