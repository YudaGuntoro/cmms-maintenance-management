"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { ConfirmModal } from "@/components/ui/modal";
import { apiDelete, apiGet, apiPost, apiPut, buildQuery } from "./api";
import { formatDateTime, formatNumber, nowDateTimeLocal, toDateTimeLocalInput } from "./format";
import { Icon } from "./icons";
import { options, type SelectOption } from "./options";
import { Badge, Card, CardHeader, DataTableFooter, EntriesSelect, Feedback, InlineSearch, InlineSelect, SelectInput, TablePanel, TextareaInput, TextInput } from "./ui";
import type { Asset, DowntimeLog, ProblemReport, WorkOrder } from "./types";

type DowntimeForm = {
  asset_id: string;
  work_order_id: string;
  downtime_category: string;
  start_time: string;
  end_time: string;
  description: string;
};

type DowntimeFilters = {
  asset_id: string;
  work_order_id: string;
};

const emptyForm: DowntimeForm = {
  asset_id: "",
  work_order_id: "",
  downtime_category: "OTHER",
  start_time: "",
  end_time: "",
  description: "",
};

function toForm(record?: DowntimeLog): DowntimeForm {
  if (!record) {
    return emptyForm;
  }

  return {
    asset_id: String(record.asset_id || ""),
    work_order_id: record.work_order_id ? String(record.work_order_id) : "",
    downtime_category: record.downtime_category || "OTHER",
    start_time: toDateTimeLocalInput(record.start_time),
    end_time: toDateTimeLocalInput(record.end_time),
    description: record.description || "",
  };
}

function payload(form: DowntimeForm) {
  return {
    asset_id: Number(form.asset_id),
    work_order_id: form.work_order_id ? Number(form.work_order_id) : null,
    downtime_category: form.downtime_category,
    start_time: form.start_time || null,
    end_time: form.end_time || null,
    description: form.description || null,
  };
}

