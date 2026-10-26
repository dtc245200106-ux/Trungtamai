(function () {
    const maxImageSize = 10 * 1024 * 1024;
    const supportedTypes = new Set(["image/jpeg", "image/png", "image/webp"]);

    function addMessage(text, imageFile, sender) {
        const messages = document.getElementById("oChatMessages");
        if (!messages) return;

        const row = document.createElement("div");
        row.className = sender === "user"
            ? "flex items-start justify-end gap-2"
            : "flex items-start gap-2";

        const bubble = document.createElement("div");
        bubble.className = sender === "user"
            ? "bg-primary text-white rounded-xl rounded-tr-none px-3 py-2 text-sm max-w-[85%] whitespace-pre-wrap"
            : "bg-white border border-outline-variant rounded-xl rounded-tl-none px-3 py-2 text-sm max-w-[85%] whitespace-pre-wrap";

        if (imageFile) {
            const preview = document.createElement("img");
            const imageUrl = URL.createObjectURL(imageFile);
            preview.src = imageUrl;
            preview.alt = imageFile.name;
            preview.className = "max-h-40 max-w-full rounded-lg mb-2 object-contain";
            preview.addEventListener("load", () => URL.revokeObjectURL(imageUrl), { once: true });
            bubble.appendChild(preview);
        }

        if (text) {
            const textNode = document.createElement("div");
            textNode.textContent = text;
            bubble.appendChild(textNode);
        }

        if (sender === "user") {
            row.appendChild(bubble);
        } else {
            const icon = document.createElement("div");
            icon.className = "w-7 h-7 rounded-full bg-primary text-white flex items-center justify-center shrink-0";
            icon.innerHTML = '<span class="material-symbols-outlined text-[16px]">smart_toy</span>';
            row.append(icon, bubble);
        }

        messages.appendChild(row);
        messages.scrollTop = messages.scrollHeight;
        return bubble;
    }

    function install(form) {
        const input = form.querySelector("#oChatInput");
        const sendButton = form.querySelector('button[type="submit"]');
        const messages = document.getElementById("oChatMessages");
        if (!input || !sendButton || !messages) return;

        const fileInput = document.createElement("input");
        fileInput.type = "file";
        fileInput.accept = "image/jpeg,image/png,image/webp";
        fileInput.hidden = true;
        fileInput.setAttribute("aria-label", "Chọn ảnh gửi cho chatbot");

        const attachButton = document.createElement("button");
        attachButton.type = "button";
        attachButton.title = "Đính kèm ảnh (JPEG, PNG, WebP; tối đa 10 MB)";
        attachButton.setAttribute("aria-label", attachButton.title);
        attachButton.className = "w-10 h-10 shrink-0 rounded-lg border border-outline-variant text-on-surface-variant flex items-center justify-center hover:bg-surface-container transition";
        attachButton.innerHTML = '<span class="material-symbols-outlined text-[20px]">image</span>';
        attachButton.addEventListener("click", () => fileInput.click());
        form.insertBefore(attachButton, input);
        form.appendChild(fileInput);

        const previewBar = document.createElement("div");
        previewBar.className = "hidden items-center gap-3 border-t border-outline-variant bg-white px-3 py-2";
        const previewImage = document.createElement("img");
        previewImage.className = "h-12 w-12 rounded object-cover";
        previewImage.alt = "Xem trước ảnh đính kèm";
        const fileName = document.createElement("span");
        fileName.className = "min-w-0 flex-1 truncate text-xs text-on-surface-variant";
        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.textContent = "Bỏ ảnh";
        removeButton.className = "shrink-0 text-xs font-medium text-red-600 hover:underline";
        previewBar.append(previewImage, fileName, removeButton);
        form.parentElement.insertBefore(previewBar, form);

        let previewUrl = null;
        function clearSelection() {
            fileInput.value = "";
            if (previewUrl) URL.revokeObjectURL(previewUrl);
            previewUrl = null;
            previewImage.removeAttribute("src");
            previewBar.classList.add("hidden");
            previewBar.classList.remove("flex");
        }

        fileInput.addEventListener("change", () => {
            const file = fileInput.files && fileInput.files[0];
            if (!file) {
                clearSelection();
                return;
            }
            if (!supportedTypes.has(file.type)) {
                clearSelection();
                alert("Chỉ hỗ trợ ảnh JPEG, PNG hoặc WebP.");
                return;
            }
            if (file.size > maxImageSize) {
                clearSelection();
                alert("Ảnh không được vượt quá 10 MB.");
                return;
            }
            if (previewUrl) URL.revokeObjectURL(previewUrl);
            previewUrl = URL.createObjectURL(file);
            previewImage.src = previewUrl;
            fileName.textContent = file.name;
            previewBar.classList.remove("hidden");
            previewBar.classList.add("flex");
        });
        removeButton.addEventListener("click", clearSelection);

        form.addEventListener("submit", async event => {
            const file = fileInput.files && fileInput.files[0];
            if (!file) return;

            event.preventDefault();
            event.stopImmediatePropagation();

            const message = input.value.trim();
            const prompt = message || "Hãy phân tích hình ảnh này và trả lời bằng tiếng Việt.";
            addMessage(message, file, "user");
            input.value = "";
            attachButton.disabled = true;
            sendButton.disabled = true;
            fileInput.disabled = true;

            const loadingBubble = addMessage("Đang phân tích ảnh...", null, "bot");
            try {
                const formData = new FormData();
                formData.append("message", prompt);
                formData.append("image", file, file.name);

                const response = await fetch("/api/chatbot/image", {
                    method: "POST",
                    credentials: "same-origin",
                    body: formData
                });
                const result = await response.json();
                if (!response.ok) throw new Error(result.message || "Không thể phân tích ảnh.");
                loadingBubble.textContent = result.answer || "AI không trả về nội dung.";
            } catch (error) {
                loadingBubble.textContent = error instanceof Error
                    ? error.message
                    : "Không thể kết nối đến chatbot.";
            } finally {
                clearSelection();
                fileInput.disabled = false;
                attachButton.disabled = false;
                sendButton.disabled = false;
                input.focus();
            }
        }, true);
    }

    document.querySelectorAll("form").forEach(install);
})();
