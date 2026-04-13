(function () {
    const state = {
        apiBaseUrl: "http://localhost:5160/api",
        token: null,
        user: null,
        employees: [],
        departments: [],
        pagination: {
            page: 1,
            pageSize: 10,
            totalCount: 0,
            totalPages: 0
        },
        filters: {
            search: "",
            department: "",
            status: "",
            sortBy: "createdAt",
            sortDir: "desc"
        }
    };

    const employeeModal = new bootstrap.Modal(document.getElementById("employeeModal"));

    $(document).ready(function () {
        wireEvents();
        applyRoleVisibility();
    });

    function wireEvents() {
        $("#loginForm").on("submit", handleLogin);
        $("#registerForm").on("submit", handleRegister);
        $("#filtersForm").on("submit", function (event) { event.preventDefault(); });
        $("#searchInput").on("input", debounce(function () {
            state.filters.search = $(this).val().trim();
            state.pagination.page = 1;
            loadEmployees();
        }, 350));
        $("#departmentFilter").on("change", function () {
            state.filters.department = $(this).val();
            state.pagination.page = 1;
            loadEmployees();
        });
        $("#statusFilter").on("change", function () {
            state.filters.status = $(this).val();
            state.pagination.page = 1;
            loadEmployees();
        });
        $("#sortBy").on("change", function () {
            state.filters.sortBy = $(this).val();
            loadEmployees();
        });
        $("#sortDir").on("change", function () {
            state.filters.sortDir = $(this).val();
            loadEmployees();
        });
        $("#pageSize").on("change", function () {
            state.pagination.pageSize = Number($(this).val());
            state.pagination.page = 1;
            loadEmployees();
        });
        $("#prevPageButton").on("click", function () {
            if (state.pagination.page > 1) {
                state.pagination.page -= 1;
                loadEmployees();
            }
        });
        $("#nextPageButton").on("click", function () {
            if (state.pagination.page < state.pagination.totalPages) {
                state.pagination.page += 1;
                loadEmployees();
            }
        });
        $("#refreshButton").on("click", refreshWorkspace);
        $("#logoutButton").on("click", logout);
        $("#addEmployeeButton").on("click", openCreateModal);
        $("#saveEmployeeButton").on("click", saveEmployee);
        $("#employeeTableBody").on("click", "[data-action='edit']", openEditModal);
        $("#employeeTableBody").on("click", "[data-action='delete']", deleteEmployee);
    }

    async function handleLogin(event) {
        event.preventDefault();
        const payload = {
            username: $("#loginUsername").val().trim(),
            password: $("#loginPassword").val()
        };

        try {
            const response = await request("/auth/login", "POST", payload, false);
            state.token = response.token;
            state.user = response.user;
            $("#loginForm")[0].reset();
            showAlert("Logged in successfully.", "success");
            showApp();
            await refreshWorkspace();
        } catch (error) {
            showAlert(error.message, "danger");
        }
    }

    async function handleRegister(event) {
        event.preventDefault();
        const payload = {
            username: $("#registerUsername").val().trim(),
            password: $("#registerPassword").val(),
            role: $("#registerRole").val()
        };

        try {
            await request("/auth/register", "POST", payload, false);
            $("#registerForm")[0].reset();
            showAlert("Account created. You can login now.", "success");
            bootstrap.Tab.getOrCreateInstance(document.getElementById("login-tab")).show();
        } catch (error) {
            showAlert(error.message, "danger");
        }
    }

    async function refreshWorkspace() {
        await Promise.all([loadDashboard(), loadEmployees()]);
    }

    async function loadDashboard() {
        try {
            const dashboard = await request("/employees/dashboard");
            $("#totalEmployees").text(dashboard.totalEmployees);
            $("#activeEmployees").text(dashboard.activeCount);
            $("#inactiveEmployees").text(dashboard.inactiveCount);
            renderDepartmentBreakdown(dashboard.departmentBreakdown);
            renderRecentEmployees(dashboard.recentEmployees);
            state.departments = dashboard.departmentBreakdown.map(item => item.department);
            syncDepartmentOptions();
        } catch (error) {
            showAlert(error.message, "danger");
        }
    }

    async function loadEmployees() {
        try {
            const query = $.param({
                search: state.filters.search || undefined,
                department: state.filters.department || undefined,
                status: state.filters.status || undefined,
                sortBy: state.filters.sortBy,
                sortDir: state.filters.sortDir,
                page: state.pagination.page,
                pageSize: state.pagination.pageSize
            });

            const response = await request(`/employees?${query}`);
            state.employees = response.items;
            state.pagination.page = response.page;
            state.pagination.pageSize = response.pageSize;
            state.pagination.totalCount = response.totalCount;
            state.pagination.totalPages = response.totalPages;
            renderEmployeeTable(response.items);
            renderPaginationSummary();
        } catch (error) {
            showAlert(error.message, "danger");
        }
    }

    function renderEmployeeTable(items) {
        const isAdmin = state.user && state.user.role === "Admin";
        if (!items.length) {
            $("#employeeTableBody").html(`<tr><td colspan="${isAdmin ? 8 : 7}" class="text-center text-muted py-4">No employees match the current filters.</td></tr>`);
            return;
        }

        const rows = items.map(employee => `
            <tr>
                <td>
                    <strong>${escapeHtml(employee.firstName)} ${escapeHtml(employee.lastName)}</strong><br>
                    <small class="text-muted">${escapeHtml(employee.phone)}</small>
                </td>
                <td>${escapeHtml(employee.email)}</td>
                <td>${escapeHtml(employee.department)}</td>
                <td>${escapeHtml(employee.designation)}</td>
                <td>${formatCurrency(employee.salary)}</td>
                <td><span class="status-pill ${employee.status === "Active" ? "status-active" : "status-inactive"}">${employee.status}</span></td>
                <td>${formatDate(employee.joinDate)}</td>
                ${isAdmin ? `
                    <td class="text-end">
                        <button class="btn btn-sm btn-link action-link" data-action="edit" data-id="${employee.id}">Edit</button>
                        <button class="btn btn-sm btn-link action-link danger" data-action="delete" data-id="${employee.id}">Delete</button>
                    </td>` : ""}
            </tr>`);

        $("#employeeTableBody").html(rows.join(""));
    }

    function renderDepartmentBreakdown(items) {
        if (!items.length) {
            $("#departmentBreakdown").html('<div class="empty-state">No department data available.</div>');
            return;
        }

        $("#departmentBreakdown").html(items.map(item => `
            <div class="stack-item">
                <span>${escapeHtml(item.department)}</span>
                <strong>${item.count}</strong>
            </div>
        `).join(""));
    }

    function renderRecentEmployees(items) {
        if (!items.length) {
            $("#recentEmployees").html('<div class="empty-state">No recent employees found.</div>');
            return;
        }

        $("#recentEmployees").html(items.map(item => `
            <div class="stack-item">
                <div>
                    <strong>${escapeHtml(item.firstName)} ${escapeHtml(item.lastName)}</strong>
                    <div class="text-muted small">${escapeHtml(item.department)} • ${escapeHtml(item.designation)}</div>
                </div>
                <span class="status-pill ${item.status === "Active" ? "status-active" : "status-inactive"}">${item.status}</span>
            </div>
        `).join(""));
    }

    function renderPaginationSummary() {
        const start = state.pagination.totalCount === 0
            ? 0
            : ((state.pagination.page - 1) * state.pagination.pageSize) + 1;
        const end = Math.min(state.pagination.page * state.pagination.pageSize, state.pagination.totalCount);
        $("#tableSummary").text(`${start}-${end} of ${state.pagination.totalCount} employees`);
        $("#prevPageButton").prop("disabled", state.pagination.page <= 1);
        $("#nextPageButton").prop("disabled", state.pagination.page >= state.pagination.totalPages);
    }

    function syncDepartmentOptions() {
        const currentValue = $("#departmentFilter").val();
        const options = ['<option value="">All Departments</option>']
            .concat(state.departments.map(department =>
                `<option value="${escapeAttribute(department)}">${escapeHtml(department)}</option>`));
        $("#departmentFilter").html(options.join(""));
        $("#departmentFilter").val(currentValue);
    }

    function showApp() {
        $("#authSection").addClass("d-none");
        $("#appSection").removeClass("d-none");
        $("#currentUserName").text(state.user.username);
        $("#currentUserRole").text(state.user.role);
        applyRoleVisibility();
    }

    function logout() {
        state.token = null;
        state.user = null;
        state.employees = [];
        state.pagination.page = 1;
        $("#appSection").addClass("d-none");
        $("#authSection").removeClass("d-none");
        $("#employeeTableBody").html('<tr><td colspan="8" class="text-center text-muted py-4">No employees loaded yet.</td></tr>');
        showAlert("Signed out.", "secondary");
    }

    function applyRoleVisibility() {
        const isAdmin = state.user && state.user.role === "Admin";
        $(".admin-only").toggleClass("d-none", !isAdmin);
    }

    function openCreateModal() {
        $("#employeeModalLabel").text("Add Employee");
        $("#employeeForm")[0].reset();
        $("#employeeId").val("");
        employeeModal.show();
    }

    async function openEditModal() {
        const employeeId = Number($(this).data("id"));
        try {
            const employee = await request(`/employees/${employeeId}`);
            $("#employeeModalLabel").text("Edit Employee");
            $("#employeeId").val(employee.id);
            $("#firstName").val(employee.firstName);
            $("#lastName").val(employee.lastName);
            $("#email").val(employee.email);
            $("#phone").val(employee.phone);
            $("#department").val(employee.department);
            $("#designation").val(employee.designation);
            $("#salary").val(employee.salary);
            $("#joinDate").val(employee.joinDate.split("T")[0]);
            $("#employeeStatus").val(employee.status);
            employeeModal.show();
        } catch (error) {
            showAlert(error.message, "danger");
        }
    }

    async function saveEmployee() {
        const employeeId = $("#employeeId").val();
        const payload = {
            id: employeeId ? Number(employeeId) : 0,
            firstName: $("#firstName").val().trim(),
            lastName: $("#lastName").val().trim(),
            email: $("#email").val().trim(),
            phone: $("#phone").val().trim(),
            department: $("#department").val().trim(),
            designation: $("#designation").val().trim(),
            salary: Number($("#salary").val()),
            joinDate: $("#joinDate").val(),
            status: $("#employeeStatus").val()
        };

        if (!payload.firstName || !payload.lastName || !payload.email) {
            showAlert("Please complete all required employee fields.", "warning");
            return;
        }

        try {
            if (employeeId) {
                await request(`/employees/${employeeId}`, "PUT", payload);
                showAlert("Employee updated successfully.", "success");
            } else {
                await request("/employees", "POST", payload);
                showAlert("Employee created successfully.", "success");
            }

            employeeModal.hide();
            await refreshWorkspace();
        } catch (error) {
            showAlert(error.message, "danger");
        }
    }

    async function deleteEmployee() {
        const employeeId = Number($(this).data("id"));
        const employee = state.employees.find(item => item.id === employeeId);
        const employeeName = employee ? `${employee.firstName} ${employee.lastName}` : "this employee";

        if (!window.confirm(`Delete ${employeeName}?`)) {
            return;
        }

        try {
            await request(`/employees/${employeeId}`, "DELETE");
            showAlert("Employee deleted successfully.", "success");
            if (state.pagination.page > 1 && state.employees.length === 1) {
                state.pagination.page -= 1;
            }
            await refreshWorkspace();
        } catch (error) {
            showAlert(error.message, "danger");
        }
    }

    async function request(path, method = "GET", body, useAuth = true) {
        const options = {
            url: `${state.apiBaseUrl}${path}`,
            method,
            headers: {},
            contentType: "application/json"
        };

        if (useAuth && state.token) {
            options.headers.Authorization = `Bearer ${state.token}`;
        }

        if (body !== undefined) {
            options.data = JSON.stringify(body);
        }

        try {
            return await $.ajax(options);
        } catch (xhr) {
            const message = xhr.responseJSON?.message || xhr.responseJSON?.title || "Request failed.";
            throw new Error(message);
        }
    }

    function showAlert(message, type) {
        const alert = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${escapeHtml(message)}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`;
        $("#alertHost").html(alert);
    }

    function formatCurrency(value) {
        return new Intl.NumberFormat("en-IN", {
            style: "currency",
            currency: "INR",
            maximumFractionDigits: 0
        }).format(value);
    }

    function formatDate(value) {
        return new Intl.DateTimeFormat("en-IN", {
            year: "numeric",
            month: "short",
            day: "2-digit"
        }).format(new Date(value));
    }

    function debounce(fn, wait) {
        let timeoutId;
        return function () {
            const context = this;
            const args = arguments;
            clearTimeout(timeoutId);
            timeoutId = setTimeout(function () {
                fn.apply(context, args);
            }, wait);
        };
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function escapeAttribute(value) {
        return escapeHtml(value);
    }
})();
