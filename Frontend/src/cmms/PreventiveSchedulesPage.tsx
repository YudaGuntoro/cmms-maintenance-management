"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import { ConfirmModal } from "@/components/ui/modal";
import { apiDelete, apiGet, apiPost, apiPut, buildQuery } from "./api";
import { Icon } from "./icons";
import { formatDateTime, formatNumber, nowDateTimeLocal, toDateTimeLocalInput } from "./format";
import { options, type SelectOption } from "./options";
import { Badge, Card, CardHeader, CheckboxInput, DataTableFooter, EntriesSelect, Feedback, InlineInput, InlineSearch, InlineSelect, SelectInput, TablePanel, TextareaInput, TextInput } from "./ui";
import type { Asset, PreventiveSchedule, WorkOrder } from "./types";

type ScheduleForm = {
  asset_id: string;
  schedule_name: string;
  frequency_type: string;
  frequency_value: string;
  next_due_date: string;
  estimated_duration_minutes: string;
  checklist_template: string;
  is_active: boolean;
};

const emptyForm: ScheduleForm = {
  asset_id: "",
  schedule_name: "",
  frequency_type: "MONTHLY",
  frequency_value: "1",
  next_due_date: nowDateTimeLocal(),
  estimated_duration_minutes: "",
  checklist_template: "",
  is_active: true,
};

const scheduleViewOptions: SelectOption[] = [{ label: "Overdue only", value: "overdue" }];

function activeFilterFromSearchParams(searchParams: Pick<URLSearchParams, "get">) {
  return searchParams.get("is_active") || "";
}

function viewFromSearchParams(searchParams: Pick<URLSearchParams, "get">) {
  return searchParams.get("view") === "overdue" ? "overdue" : "";
}

function isOverdueSchedule(schedule: PreventiveSchedule) {
  const due = new Date(schedule.next_due_date);

  if (Number.isNaN(due.getTime())) {
    return false;
  }

  const dueDate = new Date(due.getFullYear(), due.getMonth(), due.getDate());
  const now = new Date();
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());

  return dueDate < today;
}

function toForm(schedule?: PreventiveSchedule): ScheduleForm {
  if (!schedule) {
    return emptyForm;
  }

  return {
    asset_id: String(schedule.asset_id),
    schedule_name: schedule.schedule_name,
    frequency_type: schedule.frequency_type,
    frequency_value: String(schedule.frequency_value),
    next_due_date: toDateTimeLocalInput(schedule.next_due_date),
    estimated_duration_minutes: schedule.estimated_duration_minutes ? String(schedule.estimated_duration_minutes) : "",
    checklist_template: schedule.checklist_template || "",
    is_active: schedule.is_active,
  };
}

function payload(form: ScheduleForm) {
  return {
    asset_id: Number(form.asset_id),
    schedule_name: form.schedule_name,
    frequency_type: form.frequency_type,
    frequency_value: Number(form.frequency_value),
    next_due_date: form.next_due_date,
    estimated_duration_minutes: form.estimated_duration_minutes ? Number(form.estimated_duration_minutes) : null,
    checklist_template: form.checklist_template || null,
    is_active: form.is_active,
  };
}

