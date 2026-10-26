function escapeChatbotHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function formatInlineMarkdown(value) {
  return escapeChatbotHtml(value)
    .replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>")
    .replace(/\*(.+?)\*/g, "<em>$1</em>");
}

function formatChatbotResponse(value) {
  const normalized = String(value ?? "")
    .replaceAll("\r\n", "\n")
    .replace(/\s*(?=#{1,3}\s)/g, "\n")
    .replace(/\s+-\s+(?=[A-ZÀ-ỸĐ])/gu, "\n- ");

  return normalized
    .split("\n")
    .map((line) => {
      const trimmed = line.trim();
      if (!trimmed) return '<div class="chatbot-spacer"></div>';

      const headingMatch = trimmed.match(/^#{1,3}\s+(.+)$/u);
      if (headingMatch) {
        return `<div class="chatbot-heading">${formatInlineMarkdown(headingMatch[1])}</div>`;
      }

      const listMatch = trimmed.match(/^([-*]|\d+[.)])\s+(.+)$/u);
      if (listMatch) {
        const marker = /^\d/u.test(listMatch[1])
          ? `${listMatch[1].replace(/[.)]$/u, "")}.`
          : "•";
        return `<div class="chatbot-list-item"><span class="chatbot-list-marker">${marker}</span><span>${formatInlineMarkdown(listMatch[2])}</span></div>`;
      }

      return `<div class="chatbot-paragraph">${formatInlineMarkdown(trimmed)}</div>`;
    })
    .join("");
}

function getChatbotMessageText(row) {
  const bubble =
    row.querySelector(".chatbot-response") ||
    Array.from(row.children).filter(
      (child) => !child.querySelector(".material-symbols-outlined"),
    ).at(-1);
  if (!bubble) return "";

  const copy = bubble.cloneNode(true);
  copy.querySelectorAll("[data-chatbot-export-controls]").forEach((item) => item.remove());
  return (copy.innerText || copy.textContent || "").trim();
}

function requestedExportFormats(message) {
  const normalized = String(message ?? "").toLocaleLowerCase("vi");
  const mentionsWord = /(word|docx)/u.test(normalized);
  const mentionsExcel = /(excel|xlsx)/u.test(normalized);
  const asksOnlyAboutCapability =
    /(có thể|ko thể|không thể|được không|được ko|được chứ)/u.test(normalized) &&
    !/(hãy tạo|tạo giúp|tạo cho|xuất giúp|làm giúp|soạn giúp)/u.test(normalized);
  const requestsFile =
    !asksOnlyAboutCapability &&
    (/(tạo|tao|xuất|xuat|làm|lam|soạn|soan|viết|viet)[\s\S]{0,40}(word|docx|excel|xlsx)/u.test(normalized) ||
      /(gửi|gui)[\s\S]{0,30}(file|tài liệu|bảng tính)[\s\S]{0,15}(word|docx|excel|xlsx)/u.test(normalized) ||
      /(file|tài liệu|bảng tính)[\s\S]{0,15}(word|docx|excel|xlsx)/u.test(normalized));
  if (!requestsFile) return [];

  return [
    ...(mentionsWord ? ["word"] : []),
    ...(mentionsExcel ? ["excel"] : []),
  ];
}

async function downloadChatbotDocument(content, format) {
  const response = await fetch("/api/chatbot/export", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "same-origin",
    body: JSON.stringify({ format, content }),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => null);
    throw new Error(error?.message || "Không thể tạo tệp.");
  }

  const blob = await response.blob();
  const extension = format === "word" ? "docx" : "xlsx";
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `LinguistAI-${new Date().toISOString().replace(/[:.]/g, "-")}.${extension}`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.setTimeout(() => URL.revokeObjectURL(url), 1000);
}

function addChatbotExportControls(row, content) {
  const bubble =
    row.querySelector(".chatbot-response") ||
    Array.from(row.children).filter(
      (child) => !child.querySelector(".material-symbols-outlined"),
    ).at(-1);
  if (!bubble || bubble.querySelector("[data-chatbot-export-controls]")) return;

  const controls = document.createElement("div");
  controls.dataset.chatbotExportControls = "true";
  controls.className = "mt-2 flex flex-wrap gap-2 border-t border-outline-variant pt-2";

  for (const [format, label] of [["word", "Tải Word"], ["excel", "Tải Excel"]]) {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = label;
    button.className =
      "rounded-md border border-outline-variant px-2 py-1 text-xs font-medium text-primary hover:bg-surface-container";
    button.addEventListener("click", async () => {
      button.disabled = true;
      try {
        await downloadChatbotDocument(content, format);
      } catch (error) {
        alert(error instanceof Error ? error.message : "Không thể tạo tệp.");
      } finally {
        button.disabled = false;
      }
    });
    controls.appendChild(button);
  }

  bubble.appendChild(controls);
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
      "bg-white border border-outline-variant rounded-xl rounded-tl-none px-3 py-2 text-sm max-w-[80%] break-words leading-6 chatbot-response";
    bubble.innerHTML = formatChatbotResponse(noiDung);
  }

  row.appendChild(bubble);
  messages.appendChild(row);
  messages.scrollTop = messages.scrollHeight;
}

