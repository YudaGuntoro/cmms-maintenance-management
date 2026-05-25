"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { ConfirmModal, Modal } from "@/components/ui/modal";
import { getStoredUser } from "./auth";
import { apiDelete, apiGet, apiPost, apiPut, buildQuery } from "./api";
import { Icon } from "./icons";
import { formatDateTime, formatNumber, nowDateTimeLocal, statusText, toDateTimeLocalInput } from "./format";
import { options, type SelectOption } from "./options";
import { Badge, DataTableFooter, EntriesSelect, Feedback, InlineSearch, InlineSelect, SelectInput, TablePanel, TextareaInput, TextInput } from "./ui";
import type { Asset, ProblemReport, Technician, UserResponse, WorkOrder } from "./types";

type ProblemReportForm = {
  report_number: string;
  asset_id: string;
  title: string;
  description: string;
  category: string;
  priority: string;
  status: string;
  reported_by: string;
  reported_at: string;
  downtime_start: string;
};

type ProblemReportFilters = {
  asset_id: string;
  status: string;
  category: string;
};

const emptyForm: ProblemReportForm = {
  report_number: "",
  asset_id: "",
  title: "",
  description: "",
  category: "BREAKDOWN",
  priority: "MEDIUM",
  status: "PENDING",
  reported_by: "",
  reported_at: "",
  downtime_start: "",
};

function toForm(report?: ProblemReport, user?: UserResponse | null): ProblemReportForm {
  if (!report) {
    return {
      ...emptyForm,
      reported_by: user?.full_name || user?.username || "",
      reported_at: nowDateTimeLocal(),
    };
  }

  return {
    report_number: report.report_number || "",
    asset_id: String(report.asset_id || ""),
    title: report.title || "",
    description: report.description || "",
    category: report.category || "BREAKDOWN",
    priority: report.priority || "MEDIUM",
    status: report.status || "PENDING",
    reported_by: report.reported_by || user?.full_name || user?.username || "",
    reported_at: toDateTimeLocalInput(report.reported_at),
    downtime_start: toDateTimeLocalInput(report.downtime_start),
  };
}

function payload(form: ProblemReportForm) {
  return {
    report_number: form.report_number || "",
    asset_id: Number(form.asset_id),
    title: form.title,
    description: form.description || null,
    category: form.category,
    priority: form.priority,
    status: form.status,
    reported_by: form.reported_by || null,
    reported_at: form.reported_at || new Date().toISOString(),
    downtime_start: form.category === "DOWNTIME" ? form.downtime_start || null : null,
    downtime_end: null,
  };
}

function reportStatus(report: ProblemReport, workOrder?: WorkOrder) {
  if (!workOrder) {
    return report.status;
  }

  if (workOrder.status === "IN_PROGRESS") return "IN_PROGRESS";
  if (workOrder.status === "COMPLETED" || workOrder.status === "CLOSED") return "COMPLETED";
  if (workOrder.status === "CANCELLED") return "CANCELLED";
  return "PENDING";
}

function maintenanceTypeFromReport(report: ProblemReport) {
  if (report.category === "DOWNTIME" || report.category === "BREAKDOWN") {
    return "BREAKDOWN";
  }

  if (report.category === "QUALITY" || report.category === "SAFETY") {
    return "CORRECTIVE";
  }

  return "CORRECTIVE";
}

function downtimeDetail(report: ProblemReport, workOrder?: WorkOrder) {
  if (report.category !== "DOWNTIME") {
    return "-";
  }

  const start = formatDateTime(report.downtime_start);
  const end = workOrder?.downtime_end || report.downtime_end;
  const minutes = workOrder?.downtime_minutes ?? report.downtime_minutes;
  return end ? `${formatNumber(minutes)} min (${start} - ${formatDateTime(end)})` : `Since ${start}`;
}

