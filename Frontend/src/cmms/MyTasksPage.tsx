"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { Modal } from "@/components/ui/modal";
import { apiGet, apiPostForm, apiPut } from "./api";
import { getStoredUser } from "./auth";
import { Icon } from "./icons";
import { formatDateTime, nowDateTimeLocal, toDateTimeLocalInput } from "./format";
import { options } from "./options";
import { Badge, DataTableFooter, EntriesSelect, Feedback, InlineSearch, InlineSelect, SelectInput, TablePanel, TextareaInput, TextInput } from "./ui";
import type { Asset, FailureCode, RootCause, Technician, UserResponse, WorkOrder, WorkOrderPhoto } from "./types";
import { WorkOrderDetailModal } from "./WorkOrderDetailModal";

type UserWithTechnician = UserResponse & {
  technician_id?: number | null;
};

type TaskActionForm = {
  status: "PENDING" | "COMPLETED";
  failure_code: string;
  root_cause: string;
  action_taken: string;
  result: string;
  downtime_end: string;
};

type TaskQuickFilter = "total" | "active" | "urgent" | "completed" | "";

const emptyActionForm: TaskActionForm = {
  status: "PENDING",
  failure_code: "",
  root_cause: "",
  action_taken: "",
  result: "",
  downtime_end: "",
};

const userTaskStatusOptions = [
  { label: "Pending", value: "PENDING" },
  { label: "Completed", value: "COMPLETED" },
];
const activeTaskStatuses = ["OPEN", "ASSIGNED", "IN_PROGRESS", "PENDING"];
const completedTaskStatuses = ["COMPLETED", "CLOSED"];

function normalize(value?: string | null) {
  return String(value || "").trim().toLowerCase();
}

function dateValue(value?: string | null) {
  if (!value) {
    return Number.MAX_SAFE_INTEGER;
  }

  const parsed = Date.parse(value);
  return Number.isFinite(parsed) ? parsed : Number.MAX_SAFE_INTEGER;
}

function findCurrentTechnician(user: UserWithTechnician | null, technicians: Technician[]) {
  if (!user) {
    return null;
  }

  if (user.technician_id) {
    const byId = technicians.find((technician) => technician.id === user.technician_id);
    if (byId) {
      return byId;
    }
  }

  const email = normalize(user.email);
  const fullName = normalize(user.full_name);
  const username = normalize(user.username);

  return technicians.find((technician) => {
    const techEmail = normalize(technician.email);
    const techName = normalize(technician.name);
    const employeeNo = normalize(technician.employee_no);
    return (email && techEmail === email) || (fullName && techName === fullName) || (username && (employeeNo === username || techEmail === username || techName === username));
  }) || null;
}

function isPrivilegedRole(user: UserWithTechnician | null) {
  return user?.role === "ADMIN" || user?.role === "SUPERVISOR";
}

function shouldShowDowntimeEnd(task: WorkOrder | null, status: TaskActionForm["status"]) {
  return Boolean(task?.downtime_start && status === "COMPLETED");
}