export default function PreventiveSchedulesPage() {
  const searchParams = useSearchParams();
  const queryKey = searchParams.toString();
  const [schedules, setSchedules] = useState<PreventiveSchedule[]>([]);
  const [assets, setAssets] = useState<Asset[]>([]);
  const [activeFilter, setActiveFilter] = useState(() => activeFilterFromSearchParams(searchParams));
  const [dueView, setDueView] = useState(() => viewFromSearchParams(searchParams));
  const [search, setSearch] = useState("");
  const [form, setForm] = useState<ScheduleForm>(emptyForm);
  const [editing, setEditing] = useState<PreventiveSchedule | null>(null);
  const [scheduleToDelete, setScheduleToDelete] = useState<PreventiveSchedule | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [dueDate, setDueDate] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const assetOptions = useMemo<SelectOption[]>(() => assets.map((asset) => ({ label: `${asset.asset_code} - ${asset.asset_name}`, value: String(asset.id) })), [assets]);
  const assetName = useCallback((id: number) => assets.find((asset) => asset.id === id)?.asset_name || `Asset #${id}`, [assets]);

  useEffect(() => {
    const params = new URLSearchParams(queryKey);
    setActiveFilter(activeFilterFromSearchParams(params));
    setDueView(viewFromSearchParams(params));
  }, [queryKey]);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [scheduleData, assetData] = await Promise.all([
        apiGet<PreventiveSchedule[]>(`/api/preventive-schedules${buildQuery({ is_active: activeFilter })}`),
        apiGet<Asset[]>("/api/assets"),
      ]);
      setSchedules(scheduleData || []);
      setAssets(assetData || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load schedules.");
    } finally {
      setLoading(false);
    }
  }, [activeFilter]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const searchedSchedules = useMemo(() => {
    const viewFilteredSchedules = dueView === "overdue"
      ? schedules.filter(isOverdueSchedule)
      : schedules;
    const normalized = search.trim().toLowerCase();
    if (!normalized) {
      return viewFilteredSchedules;
    }

    return viewFilteredSchedules.filter((schedule) => [
      schedule.schedule_name,
      schedule.frequency_type,
      assetName(schedule.asset_id),
      schedule.checklist_template,
    ].some((value) => String(value || "").toLowerCase().includes(normalized)));
  }, [assetName, dueView, schedules, search]);

  useEffect(() => {
    setPage(1);
  }, [activeFilter, dueView, pageSize, search, searchedSchedules.length]);

  const visibleSchedules = useMemo(() => {
    const start = (page - 1) * pageSize;
    return searchedSchedules.slice(start, start + pageSize);
  }, [page, pageSize, searchedSchedules]);

  function openCreate() {
    setEditing(null);
    setForm({ ...emptyForm });
    setFormOpen(true);
  }

  function openEdit(schedule: PreventiveSchedule) {
    setEditing(schedule);
    setForm(toForm(schedule));
    setFormOpen(true);
  }

  async function saveSchedule(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      if (editing) {
        await apiPut<PreventiveSchedule>(`/api/preventive-schedules/${editing.id}`, payload(form));
        setSuccess("Schedule updated successfully.");
      } else {
        await apiPost<PreventiveSchedule>("/api/preventive-schedules", payload(form));
        setSuccess("Schedule created successfully.");
      }
      setFormOpen(false);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save schedule.");
    } finally {
      setSaving(false);
    }
  }

  async function generateDue() {
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      const generated = await apiPost<WorkOrder[]>(`/api/preventive-schedules/generate-due${buildQuery({ due_date: dueDate || undefined })}`);
      setSuccess(`${generated.length} preventive work orders generated.`);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to generate work orders.");
    } finally {
      setSaving(false);
    }
  }

  function closeDeleteModal() {
    if (deleting) {
      return;
    }

    setScheduleToDelete(null);
  }

  async function confirmDeleteSchedule() {
    if (!scheduleToDelete) {
      return;
    }

    setDeleting(true);
    setError(null);
    setSuccess(null);
    try {
      await apiDelete<string>(`/api/preventive-schedules/${scheduleToDelete.id}`);
      setSuccess("Schedule deleted successfully.");
      setScheduleToDelete(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete schedule.");
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Operations</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">Preventive Schedules</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Planned maintenance schedule and PM generation</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <button className="secondary-button" disabled={loading} onClick={() => void loadData()} type="button"><Icon name="refresh" />Refresh</button>
        </div>
      </div>

      <Feedback error={error} success={success} />

      {formOpen ? (
        <Card>
          <CardHeader action={<button className="icon-button" onClick={() => setFormOpen(false)} type="button"><Icon name="x" /></button>} description={editing ? "Update existing schedule" : "Create new schedule"} title={editing ? "Edit Preventive Schedule" : "Add Preventive Schedule"} />
          <form className="grid grid-cols-1 gap-4 p-5 md:grid-cols-2 xl:grid-cols-3" onSubmit={(event) => void saveSchedule(event)}>
            <SelectInput label="Asset" name="asset_id" onChange={(event) => setForm((current) => ({ ...current, asset_id: event.target.value }))} options={assetOptions} placeholder="Select Asset" required value={form.asset_id} />
            <TextInput label="Schedule Name" name="schedule_name" onChange={(event) => setForm((current) => ({ ...current, schedule_name: event.target.value }))} required value={form.schedule_name} />
            <SelectInput label="Frequency" name="frequency_type" onChange={(event) => setForm((current) => ({ ...current, frequency_type: event.target.value }))} options={options.frequencyType} required value={form.frequency_type} />
            <TextInput label="Frequency Value" name="frequency_value" onChange={(event) => setForm((current) => ({ ...current, frequency_value: event.target.value }))} required type="number" value={form.frequency_value} />
            <TextInput label="Next Due Date" name="next_due_date" onChange={(event) => setForm((current) => ({ ...current, next_due_date: event.target.value }))} required type="datetime-local" value={form.next_due_date} />
            <TextInput label="Duration Minutes" name="estimated_duration_minutes" onChange={(event) => setForm((current) => ({ ...current, estimated_duration_minutes: event.target.value }))} type="number" value={form.estimated_duration_minutes} />
            <div className="md:col-span-2 xl:col-span-3"><TextareaInput label="Checklist Template" name="checklist_template" onChange={(event) => setForm((current) => ({ ...current, checklist_template: event.target.value }))} value={form.checklist_template} /></div>
            <CheckboxInput checked={form.is_active} label="Active" name="is_active" onChange={(event) => setForm((current) => ({ ...current, is_active: event.target.checked }))} />
            <div className="flex flex-wrap items-center gap-2 md:col-span-2 xl:col-span-3"><button className="primary-button" disabled={saving} type="submit"><Icon name="check" />Save</button><button className="secondary-button" onClick={() => setFormOpen(false)} type="button">Cancel</button></div>
          </form>
        </Card>
      ) : null}

      <TablePanel
        footer={<DataTableFooter onPageChange={setPage} page={page} pageSize={pageSize} total={searchedSchedules.length} />}
        toolbarLeft={(
          <>
            <EntriesSelect onChange={(value) => { setPageSize(value); setPage(1); }} value={pageSize} />
            <InlineSelect onChange={setActiveFilter} options={options.boolean} placeholder="All Status" value={activeFilter} />
            <InlineSelect onChange={setDueView} options={scheduleViewOptions} placeholder="All Due" value={dueView} />
            <InlineInput label="Due" onChange={setDueDate} type="date" value={dueDate} />
            <button className="primary-button" disabled={saving} onClick={() => void generateDue()} type="button"><Icon name="calendar" />Generate</button>
            <button className="primary-button" onClick={openCreate} type="button"><Icon name="plus" />Create</button>
          </>
        )}
        toolbarRight={<InlineSearch onChange={setSearch} placeholder="Search schedules" value={search} />}
      >
          <table className="min-w-full">
            <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
              <tr>
                <th className="min-w-64 px-5 py-3 text-left text-theme-xs font-semibold">Schedule</th>
                <th className="min-w-64 px-5 py-3 text-left text-theme-xs font-semibold">Asset</th>
                <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Frequency</th>
                <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Next Due</th>
                <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Duration</th>
                <th className="min-w-28 px-5 py-3 text-left text-theme-xs font-semibold">Active</th>
                <th className="min-w-28 px-5 py-3 text-center text-theme-xs font-semibold">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
              {loading ? (
                Array.from({ length: 5 }).map((_, index) => <tr key={index}>{Array.from({ length: 7 }).map((__, cellIndex) => <td className="px-5 py-4" key={cellIndex}><div className="h-4 w-full animate-pulse rounded-md bg-gray-100 dark:bg-white/[0.05]" /></td>)}</tr>)
              ) : searchedSchedules.length ? (
                visibleSchedules.map((schedule) => (
                  <tr className="hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={schedule.id}>
                    <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{schedule.schedule_name}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{assetName(schedule.asset_id)}</td>
                    <td className="px-5 py-4"><Badge value={schedule.frequency_type} /></td>
                    <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">{formatDateTime(schedule.next_due_date)}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{formatNumber(schedule.estimated_duration_minutes)}</td>
                    <td className="px-5 py-4"><Badge value={schedule.is_active ? "ACTIVE" : "INACTIVE"} /></td>
                    <td className="px-5 py-4"><div className="flex justify-center gap-3"><button aria-label="Edit" className="text-warning-500 transition-colors hover:text-warning-600 dark:text-warning-400 dark:hover:text-warning-500" onClick={() => openEdit(schedule)} type="button"><Icon className="h-5 w-5" name="edit" /></button><button aria-label="Delete" className="text-error-500 transition-colors hover:text-error-600 dark:text-error-400 dark:hover:text-error-500" onClick={() => setScheduleToDelete(schedule)} type="button"><Icon className="h-5 w-5" name="trash" /></button></div></td>
                  </tr>
                ))
              ) : (
                <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={7}>No schedules found.</td></tr>
              )}
            </tbody>
          </table>
      </TablePanel>

      <ConfirmModal
        confirmText="Delete"
        isDestructive
        isLoading={deleting}
        isOpen={Boolean(scheduleToDelete)}
        message={`Preventive schedule "${scheduleToDelete?.schedule_name || ""}" akan dihapus permanen.`}
        onClose={closeDeleteModal}
        onConfirm={() => void confirmDeleteSchedule()}
        title="Delete Preventive Schedule"
      />
    </div>
  );
}