export default function DowntimeLogsPage() {
  const [records, setRecords] = useState<DowntimeLog[]>([]);
  const [assets, setAssets] = useState<Asset[]>([]);
  const [workOrders, setWorkOrders] = useState<WorkOrder[]>([]);
  const [problemReports, setProblemReports] = useState<ProblemReport[]>([]);
  const [filters, setFilters] = useState<DowntimeFilters>({ asset_id: "", work_order_id: "" });
  const [search, setSearch] = useState("");
  const [form, setForm] = useState<DowntimeForm>(emptyForm);
  const [editing, setEditing] = useState<DowntimeLog | null>(null);
  const [recordToDelete, setRecordToDelete] = useState<DowntimeLog | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const assetOptions = useMemo<SelectOption[]>(() => assets.map((asset) => ({
    label: `${asset.asset_code} - ${asset.asset_name}`,
    value: String(asset.id),
  })), [assets]);

  const workOrderOptions = useMemo<SelectOption[]>(() => workOrders.map((workOrder) => ({
    label: `${workOrder.wo_number} - ${workOrder.title}`,
    value: String(workOrder.id),
  })), [workOrders]);

  const formWorkOrderOptions = useMemo(() => {
    if (!form.asset_id) {
      return workOrderOptions;
    }

    return workOrders
      .filter((workOrder) => String(workOrder.asset_id) === form.asset_id)
      .map((workOrder) => ({ label: `${workOrder.wo_number} - ${workOrder.title}`, value: String(workOrder.id) }));
  }, [form.asset_id, workOrderOptions, workOrders]);

  const assetName = useCallback((id: number) => {
    const asset = assets.find((item) => item.id === id);
    return asset ? `${asset.asset_code} - ${asset.asset_name}` : `Asset #${id}`;
  }, [assets]);

  const workOrderName = useCallback((id?: number | null) => {
    if (!id) {
      return "-";
    }

    const workOrder = workOrders.find((item) => item.id === id);
    return workOrder ? `${workOrder.wo_number} - ${workOrder.title}` : `WO #${id}`;
  }, [workOrders]);

  const problemReportName = useCallback((id?: number | null) => {
    if (!id) {
      return "-";
    }

    const report = problemReports.find((item) => item.id === id);
    return report ? `${report.report_number} - ${report.title}` : `Report #${id}`;
  }, [problemReports]);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [downtimeData, assetData, workOrderData, reportData] = await Promise.all([
        apiGet<DowntimeLog[]>(`/api/downtime-logs${buildQuery(filters)}`),
        apiGet<Asset[]>("/api/assets"),
        apiGet<WorkOrder[]>("/api/work-orders"),
        apiGet<ProblemReport[]>("/api/problem-reports"),
      ]);
      setRecords(downtimeData || []);
      setAssets(assetData || []);
      setWorkOrders(workOrderData || []);
      setProblemReports(reportData || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load downtime logs.");
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const filteredRecords = useMemo(() => {
    const normalized = search.trim().toLowerCase();
    if (!normalized) {
      return records;
    }

    return records.filter((record) => [
      assetName(record.asset_id),
      workOrderName(record.work_order_id),
      problemReportName(record.problem_report_id),
      record.downtime_category,
      record.description,
    ].some((value) => String(value || "").toLowerCase().includes(normalized)));
  }, [assetName, problemReportName, records, search, workOrderName]);

  useEffect(() => {
    setPage(1);
  }, [filteredRecords.length, filters, pageSize, search]);

  const visibleRecords = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredRecords.slice(start, start + pageSize);
  }, [filteredRecords, page, pageSize]);

  function openCreate() {
    setEditing(null);
    setForm({ ...emptyForm, start_time: nowDateTimeLocal() });
    setFormOpen(true);
    setError(null);
    setSuccess(null);
  }

  function openEdit(record: DowntimeLog) {
    setEditing(record);
    setForm(toForm(record));
    setFormOpen(true);
    setError(null);
    setSuccess(null);
  }

  async function saveRecord(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      if (editing) {
        await apiPut<DowntimeLog>(`/api/downtime-logs/${editing.id}`, payload(form));
        setSuccess("Downtime log updated successfully.");
      } else {
        await apiPost<DowntimeLog>("/api/downtime-logs", payload(form));
        setSuccess("Downtime log created successfully.");
      }

      setFormOpen(false);
      setEditing(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save downtime log.");
    } finally {
      setSaving(false);
    }
  }

  function closeDeleteModal() {
    if (deleting) {
      return;
    }

    setRecordToDelete(null);
  }

  async function confirmDeleteRecord() {
    if (!recordToDelete) {
      return;
    }

    setDeleting(true);
    setError(null);
    setSuccess(null);
    try {
      await apiDelete<string>(`/api/downtime-logs/${recordToDelete.id}`);
      setSuccess("Downtime log deleted successfully.");
      setRecordToDelete(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete downtime log.");
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Operations</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">Downtime Logs</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Asset downtime history</p>
        </div>
        <button className="secondary-button" disabled={loading} onClick={() => void loadData()} type="button">
          <Icon name="refresh" />
          Refresh
        </button>
      </div>

      <Feedback error={error} success={success} />

      {formOpen ? (
        <Card>
          <CardHeader action={<button className="icon-button" onClick={() => setFormOpen(false)} type="button"><Icon name="x" /></button>} description={editing ? "Update existing downtime record" : "Create downtime record"} title={editing ? "Edit Downtime Log" : "Add Downtime Log"} />
          <form className="grid grid-cols-1 gap-4 p-5 md:grid-cols-2 xl:grid-cols-3" onSubmit={(event) => void saveRecord(event)}>
            <SelectInput label="Asset" name="asset_id" onChange={(event) => setForm((current) => ({ ...current, asset_id: event.target.value, work_order_id: "" }))} options={assetOptions} placeholder="Select Asset" required value={form.asset_id} />
            <SelectInput label="Work Order" name="work_order_id" onChange={(event) => setForm((current) => ({ ...current, work_order_id: event.target.value }))} options={formWorkOrderOptions} value={form.work_order_id} />
            <SelectInput label="Category" name="downtime_category" onChange={(event) => setForm((current) => ({ ...current, downtime_category: event.target.value }))} options={options.downtimeCategory} required value={form.downtime_category} />
            <TextInput label="Start Time" name="start_time" onChange={(event) => setForm((current) => ({ ...current, start_time: event.target.value }))} required type="datetime-local" value={form.start_time} />
            <TextInput label="End Time" name="end_time" onChange={(event) => setForm((current) => ({ ...current, end_time: event.target.value }))} type="datetime-local" value={form.end_time} />
            <div className="md:col-span-2 xl:col-span-3"><TextareaInput label="Description" name="description" onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} value={form.description} /></div>
            <div className="flex flex-wrap items-center gap-2 md:col-span-2 xl:col-span-3">
              <button className="primary-button" disabled={saving} type="submit"><Icon name="check" />Save</button>
              <button className="secondary-button" onClick={() => setFormOpen(false)} type="button">Cancel</button>
            </div>
          </form>
        </Card>
      ) : null}

      <TablePanel
        footer={<DataTableFooter onPageChange={setPage} page={page} pageSize={pageSize} total={filteredRecords.length} />}
        toolbarLeft={(
          <>
            <EntriesSelect onChange={(value) => { setPageSize(value); setPage(1); }} value={pageSize} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, asset_id: value }))} options={assetOptions} placeholder="All Assets" value={filters.asset_id} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, work_order_id: value }))} options={workOrderOptions} placeholder="All Work Orders" value={filters.work_order_id} />
            <button className="primary-button" onClick={openCreate} type="button"><Icon name="plus" />Create</button>
          </>
        )}
        toolbarRight={<InlineSearch onChange={setSearch} placeholder="Search asset or work order" value={search} />}
      >
        <table className="min-w-full">
          <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
            <tr>
              <th className="min-w-64 px-5 py-3 text-left text-theme-xs font-semibold">Asset</th>
              <th className="min-w-72 px-5 py-3 text-left text-theme-xs font-semibold">Work Order</th>
              <th className="min-w-72 px-5 py-3 text-left text-theme-xs font-semibold">Problem Report</th>
              <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">Category</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Start</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">End</th>
              <th className="min-w-28 px-5 py-3 text-left text-theme-xs font-semibold">Minutes</th>
              <th className="min-w-64 px-5 py-3 text-left text-theme-xs font-semibold">Description</th>
              <th className="min-w-28 px-5 py-3 text-center text-theme-xs font-semibold">Action</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
            {loading ? (
              Array.from({ length: 5 }).map((_, rowIndex) => (
                <tr key={rowIndex}>
                  {Array.from({ length: 9 }).map((__, cellIndex) => <td className="px-5 py-4" key={cellIndex}><div className="h-4 w-full animate-pulse rounded-md bg-gray-100 dark:bg-white/[0.05]" /></td>)}
                </tr>
              ))
            ) : visibleRecords.length ? (
              visibleRecords.map((record) => (
                <tr className="hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={record.id}>
                  <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{assetName(record.asset_id)}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{workOrderName(record.work_order_id)}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300"><span className="block max-w-[320px] truncate">{problemReportName(record.problem_report_id)}</span></td>
                  <td className="px-5 py-4"><Badge value={record.downtime_category} /></td>
                  <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">{formatDateTime(record.start_time)}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">{formatDateTime(record.end_time)}</td>
                  <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{formatNumber(record.duration_minutes)}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300"><span className="block max-w-[320px] truncate">{record.description || "-"}</span></td>
                  <td className="px-5 py-4">
                    <div className="flex justify-center gap-3">
                      <button aria-label="Edit" className="text-warning-500 transition-colors hover:text-warning-600 dark:text-warning-400 dark:hover:text-warning-500" onClick={() => openEdit(record)} type="button"><Icon className="h-5 w-5" name="edit" /></button>
                      <button aria-label="Delete" className="text-error-500 transition-colors hover:text-error-600 dark:text-error-400 dark:hover:text-error-500" onClick={() => setRecordToDelete(record)} type="button"><Icon className="h-5 w-5" name="trash" /></button>
                    </div>
                  </td>
                </tr>
              ))
            ) : (
              <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={9}>No downtime logs found.</td></tr>
            )}
          </tbody>
        </table>
      </TablePanel>

      <ConfirmModal
        confirmText="Delete"
        isDestructive
        isLoading={deleting}
        isOpen={Boolean(recordToDelete)}
        message={`Downtime log untuk "${recordToDelete ? assetName(recordToDelete.asset_id) : ""}" akan dihapus permanen.`}
        onClose={closeDeleteModal}
        onConfirm={() => void confirmDeleteRecord()}
        title="Delete Downtime Log"
      />
    </div>
  );
}