export default function MyTasksPage() {
  const [user, setUser] = useState<UserWithTechnician | null>(null);
  const [workOrders, setWorkOrders] = useState<WorkOrder[]>([]);
  const [assets, setAssets] = useState<Asset[]>([]);
  const [technicians, setTechnicians] = useState<Technician[]>([]);
  const [failureCodes, setFailureCodes] = useState<FailureCode[]>([]);
  const [rootCauses, setRootCauses] = useState<RootCause[]>([]);
  const [quickFilter, setQuickFilter] = useState<TaskQuickFilter>("");
  const [statusFilter, setStatusFilter] = useState("");
  const [priorityFilter, setPriorityFilter] = useState("");
  const [search, setSearch] = useState("");
  const [selectedTask, setSelectedTask] = useState<WorkOrder | null>(null);
  const [detailTask, setDetailTask] = useState<WorkOrder | null>(null);
  const [actionForm, setActionForm] = useState<TaskActionForm>(emptyActionForm);
  const [taskPhotoFile, setTaskPhotoFile] = useState<File | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [workOrderData, assetData, technicianData, failureData, rootCauseData] = await Promise.all([
        apiGet<WorkOrder[]>("/api/work-orders"),
        apiGet<Asset[]>("/api/assets"),
        apiGet<Technician[]>("/api/technicians?status=ACTIVE"),
        apiGet<FailureCode[]>("/api/failure-codes"),
        apiGet<RootCause[]>("/api/root-causes"),
      ]);
      setUser(getStoredUser() as UserWithTechnician | null);
      setWorkOrders(workOrderData || []);
      setAssets(assetData || []);
      setTechnicians(technicianData || []);
      setFailureCodes(failureData || []);
      setRootCauses(rootCauseData || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load my tasks.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const currentTechnician = useMemo(() => findCurrentTechnician(user, technicians), [technicians, user]);
  const failureOptions = useMemo(() => failureCodes.map((item) => ({ label: `${item.code} - ${item.name}`, value: item.code })), [failureCodes]);
  const rootCauseOptions = useMemo(() => rootCauses.map((item) => ({ label: `${item.code} - ${item.name}`, value: item.code })), [rootCauses]);

  const assetName = useCallback((id: number) => {
    const asset = assets.find((item) => item.id === id);
    return asset ? `${asset.asset_code} - ${asset.asset_name}` : `Asset #${id}`;
  }, [assets]);

  const technicianName = useCallback((id?: number | null) => {
    if (!id) {
      return "-";
    }

    return technicians.find((item) => item.id === id)?.name || `Technician #${id}`;
  }, [technicians]);

  const taskOwnerLabel = currentTechnician?.name || (isPrivilegedRole(user) ? "Team assigned queue" : user?.full_name || "Current user");

  const scopedTasks = useMemo(() => {
    if (currentTechnician) {
      return workOrders.filter((workOrder) => workOrder.assigned_to === currentTechnician.id);
    }

    if (isPrivilegedRole(user)) {
      return workOrders.filter((workOrder) => Boolean(workOrder.assigned_to));
    }

    return [];
  }, [currentTechnician, user, workOrders]);

  const filteredTasks = useMemo(() => {
    const normalized = search.trim().toLowerCase();
    return scopedTasks
      .filter((workOrder) => {
        if (quickFilter === "active") {
          return activeTaskStatuses.includes(workOrder.status);
        }

        if (quickFilter === "urgent") {
          return activeTaskStatuses.includes(workOrder.status) && workOrder.priority === "URGENT";
        }

        if (quickFilter === "completed") {
          return completedTaskStatuses.includes(workOrder.status);
        }

        return true;
      })
      .filter((workOrder) => !statusFilter || workOrder.status === statusFilter)
      .filter((workOrder) => !priorityFilter || workOrder.priority === priorityFilter)
      .filter((workOrder) => {
        if (!normalized) {
          return true;
        }

        return [
          workOrder.wo_number,
          workOrder.title,
          workOrder.status,
          workOrder.priority,
          workOrder.maintenance_type,
          workOrder.action_taken,
          workOrder.result,
          assetName(workOrder.asset_id),
        ].some((value) => String(value || "").toLowerCase().includes(normalized));
      })
      .sort((left, right) => dateValue(left.reported_at || left.created_at) - dateValue(right.reported_at || right.created_at));
  }, [assetName, priorityFilter, quickFilter, scopedTasks, search, statusFilter]);

  useEffect(() => {
    setPage(1);
  }, [filteredTasks.length, pageSize, priorityFilter, quickFilter, search, statusFilter]);

  const visibleTasks = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredTasks.slice(start, start + pageSize);
  }, [filteredTasks, page, pageSize]);

  const taskStats = useMemo(() => ({
    total: scopedTasks.length,
    urgent: scopedTasks.filter((task) => activeTaskStatuses.includes(task.status) && task.priority === "URGENT").length,
    active: scopedTasks.filter((task) => activeTaskStatuses.includes(task.status)).length,
    completed: scopedTasks.filter((task) => completedTaskStatuses.includes(task.status)).length,
  }), [scopedTasks]);

  function applyQuickFilter(value: TaskQuickFilter) {
    setQuickFilter(value);
    setStatusFilter("");
    setPriorityFilter("");
    setSearch("");
    setPage(1);
  }

  function openActionModal(task: WorkOrder) {
    setSelectedTask(task);
    setActionForm({
      status: task.status === "COMPLETED" ? "COMPLETED" : "PENDING",
      failure_code: task.failure_code || "",
      root_cause: task.root_cause || "",
      action_taken: task.action_taken || "",
      result: task.result || "",
      downtime_end: task.downtime_start ? toDateTimeLocalInput(task.downtime_end || new Date().toISOString()) : "",
    });
    setTaskPhotoFile(null);
    setError(null);
    setSuccess(null);
  }

  function closeActionModal(force = false) {
    if (saving && !force) {
      return;
    }

    setSelectedTask(null);
    setActionForm(emptyActionForm);
    setTaskPhotoFile(null);
  }

  async function submitAction(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedTask) {
      return;
    }

    if (!actionForm.action_taken.trim()) {
      setError("Action Taken wajib diisi.");
      return;
    }

    if (shouldShowDowntimeEnd(selectedTask, actionForm.status) && !actionForm.downtime_end) {
      setError("Downtime End wajib diisi saat downtime task diselesaikan.");
      return;
    }

    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      const now = new Date().toISOString();
      const isCompleted = actionForm.status === "COMPLETED";
      const downtimeEnd = shouldShowDowntimeEnd(selectedTask, actionForm.status)
        ? actionForm.downtime_end || now
        : selectedTask.downtime_end || null;
      await apiPut<WorkOrder>(`/api/work-orders/${selectedTask.id}`, {
        ...selectedTask,
        status: actionForm.status,
        completed_at: isCompleted ? selectedTask.completed_at || now : null,
        closed_at: null,
        downtime_end: downtimeEnd,
        repair_start: isCompleted ? selectedTask.repair_start || selectedTask.started_at || now : selectedTask.repair_start,
        repair_end: isCompleted ? selectedTask.repair_end || now : selectedTask.repair_end,
        failure_code: actionForm.failure_code || null,
        root_cause: actionForm.root_cause || null,
        action_taken: actionForm.action_taken.trim(),
        result: actionForm.result.trim() || null,
      });

      if (taskPhotoFile) {
        const formData = new FormData();
        formData.append("file", taskPhotoFile);
        await apiPostForm<WorkOrderPhoto>(`/api/work-orders/${selectedTask.id}/photos`, formData);
      }

      closeActionModal(true);
      setSuccess("Task updated successfully.");
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update task.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Personal Queue</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">My Task</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Assigned work orders for {taskOwnerLabel}, oldest reported first.</p>
        </div>
        <button className="secondary-button" disabled={loading} onClick={() => void loadData()} type="button">
          <Icon name="refresh" />
          Refresh
        </button>
      </div>

      <Feedback error={error} success={success} />

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
        <TaskStatCard active={quickFilter === "total" || (!quickFilter && !statusFilter && !priorityFilter && !search)} label="Total Tasks" onClick={() => applyQuickFilter("total")} value={taskStats.total} />
        <TaskStatCard active={quickFilter === "active"} label="Active" onClick={() => applyQuickFilter("active")} value={taskStats.active} />
        <TaskStatCard active={quickFilter === "urgent"} label="Urgent" onClick={() => applyQuickFilter("urgent")} value={taskStats.urgent} />
        <TaskStatCard active={quickFilter === "completed"} label="Complete" onClick={() => applyQuickFilter("completed")} value={taskStats.completed} />
      </div>

      <TablePanel
        footer={<DataTableFooter onPageChange={setPage} page={page} pageSize={pageSize} total={filteredTasks.length} />}
        toolbarLeft={(
          <>
            <EntriesSelect onChange={(value) => { setPageSize(value); setPage(1); }} value={pageSize} />
            <InlineSelect onChange={(value) => { setQuickFilter(""); setStatusFilter(value); }} options={options.workOrderStatus} placeholder="All Status" value={statusFilter} />
            <InlineSelect onChange={(value) => { setQuickFilter(""); setPriorityFilter(value); }} options={options.workOrderPriority} placeholder="All Priority" value={priorityFilter} />
          </>
        )}
        toolbarRight={<InlineSearch onChange={(value) => { setQuickFilter(""); setSearch(value); }} placeholder="Search my tasks" value={search} />}
      >
        <table className="min-w-full">
          <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
            <tr>
              <th className="min-w-20 px-5 py-3 text-left text-theme-xs font-semibold">No</th>
              <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">WO Number</th>
              <th className="min-w-64 px-5 py-3 text-left text-theme-xs font-semibold">Title</th>
              <th className="min-w-60 px-5 py-3 text-left text-theme-xs font-semibold">Asset</th>
              <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Priority</th>
              <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Status</th>
              <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">Type</th>
              <th className="min-w-72 px-5 py-3 text-left text-theme-xs font-semibold">Action Taken</th>
              <th className="min-w-72 px-5 py-3 text-left text-theme-xs font-semibold">Result</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Reported</th>
              <th className="min-w-32 px-5 py-3 text-center text-theme-xs font-semibold">Action</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
            {loading ? (
              Array.from({ length: 5 }).map((_, rowIndex) => (
                <tr key={rowIndex}>
                  {Array.from({ length: 11 }).map((__, cellIndex) => <td className="px-5 py-4" key={cellIndex}><div className="h-4 w-full animate-pulse rounded-md bg-gray-100 dark:bg-white/[0.05]" /></td>)}
                </tr>
              ))
            ) : visibleTasks.length ? (
              visibleTasks.map((workOrder, index) => (
                <tr className="cursor-pointer hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={workOrder.id} onClick={() => setDetailTask(workOrder)}>
                  <td className="px-5 py-4 text-theme-sm font-semibold text-gray-500 dark:text-gray-400">{(page - 1) * pageSize + index + 1}</td>
                  <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{workOrder.wo_number}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{workOrder.title}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{assetName(workOrder.asset_id)}</td>
                  <td className="px-5 py-4"><Badge value={workOrder.priority} /></td>
                  <td className="px-5 py-4"><Badge value={workOrder.status} /></td>
                  <td className="px-5 py-4"><Badge value={workOrder.maintenance_type} /></td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300"><span className="block max-w-[280px] truncate">{workOrder.action_taken || "-"}</span></td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300"><span className="block max-w-[280px] truncate">{workOrder.result || "-"}</span></td>
                  <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">{formatDateTime(workOrder.reported_at || workOrder.created_at)}</td>
                  <td className="px-5 py-4">
                    <div className="flex justify-center">
                      {!["CLOSED", "CANCELLED"].includes(workOrder.status) ? (
                        <button
                          aria-label="Update task"
                          className="text-warning-500 transition-colors hover:text-warning-600 disabled:cursor-not-allowed disabled:opacity-50 dark:text-warning-400 dark:hover:text-warning-500"
                          disabled={saving}
                          onClick={(event) => { event.stopPropagation(); openActionModal(workOrder); }}
                          title="Update task"
                          type="button"
                        >
                          <Icon className="h-5 w-5" name="edit" />
                        </button>
                      ) : (
                        <span className="text-theme-sm text-gray-400 dark:text-gray-600">-</span>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            ) : (
              <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={11}>No tasks found.</td></tr>
            )}
          </tbody>
        </table>
      </TablePanel>

      <Modal isOpen={Boolean(selectedTask)} onClose={closeActionModal} className="mx-4 max-w-[680px] overflow-hidden p-0" showCloseButton={false}>
        <div className="border-b border-gray-100 px-6 py-5 dark:border-white/[0.05]">
          <div className="flex items-start gap-4">
            <div className="flex size-12 shrink-0 items-center justify-center rounded-lg bg-brand-500/10 text-brand-500 ring-1 ring-brand-500/20">
              <Icon className="size-6" name="edit" />
            </div>
            <div className="min-w-0">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white/90">Update Task</h3>
              <p className="mt-1 text-sm leading-6 text-gray-500 dark:text-gray-400">Isi tindakan yang dilakukan dan pilih status baru. User hanya dapat mengubah status menjadi Pending atau Completed.</p>
              {selectedTask ? (
                <div className="mt-3 rounded-lg border border-gray-100 bg-gray-50 px-3 py-2 dark:border-white/[0.05] dark:bg-white/[0.03]">
                  <p className="text-sm font-semibold text-gray-900 dark:text-white">{selectedTask.wo_number}</p>
                  <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{selectedTask.title}</p>
                </div>
              ) : null}
            </div>
          </div>
        </div>

        <form onSubmit={(event) => void submitAction(event)}>
          <div className="grid grid-cols-1 gap-4 px-6 py-5 md:grid-cols-2">
            <SelectInput
              label="Status"
              name="status"
              onChange={(event) => setActionForm((current) => ({ ...current, status: event.target.value as TaskActionForm["status"] }))}
              options={userTaskStatusOptions}
              required
              value={actionForm.status}
            />
            {shouldShowDowntimeEnd(selectedTask, actionForm.status) ? (
              <TextInput
                helper={`Downtime start: ${formatDateTime(selectedTask?.downtime_start)}. Isi jam saat mesin/problem sudah selesai ditangani.`}
                label="Downtime End"
                name="downtime_end"
                onChange={(event) => setActionForm((current) => ({ ...current, downtime_end: event.target.value }))}
                required
                type="datetime-local"
                value={actionForm.downtime_end || nowDateTimeLocal()}
              />
            ) : null}
            <SelectInput
              label="Failure Code"
              name="failure_code"
              onChange={(event) => setActionForm((current) => ({ ...current, failure_code: event.target.value }))}
              options={failureOptions}
              value={actionForm.failure_code}
            />
            <SelectInput
              label="Root Cause"
              name="root_cause"
              onChange={(event) => setActionForm((current) => ({ ...current, root_cause: event.target.value }))}
              options={rootCauseOptions}
              value={actionForm.root_cause}
            />
            <div className="md:col-span-2">
              <TextareaInput
                label="Action Taken"
                name="action_taken"
                onChange={(event) => setActionForm((current) => ({ ...current, action_taken: event.target.value }))}
                required
                value={actionForm.action_taken}
              />
            </div>
            <div className="md:col-span-2">
              <TextareaInput
                label="Result"
                name="result"
                onChange={(event) => setActionForm((current) => ({ ...current, result: event.target.value }))}
                value={actionForm.result}
              />
            </div>
            <div className="md:col-span-2">
              <label className="block">
                <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Field Photo</span>
                <span className="flex cursor-pointer flex-col items-center justify-center rounded-lg border border-dashed border-gray-300 bg-gray-50 px-5 py-5 text-center transition-colors hover:border-brand-400 hover:bg-brand-50/60 dark:border-gray-700 dark:bg-white/[0.03] dark:hover:border-brand-500/70 dark:hover:bg-brand-500/10">
                  <span className="flex size-10 items-center justify-center rounded-lg bg-white text-brand-500 shadow-theme-sm ring-1 ring-gray-200 dark:bg-gray-900 dark:ring-white/[0.08]">
                    <Icon name="upload" />
                  </span>
                  <span className="mt-3 text-sm font-semibold text-gray-900 dark:text-white">{taskPhotoFile ? taskPhotoFile.name : "Upload condition photo"}</span>
                  <span className="mt-1 text-xs text-gray-500 dark:text-gray-400">JPG, PNG, or WEBP. Max 10 MB.</span>
                  <input
                    accept="image/jpeg,image/png,image/webp"
                    className="sr-only"
                    onChange={(event) => setTaskPhotoFile(event.target.files?.[0] || null)}
                    type="file"
                  />
                </span>
              </label>
              {taskPhotoFile ? (
                <button className="mt-2 text-sm font-semibold text-error-500 hover:text-error-600" onClick={() => setTaskPhotoFile(null)} type="button">
                  Remove selected photo
                </button>
              ) : null}
            </div>
          </div>

          <div className="flex flex-col-reverse gap-3 border-t border-gray-100 px-6 py-4 dark:border-white/[0.05] sm:flex-row sm:justify-end">
            <button className="secondary-button h-11" disabled={saving} onClick={() => closeActionModal()} type="button">
              Cancel
            </button>
            <button className="primary-button h-11 min-w-32" disabled={saving} type="submit">
              {saving ? "Processing..." : "Save Update"}
            </button>
          </div>
        </form>
      </Modal>

      <WorkOrderDetailModal
        assetLabel={detailTask ? assetName(detailTask.asset_id) : undefined}
        isOpen={Boolean(detailTask)}
        onClose={() => setDetailTask(null)}
        technicianLabel={technicianName(detailTask?.assigned_to)}
        workOrder={detailTask}
      />
    </div>
  );
}

function TaskStatCard({
  active,
  label,
  onClick,
  value,
}: {
  active: boolean;
  label: string;
  onClick: () => void;
  value: number;
}) {
  return (
    <button
      className={`group rounded-lg border p-5 text-left shadow-theme-sm transition-all hover:-translate-y-0.5 hover:border-brand-500/50 hover:bg-brand-50/40 focus:outline-none focus:ring-3 focus:ring-brand-500/20 dark:hover:bg-brand-500/10 ${
        active
          ? "border-brand-500/60 bg-brand-50/60 dark:border-brand-500/60 dark:bg-brand-500/10"
          : "border-gray-200 bg-white dark:border-gray-800 dark:bg-gray-900"
      }`}
      onClick={onClick}
      type="button"
    >
      <span className={`text-sm font-medium ${active ? "text-brand-600 dark:text-brand-400" : "text-gray-500 dark:text-gray-400"}`}>{label}</span>
      <span className="mt-3 block text-3xl font-semibold text-gray-900 dark:text-white">{value}</span>
    </button>
  );
}