export default function ReportProblemPage() {
  const [reports, setReports] = useState<ProblemReport[]>([]);
  const [assets, setAssets] = useState<Asset[]>([]);
  const [workOrders, setWorkOrders] = useState<WorkOrder[]>([]);
  const [technicians, setTechnicians] = useState<Technician[]>([]);
  const [user, setUser] = useState<UserResponse | null>(null);
  const [filters, setFilters] = useState<ProblemReportFilters>({ asset_id: "", status: "", category: "" });
  const [search, setSearch] = useState("");
  const [form, setForm] = useState<ProblemReportForm>(emptyForm);
  const [editing, setEditing] = useState<ProblemReport | null>(null);
  const [detailReport, setDetailReport] = useState<ProblemReport | null>(null);
  const [reportToDelete, setReportToDelete] = useState<ProblemReport | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const isAdminLike = user?.role === "ADMIN" || user?.role === "SUPERVISOR";
  const isOperatorView = user?.role === "OPERATOR" || user?.role === "VIEWER";

  const assetOptions = useMemo<SelectOption[]>(() => assets.map((asset) => ({ label: `${asset.asset_code} - ${asset.asset_name}`, value: String(asset.id) })), [assets]);

  const workOrderByReportId = useMemo(() => {
    const mapped = new Map<number, WorkOrder>();
    workOrders.forEach((workOrder) => {
      if (workOrder.problem_report_id && !mapped.has(workOrder.problem_report_id)) {
        mapped.set(workOrder.problem_report_id, workOrder);
      }
    });
    return mapped;
  }, [workOrders]);

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

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [reportData, assetData, workOrderData, technicianData] = await Promise.all([
        apiGet<ProblemReport[]>(`/api/problem-reports${buildQuery(filters)}`),
        apiGet<Asset[]>("/api/assets"),
        apiGet<WorkOrder[]>("/api/work-orders"),
        apiGet<Technician[]>("/api/technicians?status=ACTIVE"),
      ]);
      setReports(reportData || []);
      setAssets(assetData || []);
      setWorkOrders(workOrderData || []);
      setTechnicians(technicianData || []);
      setUser(getStoredUser());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load problem reports.");
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const visibleForRole = useMemo(() => {
    if (!isOperatorView || !user) {
      return reports;
    }

    const identities = [user.username, user.full_name].map((item) => item.toLowerCase());
    return reports.filter((report) => identities.includes(String(report.reported_by || "").toLowerCase()));
  }, [isOperatorView, reports, user]);

  const searchedReports = useMemo(() => {
    const normalized = search.trim().toLowerCase();
    if (!normalized) {
      return visibleForRole;
    }

    return visibleForRole.filter((report) => {
      const workOrder = workOrderByReportId.get(report.id);
      return [
        report.report_number,
        report.title,
        report.description,
        report.category,
        report.priority,
        reportStatus(report, workOrder),
        assetName(report.asset_id),
        workOrder?.wo_number,
        technicianName(workOrder?.assigned_to),
      ].some((value) => String(value || "").toLowerCase().includes(normalized));
    });
  }, [assetName, search, technicianName, visibleForRole, workOrderByReportId]);

  useEffect(() => {
    setPage(1);
  }, [filters, pageSize, search, searchedReports.length]);

  const pagedReports = useMemo(() => {
    const start = (page - 1) * pageSize;
    return searchedReports.slice(start, start + pageSize);
  }, [page, pageSize, searchedReports]);

  async function openCreate() {
    setEditing(null);
    setError(null);
    let reportNumber = "";
    try {
      reportNumber = await apiGet<string>("/api/problem-reports/next-number");
    } catch {
      setError("Failed to generate report number. You can still fill it manually or leave it empty.");
    }

    setForm({ ...toForm(undefined, user), report_number: reportNumber });
    setFormOpen(true);
  }

  function openEdit(report: ProblemReport) {
    setEditing(report);
    setForm(toForm(report, user));
    setFormOpen(true);
  }

  async function saveReport(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      if (editing) {
        await apiPut<ProblemReport>(`/api/problem-reports/${editing.id}`, payload(form));
        setSuccess("Problem report updated successfully.");
      } else {
        await apiPost<ProblemReport>("/api/problem-reports", payload(form));
        setSuccess("Problem report submitted successfully.");
      }

      setFormOpen(false);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save problem report.");
    } finally {
      setSaving(false);
    }
  }

  async function createWorkOrder(report: ProblemReport) {
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      await apiPost<WorkOrder>("/api/work-orders", {
        wo_number: "",
        asset_id: report.asset_id,
        problem_report_id: report.id,
        title: report.title,
        description: report.description || null,
        maintenance_type: maintenanceTypeFromReport(report),
        priority: report.priority,
        status: "OPEN",
        reported_by: report.reported_by || null,
        reported_at: report.reported_at,
        downtime_start: report.downtime_start || null,
        downtime_end: null,
      });
      setSuccess("Work order created from report.");
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create work order.");
    } finally {
      setSaving(false);
    }
  }

  async function confirmDeleteReport() {
    if (!reportToDelete) {
      return;
    }

    setDeleting(true);
    setError(null);
    setSuccess(null);
    try {
      await apiDelete<string>(`/api/problem-reports/${reportToDelete.id}`);
      setSuccess("Problem report deleted successfully.");
      setReportToDelete(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete problem report.");
    } finally {
      setDeleting(false);
    }
  }

  const detailWorkOrder = detailReport ? workOrderByReportId.get(detailReport.id) : undefined;

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Operations</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">Report Problem</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Operator problem reporting and work order tracking</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <button className="secondary-button" disabled={loading} onClick={() => void loadData()} type="button"><Icon name="refresh" />Refresh</button>
          <button className="primary-button" onClick={() => void openCreate()} type="button"><Icon name="plus" />Report</button>
        </div>
      </div>

      <Feedback error={error} success={success} />

      <TablePanel
        footer={<DataTableFooter onPageChange={setPage} page={page} pageSize={pageSize} total={searchedReports.length} />}
        toolbarLeft={(
          <>
            <EntriesSelect onChange={(value) => { setPageSize(value); setPage(1); }} value={pageSize} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, asset_id: value }))} options={assetOptions} placeholder="All Assets" value={filters.asset_id} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, status: value }))} options={options.problemReportStatus} placeholder="All Status" value={filters.status} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, category: value }))} options={options.problemReportCategory} placeholder="All Category" value={filters.category} />
          </>
        )}
        toolbarRight={<InlineSearch onChange={setSearch} placeholder="Search reports" value={search} />}
      >
        <table className="min-w-full">
          <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
            <tr>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Report No</th>
              <th className="min-w-64 px-5 py-3 text-left text-theme-xs font-semibold">Problem</th>
              <th className="min-w-60 px-5 py-3 text-left text-theme-xs font-semibold">Asset</th>
              <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Category</th>
              <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Priority</th>
              <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Status</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Work Order</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Assigned To</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Reported</th>
              <th className="min-w-40 px-5 py-3 text-center text-theme-xs font-semibold">Action</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
            {loading ? (
              Array.from({ length: 5 }).map((_, index) => <tr key={index}>{Array.from({ length: 10 }).map((__, cellIndex) => <td className="px-5 py-4" key={cellIndex}><div className="h-4 w-full animate-pulse rounded-md bg-gray-100 dark:bg-white/[0.05]" /></td>)}</tr>)
            ) : searchedReports.length ? (
              pagedReports.map((report) => {
                const workOrder = workOrderByReportId.get(report.id);
                return (
                  <tr className="cursor-pointer hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={report.id} onClick={() => setDetailReport(report)}>
                    <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{report.report_number}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300"><span className="block max-w-[280px] truncate">{report.title}</span></td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{assetName(report.asset_id)}</td>
                    <td className="px-5 py-4"><Badge value={report.category} /></td>
                    <td className="px-5 py-4"><Badge value={report.priority} /></td>
                    <td className="px-5 py-4"><Badge value={reportStatus(report, workOrder)} /></td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{workOrder?.wo_number || "-"}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{technicianName(workOrder?.assigned_to)}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">{formatDateTime(report.reported_at)}</td>
                    <td className="px-5 py-4">
                      <div className="flex items-center justify-center gap-3">
                        {isAdminLike && !workOrder ? <button aria-label="Create work order" className="text-blue-light-500 transition-colors hover:text-blue-light-600" disabled={saving} onClick={(event) => { event.stopPropagation(); void createWorkOrder(report); }} type="button"><Icon className="h-5 w-5" name="wrench" /></button> : null}
                        {isAdminLike ? <button aria-label="Edit" className="text-warning-500 transition-colors hover:text-warning-600 dark:text-warning-400 dark:hover:text-warning-500" onClick={(event) => { event.stopPropagation(); openEdit(report); }} type="button"><Icon className="h-5 w-5" name="edit" /></button> : null}
                        {isAdminLike ? <button aria-label="Delete" className="text-error-500 transition-colors hover:text-error-600 dark:text-error-400 dark:hover:text-error-500" onClick={(event) => { event.stopPropagation(); setReportToDelete(report); }} type="button"><Icon className="h-5 w-5" name="trash" /></button> : null}
                      </div>
                    </td>
                  </tr>
                );
              })
            ) : (
              <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={10}>No problem reports found.</td></tr>
            )}
          </tbody>
        </table>
      </TablePanel>

      <Modal isOpen={formOpen} onClose={() => { if (!saving) setFormOpen(false); }} className="mx-4 max-w-[860px] overflow-hidden p-0" showCloseButton={false}>
        <div className="border-b border-gray-100 px-6 py-5 dark:border-white/[0.05]">
          <div className="flex items-start gap-4">
            <div className="flex size-12 shrink-0 items-center justify-center rounded-lg bg-brand-500/10 text-brand-500 ring-1 ring-brand-500/20">
              <Icon className="size-6" name={form.category === "DOWNTIME" ? "alert" : "wrench"} />
            </div>
            <div className="min-w-0 flex-1">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white/90">{editing ? "Edit Problem Report" : "New Problem Report"}</h3>
              <p className="mt-1 text-sm leading-6 text-gray-500 dark:text-gray-400">{editing ? "Update report data and tracking status." : "Submit operator problem report for maintenance follow-up."}</p>
            </div>
            <button className="icon-button" disabled={saving} onClick={() => setFormOpen(false)} type="button"><Icon name="x" /></button>
          </div>
        </div>

        <form onSubmit={(event) => void saveReport(event)}>
          <div className="grid max-h-[70vh] grid-cols-1 gap-4 overflow-y-auto px-6 py-5 md:grid-cols-2 xl:grid-cols-3">
            <TextInput helper="Generated automatically, but you can edit it." label="Report Number" name="report_number" onChange={(event) => setForm((current) => ({ ...current, report_number: event.target.value }))} value={form.report_number} />
            <SelectInput label="Asset" name="asset_id" onChange={(event) => setForm((current) => ({ ...current, asset_id: event.target.value }))} options={assetOptions} placeholder="Select Asset" required value={form.asset_id} />
            <TextInput label="Title" name="title" onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))} required value={form.title} />
            <SelectInput label="Category" name="category" onChange={(event) => setForm((current) => ({ ...current, category: event.target.value, downtime_start: event.target.value === "DOWNTIME" ? current.downtime_start || current.reported_at || nowDateTimeLocal() : "" }))} options={options.problemReportCategory} required value={form.category} />
            <SelectInput label="Priority" name="priority" onChange={(event) => setForm((current) => ({ ...current, priority: event.target.value }))} options={options.workOrderPriority} required value={form.priority} />
            {editing && isAdminLike ? <SelectInput label="Status" name="status" onChange={(event) => setForm((current) => ({ ...current, status: event.target.value }))} options={options.problemReportStatus} required value={form.status} /> : null}
            <TextInput label="Reported By" name="reported_by" onChange={(event) => setForm((current) => ({ ...current, reported_by: event.target.value }))} value={form.reported_by} />
            <TextInput label="Reported At" name="reported_at" onChange={(event) => setForm((current) => ({ ...current, reported_at: event.target.value }))} required type="datetime-local" value={form.reported_at || nowDateTimeLocal()} />
            {form.category === "DOWNTIME" ? (
              <TextInput helper="Isi jam mulai downtime. Downtime end akan ditutup saat pekerjaan selesai." label="Downtime Start" name="downtime_start" onChange={(event) => setForm((current) => ({ ...current, downtime_start: event.target.value }))} required type="datetime-local" value={form.downtime_start} />
            ) : null}
            <div className="md:col-span-2 xl:col-span-3"><TextareaInput label="Description" name="description" onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} value={form.description} /></div>
          </div>
          <div className="flex flex-col-reverse gap-3 border-t border-gray-100 px-6 py-4 dark:border-white/[0.05] sm:flex-row sm:justify-end">
            <button className="secondary-button h-11" disabled={saving} onClick={() => setFormOpen(false)} type="button">Cancel</button>
            <button className="primary-button h-11 min-w-32" disabled={saving} type="submit"><Icon name="check" />{saving ? "Saving..." : "Save"}</button>
          </div>
        </form>
      </Modal>

      <Modal isOpen={Boolean(detailReport)} onClose={() => setDetailReport(null)} className="mx-4 max-w-[780px] overflow-hidden p-0" showCloseButton={false}>
        {detailReport ? (
          <>
            <div className="border-b border-gray-100 px-6 py-5 dark:border-white/[0.05]">
              <div className="flex items-start gap-4">
                <div className="flex size-12 shrink-0 items-center justify-center rounded-lg bg-brand-500/10 text-brand-500 ring-1 ring-brand-500/20">
                  <Icon className="size-6" name={detailReport.category === "DOWNTIME" ? "alert" : "wrench"} />
                </div>
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <h3 className="text-lg font-semibold text-gray-900 dark:text-white/90">{detailReport.report_number}</h3>
                    <Badge value={reportStatus(detailReport, detailWorkOrder)} />
                  </div>
                  <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{detailReport.title}</p>
                </div>
                <button className="icon-button" onClick={() => setDetailReport(null)} type="button"><Icon name="x" /></button>
              </div>
            </div>

            <div className="grid grid-cols-1 gap-4 px-6 py-5 md:grid-cols-2">
              <DetailItem label="Asset" value={assetName(detailReport.asset_id)} />
              <DetailItem label="Reported By" value={detailReport.reported_by || "-"} />
              <DetailItem label="Category" value={statusText(detailReport.category)} />
              <DetailItem label="Priority" value={statusText(detailReport.priority)} />
              <DetailItem label="Reported" value={formatDateTime(detailReport.reported_at)} />
              <DetailItem label="Downtime" value={downtimeDetail(detailReport, detailWorkOrder)} />
              <DetailItem label="Work Order" value={detailWorkOrder?.wo_number || "-"} />
              <DetailItem label="Assigned To" value={technicianName(detailWorkOrder?.assigned_to)} />
              <div className="md:col-span-2">
                <DetailItem label="Description" value={detailReport.description || "-"} />
              </div>
            </div>
          </>
        ) : null}
      </Modal>

      <ConfirmModal
        confirmText="Delete"
        isDestructive
        isLoading={deleting}
        isOpen={Boolean(reportToDelete)}
        message={`Problem report "${reportToDelete?.report_number || reportToDelete?.title || ""}" akan dihapus permanen.`}
        onClose={() => { if (!deleting) setReportToDelete(null); }}
        onConfirm={() => void confirmDeleteReport()}
        title="Delete Problem Report"
      />
    </div>
  );
}

function DetailItem({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-gray-100 bg-gray-50 px-4 py-3 dark:border-white/[0.05] dark:bg-white/[0.03]">
      <p className="text-xs font-semibold uppercase tracking-[0.14em] text-gray-500 dark:text-gray-400">{label}</p>
      <p className="mt-1 text-sm font-medium text-gray-900 dark:text-white/90">{value}</p>
    </div>
  );
}
