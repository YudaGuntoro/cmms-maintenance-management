"use client";

import { useCallback, useEffect, useMemo, useState, type ChangeEvent, type FormEvent } from "react";
import DatePicker from "@/components/form/date-picker";
import { ConfirmModal, Modal } from "@/components/ui/modal";
import { apiDelete, apiGet, apiPost, apiPostForm, apiPut, buildQuery } from "./api";
import { FileDropzone } from "./FileDropzone";
import { formatDateTime, formatNumber, nowDateTimeLocal, statusText, toDateTimeLocalInput } from "./format";
import { Icon } from "./icons";
import { options, type SelectOption } from "./options";
import { Badge, Card, CardHeader, CheckboxInput, DataTableFooter, EntriesSelect, Feedback, InlineSearch, InlineSelect, SelectInput, TablePanel, TextareaInput, TextInput } from "./ui";
import type { Asset, ContractorWorkPlan, ContractorWorkReminder, WorkOrder } from "./types";

type ContractorForm = {
  vendor_name: string;
  vendor_pic_name: string;
  vendor_pic_phone: string;
  worker_count: string;
  internal_pic_name: string;
  department_area: string;
  work_title: string;
  work_description: string;
  work_area: string;
  work_location: string;
  asset_id: string;
  additional_notes: string;
  start_at: string;
  end_at: string;
  status: string;
  permit_document_status: string;
  working_at_height: boolean;
  hot_work: boolean;
  welding: boolean;
  electrical_work: boolean;
  confined_space: boolean;
  heavy_equipment_activity: boolean;
  chemical_handling: boolean;
  shutdown_activity: boolean;
  loto_required: boolean;
  need_safety_standby: boolean;
};

type ContractorFilters = {
  vendor: string;
  area: string;
  status: string;
  start_date: string;
  end_date: string;
  risk: string;
  pic_mtc: string;
};

const riskDefinitions: Array<{ key: keyof Pick<ContractorForm, "working_at_height" | "hot_work" | "welding" | "electrical_work" | "confined_space" | "heavy_equipment_activity" | "chemical_handling" | "shutdown_activity" | "loto_required" | "need_safety_standby">; label: string; value: string }> = [
  { key: "working_at_height", label: "Working at Height", value: "working_at_height" },
  { key: "hot_work", label: "Hot Work", value: "hot_work" },
  { key: "welding", label: "Welding", value: "welding" },
  { key: "electrical_work", label: "Electrical Work", value: "electrical_work" },
  { key: "confined_space", label: "Confined Space", value: "confined_space" },
  { key: "heavy_equipment_activity", label: "Heavy Equipment", value: "heavy_equipment_activity" },
  { key: "chemical_handling", label: "Chemical Handling", value: "chemical_handling" },
  { key: "shutdown_activity", label: "Shutdown Activity", value: "shutdown_activity" },
  { key: "loto_required", label: "LOTO Required", value: "loto_required" },
  { key: "need_safety_standby", label: "Need Safety Standby", value: "need_safety_standby" },
];

const riskOptions: SelectOption[] = [
  { label: "High Risk", value: "high_risk" },
  ...riskDefinitions.map((risk) => ({ label: risk.label, value: risk.value })),
];

const emptyFilters: ContractorFilters = {
  vendor: "",
  area: "",
  status: "",
  start_date: "",
  end_date: "",
  risk: "",
  pic_mtc: "",
};

function defaultForm(): ContractorForm {
  const start = nowDateTimeLocal();
  const startDate = new Date();
  startDate.setHours(startDate.getHours() + 2);
  const end = toDateTimeLocalInput(startDate.toISOString());

  return {
    vendor_name: "",
    vendor_pic_name: "",
    vendor_pic_phone: "",
    worker_count: "1",
    internal_pic_name: "",
    department_area: "",
    work_title: "",
    work_description: "",
    work_area: "",
    work_location: "",
    asset_id: "",
    additional_notes: "",
    start_at: start,
    end_at: end,
    status: "PLANNED",
    permit_document_status: "NOT_UPLOADED",
    working_at_height: false,
    hot_work: false,
    welding: false,
    electrical_work: false,
    confined_space: false,
    heavy_equipment_activity: false,
    chemical_handling: false,
    shutdown_activity: false,
    loto_required: false,
    need_safety_standby: false,
  };
}