function installChatbotFileExports(messages) {
  let lastUserRequest = "";

  function processRows(rows, autoExport = true) {
    for (const row of rows) {
      if (!(row instanceof HTMLElement) || row.parentElement !== messages) continue;

      const isUserMessage =
        row.classList.contains("justify-end") ||
        row.className.includes("justify-end");
      if (isUserMessage) {
        lastUserRequest = getChatbotMessageText(row);
        continue;
      }

      const content = getChatbotMessageText(row);
      if (!content || content.includes("Đang xử lý...") || content.includes("Đang phân tích ảnh...")) {
        continue;
      }

      addChatbotExportControls(row, content);
      const formats = requestedExportFormats(lastUserRequest);
      lastUserRequest = "";
      if (autoExport && formats.length === 1) {
        void (async () => {
          const controls = row.querySelector("[data-chatbot-export-controls]");
          if (controls) controls.firstElementChild.textContent = "Đang tạo tệp...";
          try {
            for (const format of formats) {
              await downloadChatbotDocument(content, format);
            }
            if (controls) controls.firstElementChild.textContent = "Đã tải tệp";
          } catch (error) {
            if (controls) controls.firstElementChild.textContent = "Tạo tệp thất bại";
            console.error("Không thể tạo tệp chatbot:", error);
          }
        })();
      }
    }
  }

  processRows(Array.from(messages.children), false);

  const observer = new MutationObserver((mutations) => {
    const rows = new Set();
    for (const mutation of mutations) {
      const row =
        mutation.target instanceof Element
          ? mutation.target.closest("#oChatMessages > *")
          : mutation.target.parentElement?.closest("#oChatMessages > *");
      if (row) rows.add(row);

      for (const node of mutation.addedNodes) {
        if (node instanceof HTMLElement && node.parentElement === messages) rows.add(node);
      }
    }
    processRows(Array.from(rows));
  });
  observer.observe(messages, { childList: true, subtree: true });
}

const chatbotStyle = document.createElement("style");
chatbotStyle.textContent = `
  .chatbot-response { overflow-wrap: anywhere; line-height: 1.6; }
  .chatbot-heading { margin: 0.7rem 0 0.25rem; color: #091426; font-weight: 700; line-height: 1.35; }
  .chatbot-heading:first-child { margin-top: 0; }
  .chatbot-paragraph { margin: 0.2rem 0; }
  .chatbot-spacer { height: 0.35rem; }
  .chatbot-list-item { display: flex; gap: 0.45rem; align-items: flex-start; margin: 0.15rem 0; }
  .chatbot-list-marker { flex: 0 0 1.25rem; color: #0b1736; font-weight: 700; text-align: right; }
  .chatbot-response strong { color: #0b1736; }
  .chatbot-response em { font-style: italic; color: #3b4657; }
`;
document.head.appendChild(chatbotStyle);

document.addEventListener("DOMContentLoaded", async () => {
  const messages = document.getElementById("oChatMessages");
  if (!messages) return;

  try {
    const response = await fetch("/api/chatbot/history", {
      credentials: "same-origin",
    });
    if (!response.ok) return;
    const history = await response.json();
    if (history.length) {
      messages.innerHTML = "";
      history.forEach((item) => {
        themTinNhanChatbot(item.userMessage, "nguoiDung");
        themTinNhanChatbot(item.botResponse, "bot");
      });
    }
  } catch (error) {
    console.error("Lỗi tải lịch sử chatbot:", error);
  }

  installChatbotFileExports(messages);
});
