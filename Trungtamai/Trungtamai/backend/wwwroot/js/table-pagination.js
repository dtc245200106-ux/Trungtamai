(function () {
    const tables = new Map();
    const pageSize = 10;

    function refresh(tableId, resetPage) {
        const state = tables.get(tableId);
        if (!state) return;

        const rows = Array.from(state.table.querySelectorAll("tbody [data-pagination-row]"));
        rows.forEach(row => { row.hidden = false; });

        const visibleRows = rows.filter(row => row.style.display !== "none");
        const totalPages = Math.ceil(visibleRows.length / pageSize);
        if (resetPage) state.currentPage = 1;
        state.currentPage = Math.max(1, Math.min(state.currentPage, totalPages || 1));

        const start = (state.currentPage - 1) * pageSize;
        visibleRows.forEach((row, index) => {
            row.hidden = index < start || index >= start + pageSize;
        });

        renderControls(state, visibleRows.length, totalPages, start);
    }

    function renderControls(state, totalItems, totalPages, start) {
        const container = state.controls;
        if (!container) return;
        container.replaceChildren();

        if (totalItems === 0) {
            container.classList.add("hidden");
            return;
        }

        container.classList.remove("hidden");
        const summary = document.createElement("span");
        summary.className = "text-sm text-on-surface-variant";
        summary.textContent = `Hiển thị ${start + 1}-${Math.min(start + pageSize, totalItems)} trên ${totalItems}`;
        container.appendChild(summary);

        const buttons = document.createElement("div");
        buttons.className = "flex items-center gap-1";
        container.appendChild(buttons);

        const addButton = (label, page, disabled, active) => {
            const button = document.createElement("button");
            button.type = "button";
            button.textContent = label;
            button.disabled = disabled;
            button.className = active
                ? "min-w-9 h-9 px-2 rounded-lg bg-primary text-white text-sm font-medium"
                : "min-w-9 h-9 px-2 rounded-lg border border-outline-variant text-sm text-on-surface-variant hover:bg-surface-container disabled:opacity-40 disabled:cursor-not-allowed";
            button.addEventListener("click", () => {
                state.currentPage = page;
                refresh(state.table.id, false);
            });
            buttons.appendChild(button);
        };

        addButton("Trước", state.currentPage - 1, state.currentPage <= 1, false);
        for (let page = 1; page <= totalPages; page++) {
            addButton(String(page), page, false, page === state.currentPage);
        }
        addButton("Sau", state.currentPage + 1, state.currentPage >= totalPages, false);
    }

    window.TablePagination = {
        init(tableId, controlsId) {
            const table = document.getElementById(tableId);
            const controls = document.getElementById(controlsId);
            if (!table || !controls) return;

            tables.set(tableId, { table, controls, currentPage: 1 });
            refresh(tableId, true);
        },
        refresh
    };
})();