function toForm(plan?: ContractorWorkPlan | null): ContractorForm {
  if (!plan) {
    return defaultForm();
  }

  return {
    vendor_name: plan.vendor_name || "",
    vendor_pic_name: plan.vendor_pic_name || "",
    vendor_pic_phone: plan.vendor_pic_phone || "",
    worker_count: String(plan.worker_count ?? 0),
    internal_pic_name: plan.internal_pic_name || "",
    department_area: plan.department_area || "",
    work_title: plan.work_title || "",
    work_description: plan.work_description || "",
    work_area: plan.work_area || "",
    work_location: plan.work_location || "",
    asset_id: plan.asset_id ? String(plan.asset_id) : "",
    additional_notes: plan.additional_notes || "",
    start_at: toDateTimeLocalInput(plan.start_at),
    end_at: toDateTimeLocalInput(plan.end_at),
    status: plan.status || "PLANNED",
    permit_document_status: plan.permit_document_status || "NOT_UPLOADED",
    working_at_height: Boolean(plan.working_at_height),
    hot_work: Boolean(plan.hot_work),
    welding: Boolean(plan.welding),
    electrical_work: Boolean(plan.electrical_work),
    confined_space: Boolean(plan.confined_space),
    heavy_equipment_activity: Boolean(plan.heavy_equipment_activity),
    chemical_handling: Boolean(plan.chemical_handling),
    shutdown_activity: Boolean(plan.shutdown_activity),
    loto_required: Boolean(plan.loto_required),
    need_safety_standby: Boolean(plan.need_safety_standby),
  };
}

function payload(form: ContractorForm) {
  return {
    vendor_name: form.vendor_name,
    vendor_pic_name: form.vendor_pic_name,
    vendor_pic_phone: form.vendor_pic_phone || null,
    worker_count: Number(form.worker_count || 0),
    internal_pic_name: form.internal_pic_name,
    department_area: form.department_area,
    work_title: form.work_title,
    work_description: form.work_description || null,
    work_area: form.work_area,
    work_location: form.work_location || null,
    asset_id: form.asset_id ? Number(form.asset_id) : null,
    additional_notes: form.additional_notes || null,
    start_at: form.start_at,
    end_at: form.end_at,
    status: form.status,
    permit_document_status: form.permit_document_status,
    working_at_height: form.working_at_height,
    hot_work: form.hot_work,
    welding: form.welding,
    electrical_work: form.electrical_work,
    confined_space: form.confined_space,
    heavy_equipment_activity: form.heavy_equipment_activity,
    chemical_handling: form.chemical_handling,
    shutdown_activity: form.shutdown_activity,
    loto_required: form.loto_required,
    need_safety_standby: form.need_safety_standby,
  };
}

function reminderClasses(severity: string) {
  if (severity === "DANGER") {
    return "border-error-500/20 bg-error-50 text-error-700 dark:border-error-500/30 dark:bg-error-500/10 dark:text-error-400";
  }

  if (severity === "WARNING") {
    return "border-warning-500/25 bg-warning-50 text-warning-700 dark:border-warning-500/30 dark:bg-warning-500/10 dark:text-warning-400";
  }

  return "border-blue-light-500/20 bg-blue-light-50 text-blue-light-700 dark:border-blue-light-500/30 dark:bg-blue-light-500/10 dark:text-blue-light-500";
}

function durationText(minutes: number) {
  if (!Number.isFinite(minutes) || minutes <= 0) {
    return "-";
  }

  if (minutes < 60) {
    return `${minutes} min`;
  }

  return `${Math.floor(minutes / 60)}h ${minutes % 60}m`;
}

export default function ContractorMonitoringPage() {
  const [plans, setPlans] = useState<ContractorWorkPlan[]>([]);
  const [reminders, setReminders] = useState<ContractorWorkReminder[]>([]);
  const [assets, setAssets] = useState<Asset[]>([]);
  const [filters, setFilters] = useState<ContractorFilters>(emptyFilters);
  const [search, setSearch] = useState("");
  const [form, setForm] = useState<ContractorForm>(() => defaultForm());
  const [editing, setEditing] = useState<ContractorWorkPlan | null>(null);
  const [uploadPlan, setUploadPlan] = useState<ContractorWorkPlan | null>(null);
  const [planToDelete, setPlanToDelete] = useState<ContractorWorkPlan | null>(null);
  const [documentFile, setDocumentFile] = useState<File | null>(null);
  const [documentType, setDocumentType] = useState("PERMIT");
  const [documentStatus, setDocumentStatus] = useState("UPLOADED");
  const [documentExpiresAt, setDocumentExpiresAt] = useState("");
  const [documentNotes, setDocumentNotes] = useState("");
  const [formOpen, setFormOpen] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const assetOptions = useMemo<SelectOption[]>(() => assets.map((asset) => ({ label: `${asset.asset_code} - ${asset.asset_name}`, value: String(asset.id) })), [assets]);

  const stats = useMemo(() => {
    const permitAttention = plans.filter((plan) => plan.permit_document_status !== "UPLOADED").length;
    const ready = plans.filter((plan) => plan.status === "READY_TO_START" || plan.status === "ONGOING").length;
    const highRisk = plans.filter((plan) => plan.has_high_risk).length;
    const startingSoon = reminders.filter((reminder) => reminder.type === "STARTING_SOON").length;
    return { permitAttention, ready, highRisk, startingSoon, total: plans.length };
  }, [plans, reminders]);

  const filteredPlans = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();
    if (!normalizedSearch) {
      return plans;
    }

    return plans.filter((plan) =>
      [plan.vendor_name, plan.work_title, plan.work_area, plan.vendor_pic_name, plan.internal_pic_name, plan.permit_document_status, plan.status]
        .filter(Boolean)
        .some((value) => String(value).toLowerCase().includes(normalizedSearch))
    );
  }, [plans, search]);

  const pagedPlans = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredPlans.slice(start, start + pageSize);
  }, [filteredPlans, page, pageSize]);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [planData, reminderData, assetData] = await Promise.all([
        apiGet<ContractorWorkPlan[]>(`/api/contractor-monitoring${buildQuery(filters)}`),
        apiGet<ContractorWorkReminder[]>("/api/contractor-monitoring/reminders"),
        apiGet<Asset[]>("/api/assets"),
      ]);
      setPlans(planData || []);
      setReminders(reminderData || []);
      setAssets(assetData || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load contractor monitoring.");
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  useEffect(() => {
    setPage(1);
  }, [filters, search, pageSize]);

  const onFormChange = (event: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const target = event.target;
    const name = target.name as keyof ContractorForm;
    const value = target instanceof HTMLInputElement && target.type === "checkbox" ? target.checked : target.value;
    setForm((current) => ({ ...current, [name]: value }));
  };

  const openCreate = () => {
    setEditing(null);
    setForm(defaultForm());
    setFormOpen(true);
  };

  const openEdit = async (plan: ContractorWorkPlan) => {
    setError(null);
    try {
      const detail = await apiGet<ContractorWorkPlan>(`/api/contractor-monitoring/${plan.id}`);
      setEditing(detail || plan);
      setForm(toForm(detail || plan));
      setFormOpen(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load contractor plan.");
    }
  };

  const submitForm = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      if (editing) {
        await apiPut<ContractorWorkPlan>(`/api/contractor-monitoring/${editing.id}`, payload(form));
        setSuccess("Contractor plan updated.");
      } else {
        await apiPost<ContractorWorkPlan>("/api/contractor-monitoring", payload(form));
        setSuccess("Contractor plan created.");
      }

      setFormOpen(false);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save contractor plan.");
    } finally {
      setSaving(false);
    }
  };

  const deletePlan = async () => {
    if (!planToDelete) {
      return;
    }

    setDeleting(true);
    setError(null);
    setSuccess(null);
    try {
      await apiDelete<string>(`/api/contractor-monitoring/${planToDelete.id}`);
      setSuccess("Contractor plan deleted.");
      setPlanToDelete(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete contractor plan.");
    } finally {
      setDeleting(false);
    }
  };

  const openUpload = async (plan: ContractorWorkPlan) => {
    setError(null);
    try {
      const detail = await apiGet<ContractorWorkPlan>(`/api/contractor-monitoring/${plan.id}`);
      setUploadPlan(detail || plan);
    } catch {
      setUploadPlan(plan);
    }
    setDocumentFile(null);
    setDocumentType("PERMIT");
    setDocumentStatus("UPLOADED");
    setDocumentExpiresAt("");
    setDocumentNotes("");
  };

  const uploadDocument = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!uploadPlan || !documentFile) {
      setError("File dokumen wajib dipilih.");
      return;
    }

    setUploading(true);
    setError(null);
    setSuccess(null);
    try {
      const formData = new FormData();
      formData.append("file", documentFile);
      formData.append("document_type", documentType);
      formData.append("document_status", documentStatus);
      if (documentExpiresAt) {
        formData.append("expires_at", documentExpiresAt);
      }
      if (documentNotes) {
        formData.append("notes", documentNotes);
      }

      await apiPostForm(`/api/contractor-monitoring/${uploadPlan.id}/documents`, formData);
      setSuccess("Document uploaded.");
      setUploadPlan(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to upload document.");
    } finally {
      setUploading(false);
    }
  };

  const createSupervisionWorkOrder = async (plan: ContractorWorkPlan) => {
    setError(null);
    setSuccess(null);
    try {
      const workOrder = await apiPost<WorkOrder>(`/api/contractor-monitoring/${plan.id}/supervision-work-order`);
      setSuccess(`WO ${workOrder?.wo_number || ""} Contractor Supervision created.`);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create supervision WO.");
    }
  };

  const copyVendorFormLink = async () => {
    const url = `${window.location.origin}/vendor-monitoring`;
    setError(null);
    setSuccess(null);
    try {
      if (navigator.clipboard?.writeText) {
        await navigator.clipboard.writeText(url);
      } else {
        const textarea = document.createElement("textarea");
        textarea.value = url;
        textarea.setAttribute("readonly", "true");
        textarea.style.position = "fixed";
        textarea.style.opacity = "0";
        document.body.appendChild(textarea);
        textarea.select();
        document.execCommand("copy");
        document.body.removeChild(textarea);
      }
      setSuccess("Vendor form link copied.");
    } catch {
      setError(`Gagal copy otomatis. Link form vendor: ${url}`);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">Contractor Monitoring</h1>
        </div>
        <div className="flex flex-col gap-2 sm:flex-row">
          <button className="inline-flex h-11 items-center justify-center gap-2 rounded-lg border border-gray-300 bg-white px-4 text-sm font-semibold text-gray-700 shadow-theme-xs transition-colors hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200 dark:hover:bg-white/[0.04]" onClick={copyVendorFormLink} type="button">
            <Icon name="copy" className="h-4 w-4" />
            Copy Vendor Link
          </button>
          <button className="inline-flex h-11 items-center justify-center gap-2 rounded-lg bg-brand-500 px-4 text-sm font-semibold text-white shadow-theme-xs transition-colors hover:bg-brand-600" onClick={openCreate} type="button">
            <Icon name="plus" className="h-4 w-4" />
            New Plan
          </button>
        </div>
      </div>

      <Feedback error={error} success={success} />

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
        <MetricCard label="Total Plans" value={stats.total} tone="gray" />
        <MetricCard label="Ready / Ongoing" value={stats.ready} tone="green" />
        <MetricCard label="Permit Attention" value={stats.permitAttention} tone="amber" />
        <MetricCard label="High Risk" value={stats.highRisk} tone="red" />
        <MetricCard label="Starting Soon" value={stats.startingSoon} tone="blue" />
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_360px]">
        <TablePanel
          footer={<DataTableFooter onPageChange={setPage} page={page} pageSize={pageSize} total={filteredPlans.length} />}
          toolbarLeft={
            <>
              <EntriesSelect onChange={setPageSize} value={pageSize} />
              <InlineSearch onChange={setSearch} placeholder="Vendor, work, area, PIC" value={search} />
            </>
          }
          toolbarRight={
            <>
              <InlineSelect label="Status" onChange={(value) => setFilters((current) => ({ ...current, status: value }))} options={options.contractorWorkStatus} value={filters.status} />
              <InlineSelect label="Risk" onChange={(value) => setFilters((current) => ({ ...current, risk: value }))} options={riskOptions} value={filters.risk} />
            </>
          }
        >
          <div className="border-b border-gray-100 bg-gray-50 px-4 py-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
            <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-[minmax(180px,1fr)_minmax(180px,1fr)_minmax(160px,0.9fr)_minmax(250px,0.9fr)]">
              <input className="control-input" onChange={(event) => setFilters((current) => ({ ...current, vendor: event.target.value }))} placeholder="Vendor" value={filters.vendor} />
              <input className="control-input" onChange={(event) => setFilters((current) => ({ ...current, area: event.target.value }))} placeholder="Area kerja" value={filters.area} />
              <input className="control-input" onChange={(event) => setFilters((current) => ({ ...current, pic_mtc: event.target.value }))} placeholder="PIC MTC" value={filters.pic_mtc} />
              <div className="grid grid-cols-2 gap-3">
                <DateFilterInput onChange={(value) => setFilters((current) => ({ ...current, start_date: value }))} placeholder="Start date" value={filters.start_date} />
                <DateFilterInput onChange={(value) => setFilters((current) => ({ ...current, end_date: value }))} placeholder="End date" value={filters.end_date} />
              </div>
            </div>
          </div>

          <table className="min-w-[1120px] divide-y divide-gray-100 text-left text-sm dark:divide-white/[0.05]">
            <thead className="bg-gray-50 text-xs uppercase tracking-wide text-gray-500 dark:bg-white/[0.03] dark:text-gray-400">
              <tr>
                <th className="px-4 py-3 font-semibold">Vendor</th>
                <th className="px-4 py-3 font-semibold">Work</th>
                <th className="px-4 py-3 font-semibold">Area</th>
                <th className="px-4 py-3 font-semibold">Schedule</th>
                <th className="px-4 py-3 font-semibold">PIC</th>
                <th className="px-4 py-3 font-semibold">Status</th>
                <th className="px-4 py-3 font-semibold">Risk</th>
                <th className="px-4 py-3 font-semibold">Permit</th>
                <th className="px-4 py-3 text-right font-semibold">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
              {loading ? (
                <tr>
                  <td className="px-4 py-8 text-center text-gray-500 dark:text-gray-400" colSpan={9}>Loading...</td>
                </tr>
              ) : pagedPlans.length === 0 ? (
                <tr>
                  <td className="px-4 py-8 text-center text-gray-500 dark:text-gray-400" colSpan={9}>No contractor plans found.</td>
                </tr>
              ) : (
                pagedPlans.map((plan) => (
                  <tr className="align-top hover:bg-gray-50 dark:hover:bg-white/[0.03]" key={plan.id}>
                    <td className="px-4 py-4">
                      <div className="font-semibold text-gray-900 dark:text-white">{plan.vendor_name}</div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">{plan.vendor_pic_name} · {plan.vendor_pic_phone || "-"}</div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">{formatNumber(plan.worker_count)} workers</div>
                    </td>
                    <td className="px-4 py-4">
                      <div className="font-medium text-gray-800 dark:text-gray-100">{plan.work_title}</div>
                      <div className="mt-1 line-clamp-2 max-w-[240px] text-xs text-gray-500 dark:text-gray-400">{plan.work_description || plan.additional_notes || "-"}</div>
                    </td>
                    <td className="px-4 py-4 text-gray-700 dark:text-gray-300">
                      <div>{plan.work_area}</div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">{plan.work_location || plan.department_area}</div>
                    </td>
                    <td className="px-4 py-4 text-gray-700 dark:text-gray-300">
                      <div>{formatDateTime(plan.start_at)}</div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">{formatDateTime(plan.end_at)}</div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">{durationText(plan.estimated_duration_minutes)}</div>
                    </td>
                    <td className="px-4 py-4 text-gray-700 dark:text-gray-300">
                      <div>{plan.internal_pic_name}</div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">MTC</div>
                    </td>
                    <td className="px-4 py-4"><Badge value={plan.status} /></td>
                    <td className="px-4 py-4">
                      <div className="flex max-w-[220px] flex-wrap gap-1.5">
                        {(plan.risk_tags?.length ? plan.risk_tags : ["Normal"]).slice(0, 4).map((risk) => (
                          <RiskPill highRisk={plan.has_high_risk} key={risk} label={risk} />
                        ))}
                      </div>
                    </td>
                    <td className="px-4 py-4">
                      <Badge value={plan.permit_document_status} />
                      <div className="mt-1 text-xs text-gray-500 dark:text-gray-400">{plan.documents?.length || 0} docs</div>
                    </td>
                    <td className="px-4 py-4">
                      <div className="flex justify-end gap-2">
                        <IconButton label="Edit" name="edit" onClick={() => void openEdit(plan)} />
                        <IconButton label="Upload" name="upload" onClick={() => void openUpload(plan)} />
                        <IconButton
                          disabled={Boolean(plan.work_order_id)}
                          label={plan.work_order_id ? "WO Supervisi sudah dibuat" : "Buat WO Supervisi Vendor"}
                          name="activity"
                          onClick={() => void createSupervisionWorkOrder(plan)}
                          text={plan.work_order_id ? "WO dibuat" : "Buat WO"}
                        />
                        <IconButton destructive label="Delete" name="trash" onClick={() => setPlanToDelete(plan)} />
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </TablePanel>

        <Card>
          <CardHeader title="Reminders" />
          <div className="space-y-3 p-4">
            {reminders.length === 0 ? (
              <p className="text-sm text-gray-500 dark:text-gray-400">No reminders.</p>
            ) : (
              reminders.slice(0, 8).map((reminder, index) => (
                <div className={`rounded-lg border px-3 py-3 text-sm ${reminderClasses(reminder.severity)}`} key={`${reminder.contractor_work_plan_id}-${reminder.type}-${index}`}>
                  <div className="flex items-start justify-between gap-3">
                    <span className="font-semibold">{statusText(reminder.type)}</span>
                    <span className="text-xs font-semibold">{reminder.severity}</span>
                  </div>
                  <p className="mt-1 text-xs leading-5">{reminder.message}</p>
                  <p className="mt-2 text-xs">{reminder.vendor_name} · {reminder.work_area}</p>
                  <p className="text-xs">{formatDateTime(reminder.start_at)}</p>
                </div>
              ))
            )}
          </div>
        </Card>
      </div>

      <Modal className="mx-4 max-h-[92vh] max-w-[980px] overflow-y-auto p-0" isOpen={formOpen} onClose={() => setFormOpen(false)}>
        <form className="p-6 sm:p-7" onSubmit={submitForm}>
          <div className="mb-6 pr-12">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white">{editing ? "Edit Contractor Plan" : "New Contractor Plan"}</h2>
          </div>

          <div className="grid gap-5 lg:grid-cols-2">
            <TextInput label="Nama Vendor" name="vendor_name" onChange={onFormChange} required value={form.vendor_name} />
            <TextInput label="PIC Vendor" name="vendor_pic_name" onChange={onFormChange} required value={form.vendor_pic_name} />
            <TextInput label="Nomor HP PIC Vendor" name="vendor_pic_phone" onChange={onFormChange} value={form.vendor_pic_phone} />
            <TextInput label="Jumlah Pekerja" name="worker_count" onChange={onFormChange} required type="number" value={form.worker_count} />
            <TextInput label="PIC MTC" name="internal_pic_name" onChange={onFormChange} required value={form.internal_pic_name} />
            <TextInput label="Departemen / Area Terkait" name="department_area" onChange={onFormChange} required value={form.department_area} />
            <TextInput label="Judul Pekerjaan" name="work_title" onChange={onFormChange} required value={form.work_title} />
            <TextInput label="Area / Lokasi Pekerjaan" name="work_area" onChange={onFormChange} required value={form.work_area} />
            <TextInput label="Detail Lokasi" name="work_location" onChange={onFormChange} value={form.work_location} />
            <SelectInput label="Asset / Mesin" name="asset_id" onChange={onFormChange} options={assetOptions} placeholder="Optional" value={form.asset_id} />
            <TextInput label="Tanggal & Jam Mulai" name="start_at" onChange={onFormChange} required type="datetime-local" value={form.start_at} />
            <TextInput label="Tanggal & Jam Selesai" name="end_at" onChange={onFormChange} required type="datetime-local" value={form.end_at} />
            <SelectInput label="Status" name="status" onChange={onFormChange} options={options.contractorWorkStatus} required value={form.status} />
            <SelectInput label="Status Permit" name="permit_document_status" onChange={onFormChange} options={options.contractorDocumentStatus} required value={form.permit_document_status} />
            <div className="lg:col-span-2">
              <TextareaInput label="Deskripsi Pekerjaan" name="work_description" onChange={onFormChange} value={form.work_description} />
            </div>
            <div className="lg:col-span-2">
              <TextareaInput label="Catatan Tambahan" name="additional_notes" onChange={onFormChange} value={form.additional_notes} />
            </div>
          </div>

          <div className="mt-6">
            <h3 className="mb-3 text-sm font-semibold text-gray-800 dark:text-gray-100">Checklist Risiko / Aktivitas</h3>
            <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
              {riskDefinitions.map((risk) => (
                <CheckboxInput checked={Boolean(form[risk.key])} key={risk.key} label={risk.label} name={risk.key} onChange={onFormChange} />
              ))}
            </div>
          </div>

          {editing?.audits?.length ? (
            <div className="mt-6 rounded-lg border border-gray-200 bg-gray-50 p-4 dark:border-gray-800 dark:bg-white/[0.03]">
              <h3 className="mb-3 text-sm font-semibold text-gray-800 dark:text-gray-100">Audit Trail</h3>
              <div className="max-h-36 space-y-2 overflow-y-auto text-xs text-gray-600 dark:text-gray-300">
                {editing.audits.slice(0, 8).map((audit) => (
                  <div className="flex flex-col gap-1 border-b border-gray-100 pb-2 last:border-b-0 dark:border-white/[0.05]" key={audit.id}>
                    <span className="font-semibold">{statusText(audit.action)} {audit.field_name ? `· ${audit.field_name}` : ""}</span>
                    <span>{audit.old_value || "-"} → {audit.new_value || "-"}</span>
                    <span>{audit.performed_by || "-"} · {formatDateTime(audit.created_at)}</span>
                  </div>
                ))}
              </div>
            </div>
          ) : null}

          <div className="mt-7 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
            <button className="inline-flex h-11 items-center justify-center rounded-lg border border-gray-300 bg-white px-5 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200" onClick={() => setFormOpen(false)} type="button">
              Cancel
            </button>
            <button className="inline-flex h-11 items-center justify-center gap-2 rounded-lg bg-brand-500 px-5 text-sm font-semibold text-white transition-colors hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-60" disabled={saving} type="submit">
              <Icon name="check" className="h-4 w-4" />
              {saving ? "Saving..." : "Save Plan"}
            </button>
          </div>
        </form>
      </Modal>

      <Modal className="mx-4 max-w-[620px] overflow-hidden p-0" isOpen={Boolean(uploadPlan)} onClose={() => setUploadPlan(null)}>
        <form className="p-6 sm:p-7" onSubmit={uploadDocument}>
          <div className="mb-6 pr-12">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Upload Document</h2>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{uploadPlan?.vendor_name} · {uploadPlan?.work_title}</p>
          </div>
          <div className="grid gap-5">
            <SelectInput label="Document Type" name="document_type" onChange={(event) => setDocumentType(event.target.value)} options={options.contractorDocumentType} required value={documentType} />
            <SelectInput label="Document Status" name="document_status" onChange={(event) => setDocumentStatus(event.target.value)} options={options.contractorDocumentStatus} required value={documentStatus} />
            <TextInput label="Expired At" name="expires_at" onChange={(event) => setDocumentExpiresAt(event.target.value)} type="date" value={documentExpiresAt} />
            <FileDropzone file={documentFile} label="File" onChange={setDocumentFile} />
            <TextareaInput label="Notes" name="notes" onChange={(event) => setDocumentNotes(event.target.value)} value={documentNotes} />
          </div>
          {uploadPlan?.documents?.length ? (
            <div className="mt-5 rounded-lg border border-gray-200 p-3 text-sm dark:border-gray-800">
              <h3 className="mb-2 font-semibold text-gray-800 dark:text-gray-100">Uploaded Documents</h3>
              <div className="space-y-2">
                {uploadPlan.documents.map((document) => (
                  <div className="flex items-center justify-between gap-3 text-xs text-gray-600 dark:text-gray-300" key={document.id}>
                    <span>{document.file_name}</span>
                    <Badge value={document.document_status} />
                  </div>
                ))}
              </div>
            </div>
          ) : null}
          <div className="mt-7 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
            <button className="inline-flex h-11 items-center justify-center rounded-lg border border-gray-300 bg-white px-5 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200" onClick={() => setUploadPlan(null)} type="button">
              Cancel
            </button>
            <button className="inline-flex h-11 items-center justify-center gap-2 rounded-lg bg-brand-500 px-5 text-sm font-semibold text-white transition-colors hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-60" disabled={uploading} type="submit">
              <Icon name="upload" className="h-4 w-4" />
              {uploading ? "Uploading..." : "Upload"}
            </button>
          </div>
        </form>
      </Modal>

      <ConfirmModal
        confirmText="Delete"
        isDestructive
        isLoading={deleting}
        isOpen={Boolean(planToDelete)}
        message={`Delete contractor plan "${planToDelete?.work_title || ""}"?`}
        onClose={() => setPlanToDelete(null)}
        onConfirm={() => void deletePlan()}
        title="Delete Contractor Plan"
      />
    </div>
  );
}

function MetricCard({ label, value, tone }: { label: string; value: number; tone: "gray" | "green" | "amber" | "red" | "blue" }) {
  const toneClass = {
    amber: "border-warning-500/20 bg-warning-50 text-warning-700 dark:border-warning-500/30 dark:bg-warning-500/10 dark:text-warning-400",
    blue: "border-blue-light-500/20 bg-blue-light-50 text-blue-light-700 dark:border-blue-light-500/30 dark:bg-blue-light-500/10 dark:text-blue-light-500",
    gray: "border-gray-200 bg-white text-gray-900 dark:border-gray-800 dark:bg-gray-900 dark:text-white",
    green: "border-success-500/20 bg-success-50 text-success-700 dark:border-success-500/30 dark:bg-success-500/10 dark:text-success-400",
    red: "border-error-500/20 bg-error-50 text-error-700 dark:border-error-500/30 dark:bg-error-500/10 dark:text-error-400",
  }[tone];

  return (
    <section className={`rounded-lg border p-4 shadow-theme-sm ${toneClass}`}>
      <p className="text-xs font-semibold uppercase tracking-wide opacity-80">{label}</p>
      <p className="mt-2 text-2xl font-semibold">{formatNumber(value)}</p>
    </section>
  );
}

function RiskPill({ highRisk, label }: { highRisk: boolean; label: string }) {
  return (
    <span className={`inline-flex rounded-full border px-2 py-0.5 text-xs font-semibold ${highRisk ? "border-error-500/20 bg-error-50 text-error-700 dark:border-error-500/30 dark:bg-error-500/10 dark:text-error-400" : "border-gray-200 bg-gray-50 text-gray-600 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300"}`}>
      {label}
    </span>
  );
}

function DateFilterInput({ onChange, placeholder, value }: { onChange: (value: string) => void; placeholder: string; value: string }) {
  return (
    <DatePicker
      className="control-input min-w-[118px] px-3 pr-10"
      dateFormat="Y-m-d"
      defaultDate={value || undefined}
      id={`contractor-filter-${placeholder.toLowerCase().replace(/\s+/g, "-")}`}
      onChange={([date]) => onChange(date ? formatDateFilterValue(date) : "")}
      placeholder={placeholder}
    />
  );
}

function formatDateFilterValue(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function IconButton({
  destructive,
  disabled,
  label,
  name,
  onClick,
  text,
}: {
  destructive?: boolean;
  disabled?: boolean;
  label: string;
  name: "activity" | "edit" | "trash" | "upload" | "wrench";
  onClick: () => void;
  text?: string;
}) {
  return (
    <button
      aria-label={label}
      className={`inline-flex h-9 items-center justify-center rounded-lg border text-sm transition-colors disabled:cursor-not-allowed disabled:opacity-45 ${
        text ? "min-w-[92px] gap-2 px-3 whitespace-nowrap" : "w-9"
      } ${
        destructive
          ? "border-error-500/20 text-error-600 hover:bg-error-50 dark:border-error-500/30 dark:text-error-400 dark:hover:bg-error-500/10"
          : "border-gray-200 text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-white/[0.04] dark:hover:text-white"
      }`}
      disabled={disabled}
      onClick={onClick}
      title={label}
      type="button"
    >
      <Icon name={name} className="h-4 w-4" />
      {text ? <span className="text-xs font-semibold">{text}</span> : null}
    </button>
  );
}
