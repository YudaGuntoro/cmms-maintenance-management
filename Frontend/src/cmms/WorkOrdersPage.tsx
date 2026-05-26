"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import { ConfirmModal, Modal } from "@/components/ui/modal";
import { apiDelete, apiGet, apiPatch, apiPost, apiPut, buildQuery } from "./api";
import { Icon } from "./icons";
import { formatDateTime, formatNumber, nowDateTimeLocal, toDateTimeLocalInput } from "./format";
import { options, type SelectOption } from "./options";
import { Badge, Card, CardHeader, DataTableFooter, EntriesSelect, Feedback, InlineSearch, InlineSelect, SelectInput, TablePanel, TextareaInput, TextInput } from "./ui";
import type { Asset, FailureCode, ProblemReport, RootCause, Sparepart, Technician, WorkOrder } from "./types";
import { WorkOrderDetailModal } from "./WorkOrderDetailModal";

type WorkOrderForm = {
  wo_number: string;
  asset_id: string;
  problem_report_id: string;
  title: string;
  description: string;
  maintenance_type: string;
  priority: string;
  status: string;
  reported_by: string;
  assigned_to: string;
  reported_at: string;
  scheduled_at: string;
  failure_code: string;
  root_cause: string;
  action_taken: string;
  result: string;
};

type WorkOrderFilters = {
  asset_id: string;
  status: string;
  priority: string;
  maintenance_type: string;
};

type WorkOrderActionType = "assign" | "complete" | "close" | "sparepart";

type WorkOrderActionForm = {
  assigned_to: string;
  scheduled_at: string;
  sparepart_id: string;
  qty_used: string;
  failure_code: string;
  root_cause: string;
  action_taken: string;
  result: string;
};

type MasterLookup = {
  id: number;
  code: string;
  name: string;
  is_active?: boolean;
};

type WorkOrderMasterLookups = {
  maintenanceTypes: MasterLookup[];
  workOrderPriorities: MasterLookup[];
  workOrderStatuses: MasterLookup[];
};

const openWorkOrderStatuses = new Set(["OPEN", "ASSIGNED", "IN_PROGRESS", "PENDING"]);
const workOrderViewOptions: SelectOption[] = [{ label: "Open backlog", value: "open" }];

const maintenanceTypeFallbackIds: Record<string, number> = {
  PREVENTIVE: 1,
  CORRECTIVE: 2,
  BREAKDOWN: 3,
  PREDICTIVE: 4,
  INSPECTION: 5,
  CONTRACTOR_SUPERVISION: 6,
};

const workOrderPriorityFallbackIds: Record<string, number> = {
  LOW: 1,
  MEDIUM: 2,
  HIGH: 3,
  URGENT: 4,
};

const workOrderStatusFallbackIds: Record<string, number> = {
  OPEN: 1,
  ASSIGNED: 2,
  IN_PROGRESS: 3,
  PENDING: 4,
  DRAFT: 5,
  COMPLETED: 6,
  CLOSED: 7,
  CANCELLED: 8,
};

const emptyForm: WorkOrderForm = {
  wo_number: "",
  asset_id: "",
  problem_report_id: "",
  title: "",
  description: "",
  maintenance_type: "",
  priority: "",
  status: "",
  reported_by: "",
  assigned_to: "",
  reported_at: "",
  scheduled_at: "",
  failure_code: "",
  root_cause: "",
  action_taken: "",
  result: "",
};

const emptyActionForm: WorkOrderActionForm = {
  assigned_to: "",
  scheduled_at: "",
  sparepart_id: "",
  qty_used: "1",
  failure_code: "",
  root_cause: "",
  action_taken: "",
  result: "",
};

function maintenanceTypeFromReport(report: ProblemReport) {
  if (report.category === "DOWNTIME" || report.category === "BREAKDOWN") {
    return "BREAKDOWN";
  }

  return "CORRECTIVE";
}

function filtersFromSearchParams(searchParams: Pick<URLSearchParams, "get">): WorkOrderFilters {
  return {
    asset_id: searchParams.get("asset_id") || "",
    status: searchParams.get("status") || "",
    priority: searchParams.get("priority") || "",
    maintenance_type: searchParams.get("maintenance_type") || "",
  };
}

function viewFromSearchParams(searchParams: Pick<URLSearchParams, "get">) {
  return searchParams.get("view") === "open" ? "open" : "";
}

function toForm(workOrder?: WorkOrder): WorkOrderForm {
  if (!workOrder) {
    return emptyForm;
  }

  return {
    wo_number: workOrder.wo_number || "",
    asset_id: String(workOrder.asset_id || ""),
    problem_report_id: workOrder.problem_report_id ? String(workOrder.problem_report_id) : "",
    title: workOrder.title || "",
    description: workOrder.description || "",
    maintenance_type: workOrder.maintenance_type || "CORRECTIVE",
    priority: workOrder.priority || "MEDIUM",
    status: workOrder.status || "OPEN",
    reported_by: workOrder.reported_by || "",
    assigned_to: workOrder.assigned_to ? String(workOrder.assigned_to) : "",
    reported_at: toDateTimeLocalInput(workOrder.reported_at),
    scheduled_at: toDateTimeLocalInput(workOrder.scheduled_at),
    failure_code: workOrder.failure_code || "",
    root_cause: workOrder.root_cause || "",
    action_taken: workOrder.action_taken || "",
    result: workOrder.result || "",
  };
}

function masterOptions(items: MasterLookup[], fallback: SelectOption[]) {
  const activeItems = items.filter((item) => item.is_active !== false);
  if (!activeItems.length) {
    return fallback;
  }

  return activeItems.map((item) => ({
    label: item.name || item.code,
    value: item.code,
  }));
}

function masterId(items: MasterLookup[], code: string, fallbackIds: Record<string, number>) {
  return items.find((item) => item.code === code)?.id ?? fallbackIds[code] ?? null;
}

function payload(form: WorkOrderForm, masterLookups: WorkOrderMasterLookups) {
  return {
    wo_number: form.wo_number || "",
    asset_id: Number(form.asset_id),
    problem_report_id: form.problem_report_id ? Number(form.problem_report_id) : null,
    title: form.title,
    description: form.description || null,
    maintenance_type_id: masterId(masterLookups.maintenanceTypes, form.maintenance_type, maintenanceTypeFallbackIds),
    maintenance_type: form.maintenance_type,
    priority_id: masterId(masterLookups.workOrderPriorities, form.priority, workOrderPriorityFallbackIds),
    priority: form.priority,
    status_id: masterId(masterLookups.workOrderStatuses, form.status, workOrderStatusFallbackIds),
    status: form.status,
    reported_by: form.reported_by || null,
    assigned_to: form.assigned_to ? Number(form.assigned_to) : null,
    reported_at: form.reported_at || null,
    scheduled_at: form.scheduled_at || null,
    failure_code: form.failure_code || null,
    root_cause: form.root_cause || null,
    action_taken: form.action_taken || null,
    result: form.result || null,
  };
}

function validateWorkOrderForm(form: WorkOrderForm, masterLookups: WorkOrderMasterLookups) {
  const missing = [
    { label: "Asset", value: form.asset_id },
    { label: "Title", value: form.title },
    { label: "Type", value: form.maintenance_type },
    { label: "Priority", value: form.priority },
    { label: "Status", value: form.status },
  ]
    .filter((item) => !String(item.value || "").trim())
    .map((item) => item.label);

  if (missing.length) {
    return `${missing.join(", ")} wajib diisi.`;
  }

  const invalidMaster = [
    { label: "Type", id: masterId(masterLookups.maintenanceTypes, form.maintenance_type, maintenanceTypeFallbackIds) },
    { label: "Priority", id: masterId(masterLookups.workOrderPriorities, form.priority, workOrderPriorityFallbackIds) },
    { label: "Status", id: masterId(masterLookups.workOrderStatuses, form.status, workOrderStatusFallbackIds) },
  ].find((item) => !item.id);

  return invalidMaster ? `${invalidMaster.label} tidak ditemukan di master data.` : null;
}

function actionModalTitle(type: WorkOrderActionType | null) {
  if (type === "assign") return "Assign Technician";
  if (type === "complete") return "Complete Work Order";
  if (type === "close") return "Close Work Order";
  return "Record Sparepart Usage";
}

function actionModalDescription(type: WorkOrderActionType | null) {
  if (type === "assign") return "Pilih technician yang akan mengerjakan work order ini.";
  if (type === "complete") return "Lengkapi hasil perbaikan sebelum status menjadi completed.";
  if (type === "close") return "Review penyelesaian work order sebelum ditutup.";
  return "Catat sparepart yang digunakan untuk work order ini.";
}

function actionSubmitLabel(type: WorkOrderActionType | null) {
  if (type === "assign") return "Assign";
  if (type === "complete") return "Complete";
  if (type === "close") return "Close";
  return "Save Usage";
}

export default function WorkOrdersPage() {
  const searchParams = useSearchParams();
  const queryKey = searchParams.toString();
  const [workOrders, setWorkOrders] = useState<WorkOrder[]>([]);
  const [problemReports, setProblemReports] = useState<ProblemReport[]>([]);
  const [assets, setAssets] = useState<Asset[]>([]);
  const [technicians, setTechnicians] = useState<Technician[]>([]);
  const [spareparts, setSpareparts] = useState<Sparepart[]>([]);
  const [failureCodes, setFailureCodes] = useState<FailureCode[]>([]);
  const [rootCauses, setRootCauses] = useState<RootCause[]>([]);
  const [maintenanceTypes, setMaintenanceTypes] = useState<MasterLookup[]>([]);
  const [workOrderPriorities, setWorkOrderPriorities] = useState<MasterLookup[]>([]);
  const [workOrderStatuses, setWorkOrderStatuses] = useState<MasterLookup[]>([]);
  const [filters, setFilters] = useState<WorkOrderFilters>(() => filtersFromSearchParams(searchParams));
  const [quickView, setQuickView] = useState(() => viewFromSearchParams(searchParams));
  const [search, setSearch] = useState("");
  const [form, setForm] = useState<WorkOrderForm>(emptyForm);
  const [editing, setEditing] = useState<WorkOrder | null>(null);
  const [actionType, setActionType] = useState<WorkOrderActionType | null>(null);
  const [actionWorkOrder, setActionWorkOrder] = useState<WorkOrder | null>(null);
  const [detailWorkOrder, setDetailWorkOrder] = useState<WorkOrder | null>(null);
  const [actionForm, setActionForm] = useState<WorkOrderActionForm>(emptyActionForm);
  const [workOrderToDelete, setWorkOrderToDelete] = useState<WorkOrder | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const assetOptions = useMemo<SelectOption[]>(() => assets.map((asset) => ({ label: `${asset.asset_code} - ${asset.asset_name}`, value: String(asset.id) })), [assets]);
  const problemReportOptions = useMemo<SelectOption[]>(() => {
    return problemReports
      .filter((report) => {
        const linkedWorkOrder = workOrders.find((workOrder) => workOrder.problem_report_id === report.id);
        return !linkedWorkOrder || linkedWorkOrder.id === editing?.id;
      })
      .map((report) => ({ label: `${report.report_number} - ${report.title}`, value: String(report.id) }));
  }, [editing?.id, problemReports, workOrders]);
  const technicianOptions = useMemo<SelectOption[]>(() => technicians.map((tech) => ({ label: `${tech.employee_no} - ${tech.name}`, value: String(tech.id) })), [technicians]);
  const sparepartOptions = useMemo<SelectOption[]>(() => spareparts.map((part) => ({ label: `${part.part_code} - ${part.part_name} (${formatNumber(part.stock_qty)} ${part.unit})`, value: String(part.id) })), [spareparts]);
  const failureOptions = useMemo<SelectOption[]>(() => failureCodes.map((item) => ({ label: `${item.code} - ${item.name}`, value: item.code })), [failureCodes]);
  const rootCauseOptions = useMemo<SelectOption[]>(() => rootCauses.map((item) => ({ label: `${item.code} - ${item.name}`, value: item.code })), [rootCauses]);
  const maintenanceTypeOptions = useMemo<SelectOption[]>(() => masterOptions(maintenanceTypes, options.maintenanceType), [maintenanceTypes]);
  const workOrderPriorityOptions = useMemo<SelectOption[]>(() => masterOptions(workOrderPriorities, options.workOrderPriority), [workOrderPriorities]);
  const workOrderStatusOptions = useMemo<SelectOption[]>(() => masterOptions(workOrderStatuses, options.workOrderStatus), [workOrderStatuses]);
  const masterLookups = useMemo<WorkOrderMasterLookups>(() => ({
    maintenanceTypes,
    workOrderPriorities,
    workOrderStatuses,
  }), [maintenanceTypes, workOrderPriorities, workOrderStatuses]);

  useEffect(() => {
    const params = new URLSearchParams(queryKey);
    setFilters(filtersFromSearchParams(params));
    setQuickView(viewFromSearchParams(params));
  }, [queryKey]);

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
      const [
        workOrderData,
        reportData,
        assetData,
        technicianData,
        sparepartData,
        failureData,
        rootCauseData,
        maintenanceTypeData,
        priorityData,
        statusData,
      ] = await Promise.all([
        apiGet<WorkOrder[]>(`/api/work-orders${buildQuery(filters)}`),
        apiGet<ProblemReport[]>("/api/problem-reports"),
        apiGet<Asset[]>("/api/assets"),
        apiGet<Technician[]>("/api/technicians?status=ACTIVE"),
        apiGet<Sparepart[]>("/api/spareparts"),
        apiGet<FailureCode[]>("/api/failure-codes"),
        apiGet<RootCause[]>("/api/root-causes"),
        apiGet<MasterLookup[]>("/api/maintenance-types"),
        apiGet<MasterLookup[]>("/api/work-order-priorities"),
        apiGet<MasterLookup[]>("/api/work-order-statuses"),
      ]);
      setWorkOrders(workOrderData || []);
      setProblemReports(reportData || []);
      setAssets(assetData || []);
      setTechnicians(technicianData || []);
      setSpareparts(sparepartData || []);
      setFailureCodes(failureData || []);
      setRootCauses(rootCauseData || []);
      setMaintenanceTypes(maintenanceTypeData || []);
      setWorkOrderPriorities(priorityData || []);
      setWorkOrderStatuses(statusData || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load work orders.");
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const searchedWorkOrders = useMemo(() => {
    const viewFilteredWorkOrders = quickView === "open"
      ? workOrders.filter((workOrder) => openWorkOrderStatuses.has(workOrder.status))
      : workOrders;
    const normalized = search.trim().toLowerCase();
    if (!normalized) {
      return viewFilteredWorkOrders;
    }

    return viewFilteredWorkOrders.filter((workOrder) => [
      workOrder.wo_number,
      workOrder.title,
      workOrder.priority,
      workOrder.status,
      workOrder.maintenance_type,
      workOrder.action_taken,
      workOrder.result,
      problemReportName(workOrder.problem_report_id),
      assetName(workOrder.asset_id),
      technicianName(workOrder.assigned_to),
    ].some((value) => String(value || "").toLowerCase().includes(normalized)));
  }, [assetName, problemReportName, quickView, search, technicianName, workOrders]);

  useEffect(() => {
    setPage(1);
  }, [filters, pageSize, quickView, search, searchedWorkOrders.length]);

  const visibleWorkOrders = useMemo(() => {
    const start = (page - 1) * pageSize;
    return searchedWorkOrders.slice(start, start + pageSize);
  }, [page, pageSize, searchedWorkOrders]);

  function openCreate() {
    setEditing(null);
    setForm({ ...emptyForm });
    setFormOpen(true);
  }

  function openEdit(workOrder: WorkOrder) {
    setEditing(workOrder);
    setForm(toForm(workOrder));
    setFormOpen(true);
  }

  function selectProblemReport(reportId: string) {
    const report = problemReports.find((item) => String(item.id) === reportId);
    setForm((current) => {
      if (!report) {
        return { ...current, problem_report_id: reportId };
      }

      return {
        ...current,
        problem_report_id: reportId,
        asset_id: String(report.asset_id),
        title: current.title || report.title,
        description: current.description || report.description || "",
        maintenance_type: maintenanceTypeFromReport(report),
        priority: report.priority,
        reported_by: current.reported_by || report.reported_by || "",
        reported_at: current.reported_at || toDateTimeLocalInput(report.reported_at),
      };
    });
  }

  function openActionModal(type: WorkOrderActionType, workOrder: WorkOrder) {
    setActionType(type);
    setActionWorkOrder(workOrder);
    setActionForm({
      assigned_to: workOrder.assigned_to ? String(workOrder.assigned_to) : "",
      scheduled_at: toDateTimeLocalInput(workOrder.scheduled_at),
      sparepart_id: "",
      qty_used: "1",
      failure_code: workOrder.failure_code || "",
      root_cause: workOrder.root_cause || "",
      action_taken: workOrder.action_taken || "",
      result: workOrder.result || "",
    });
    setError(null);
    setSuccess(null);
  }

  function closeActionModal(force = false) {
    if (saving && !force) {
      return;
    }

    setActionType(null);
    setActionWorkOrder(null);
    setActionForm(emptyActionForm);
  }

  async function saveWorkOrder(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSuccess(null);

    const validationMessage = validateWorkOrderForm(form, masterLookups);
    if (validationMessage) {
      setError(validationMessage);
      return;
    }

    setSaving(true);
    try {
      if (editing) {
        await apiPut<WorkOrder>(`/api/work-orders/${editing.id}`, payload(form, masterLookups));
        setSuccess("Work order updated successfully.");
      } else {
        await apiPost<WorkOrder>("/api/work-orders", payload(form, masterLookups));
        setSuccess("Work order created successfully.");
      }
      setFormOpen(false);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save work order.");
    } finally {
      setSaving(false);
    }
  }

  async function startWorkOrder(workOrder: WorkOrder) {
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      await apiPatch<WorkOrder>(`/api/work-orders/${workOrder.id}/start`);
      setSuccess("Work order action processed.");
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to process work order action.");
    } finally {
      setSaving(false);
    }
  }

  async function submitActionModal(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!actionType || !actionWorkOrder) {
      return;
    }

    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      if (actionType === "assign") {
        if (!actionForm.assigned_to) {
          throw new Error("Technician wajib dipilih.");
        }

        await apiPatch<WorkOrder>(`/api/work-orders/${actionWorkOrder.id}/assign`, {
          assigned_to: Number(actionForm.assigned_to),
          scheduled_at: actionForm.scheduled_at || actionWorkOrder.scheduled_at || null,
        });
      }

      if (actionType === "complete") {
        await apiPatch<WorkOrder>(`/api/work-orders/${actionWorkOrder.id}/complete`, {
          completed_at: new Date().toISOString(),
          repair_start: actionWorkOrder.repair_start || actionWorkOrder.started_at || new Date().toISOString(),
          repair_end: new Date().toISOString(),
          failure_code: actionForm.failure_code || actionWorkOrder.failure_code || null,
          root_cause: actionForm.root_cause || actionWorkOrder.root_cause || null,
          action_taken: actionForm.action_taken || actionWorkOrder.action_taken || null,
          result: actionForm.result || actionWorkOrder.result || null,
        });
      }

      if (actionType === "close") {
        await apiPatch<WorkOrder>(`/api/work-orders/${actionWorkOrder.id}/close`, {
          closed_at: new Date().toISOString(),
          failure_code: actionForm.failure_code || actionWorkOrder.failure_code || null,
          root_cause: actionForm.root_cause || actionWorkOrder.root_cause || null,
          action_taken: actionForm.action_taken || actionWorkOrder.action_taken || null,
          result: actionForm.result || actionWorkOrder.result || null,
        });
      }

      if (actionType === "sparepart") {
        if (!actionForm.sparepart_id || !actionForm.qty_used) {
          throw new Error("Sparepart dan qty wajib diisi.");
        }

        await apiPost(`/api/work-orders/${actionWorkOrder.id}/spareparts`, {
          sparepart_id: Number(actionForm.sparepart_id),
          qty_used: Number(actionForm.qty_used),
          used_at: new Date().toISOString(),
        });
      }

      closeActionModal(true);
      setSuccess("Work order action processed.");
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to process work order action.");
    } finally {
      setSaving(false);
    }
  }

  function closeDeleteModal() {
    if (deleting) {
      return;
    }

    setWorkOrderToDelete(null);
  }

  async function confirmDeleteWorkOrder() {
    if (!workOrderToDelete) {
      return;
    }

    setDeleting(true);
    setError(null);
    setSuccess(null);
    try {
      await apiDelete<string>(`/api/work-orders/${workOrderToDelete.id}`);
      setSuccess("Work order deleted successfully.");
      setWorkOrderToDelete(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete work order.");
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Operations</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">Work Orders</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Corrective, breakdown, preventive, predictive, and inspection jobs</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <button className="secondary-button" disabled={loading} onClick={() => void loadData()} type="button"><Icon name="refresh" />Refresh</button>
        </div>
      </div>

      <Feedback error={error} success={success} />

      {formOpen ? (
        <Card>
          <CardHeader action={<button className="icon-button" onClick={() => setFormOpen(false)} type="button"><Icon name="x" /></button>} description={editing ? "Update existing work order" : "Create new work order"} title={editing ? "Edit Work Order" : "Add Work Order"} />
          <form className="grid grid-cols-1 gap-4 p-5 md:grid-cols-2 xl:grid-cols-3" onSubmit={(event) => void saveWorkOrder(event)}>
            <TextInput helper="Leave empty to generate automatically" label="WO Number" name="wo_number" onChange={(event) => setForm((current) => ({ ...current, wo_number: event.target.value }))} value={form.wo_number} />
            <SelectInput label="Asset" name="asset_id" onChange={(event) => setForm((current) => ({ ...current, asset_id: event.target.value }))} options={assetOptions} placeholder="Select Asset" required value={form.asset_id} />
            <SelectInput label="Report Reference" name="problem_report_id" onChange={(event) => selectProblemReport(event.target.value)} options={problemReportOptions} value={form.problem_report_id} />
            <TextInput label="Title" name="title" onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))} required value={form.title} />
            <SelectInput label="Type" name="maintenance_type" onChange={(event) => setForm((current) => ({ ...current, maintenance_type: event.target.value }))} options={maintenanceTypeOptions} required value={form.maintenance_type} />
            <SelectInput label="Priority" name="priority" onChange={(event) => setForm((current) => ({ ...current, priority: event.target.value }))} options={workOrderPriorityOptions} required value={form.priority} />
            <SelectInput label="Status" name="status" onChange={(event) => setForm((current) => ({ ...current, status: event.target.value }))} options={workOrderStatusOptions} required value={form.status} />
            <TextInput label="Reported By" name="reported_by" onChange={(event) => setForm((current) => ({ ...current, reported_by: event.target.value }))} value={form.reported_by} />
            <SelectInput label="Assigned To" name="assigned_to" onChange={(event) => setForm((current) => ({ ...current, assigned_to: event.target.value }))} options={technicianOptions} value={form.assigned_to} />
            <TextInput label="Reported At" name="reported_at" onChange={(event) => setForm((current) => ({ ...current, reported_at: event.target.value }))} type="datetime-local" value={form.reported_at || nowDateTimeLocal()} />
            <TextInput label="Scheduled At" name="scheduled_at" onChange={(event) => setForm((current) => ({ ...current, scheduled_at: event.target.value }))} type="datetime-local" value={form.scheduled_at} />
            <SelectInput label="Failure Code" name="failure_code" onChange={(event) => setForm((current) => ({ ...current, failure_code: event.target.value }))} options={failureOptions} value={form.failure_code} />
            <SelectInput label="Root Cause" name="root_cause" onChange={(event) => setForm((current) => ({ ...current, root_cause: event.target.value }))} options={rootCauseOptions} value={form.root_cause} />
            <div className="md:col-span-2 xl:col-span-3"><TextareaInput label="Description" name="description" onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} value={form.description} /></div>
            <div className="md:col-span-2 xl:col-span-3"><TextareaInput label="Action Taken" name="action_taken" onChange={(event) => setForm((current) => ({ ...current, action_taken: event.target.value }))} value={form.action_taken} /></div>
            <div className="md:col-span-2 xl:col-span-3"><TextareaInput label="Result" name="result" onChange={(event) => setForm((current) => ({ ...current, result: event.target.value }))} value={form.result} /></div>
            <div className="flex flex-wrap items-center gap-2 md:col-span-2 xl:col-span-3"><button className="primary-button" disabled={saving} type="submit"><Icon name="check" />Save</button><button className="secondary-button" onClick={() => setFormOpen(false)} type="button">Cancel</button></div>
          </form>
        </Card>
      ) : null}

      <TablePanel
        footer={<DataTableFooter onPageChange={setPage} page={page} pageSize={pageSize} total={searchedWorkOrders.length} />}
        toolbarLeft={(
          <>
            <EntriesSelect onChange={(value) => { setPageSize(value); setPage(1); }} value={pageSize} />
            <InlineSelect onChange={setQuickView} options={workOrderViewOptions} placeholder="All Views" value={quickView} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, asset_id: value }))} options={assetOptions} placeholder="All Assets" value={filters.asset_id} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, status: value }))} options={workOrderStatusOptions} placeholder="All Status" value={filters.status} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, priority: value }))} options={workOrderPriorityOptions} placeholder="All Priority" value={filters.priority} />
            <button className="primary-button" onClick={openCreate} type="button"><Icon name="plus" />Create</button>
          </>
        )}
        toolbarRight={<InlineSearch onChange={setSearch} placeholder="Search work orders" value={search} />}
      >
          <table className="min-w-full">
            <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
              <tr>
                <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">WO Number</th>
                <th className="min-w-64 px-5 py-3 text-left text-theme-xs font-semibold">Title</th>
                <th className="min-w-60 px-5 py-3 text-left text-theme-xs font-semibold">Asset</th>
                <th className="min-w-56 px-5 py-3 text-left text-theme-xs font-semibold">Reference</th>
                <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Type</th>
                <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Priority</th>
                <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Status</th>
                <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Technician</th>
                <th className="min-w-72 px-5 py-3 text-left text-theme-xs font-semibold">Action Taken</th>
                <th className="min-w-72 px-5 py-3 text-left text-theme-xs font-semibold">Result</th>
                <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">Reported</th>
                <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Repair</th>
                <th className="min-w-72 px-5 py-3 text-center text-theme-xs font-semibold">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
              {loading ? (
                Array.from({ length: 5 }).map((_, index) => <tr key={index}>{Array.from({ length: 13 }).map((__, cellIndex) => <td className="px-5 py-4" key={cellIndex}><div className="h-4 w-full animate-pulse rounded-md bg-gray-100 dark:bg-white/[0.05]" /></td>)}</tr>)
              ) : searchedWorkOrders.length ? (
                visibleWorkOrders.map((workOrder) => (
                  <tr className="cursor-pointer hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={workOrder.id} onClick={() => setDetailWorkOrder(workOrder)}>
                    <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{workOrder.wo_number}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{workOrder.title}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{assetName(workOrder.asset_id)}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300"><span className="block max-w-[220px] truncate">{problemReportName(workOrder.problem_report_id)}</span></td>
                    <td className="px-5 py-4"><Badge value={workOrder.maintenance_type} /></td>
                    <td className="px-5 py-4"><Badge value={workOrder.priority} /></td>
                    <td className="px-5 py-4"><Badge value={workOrder.status} /></td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{technicianName(workOrder.assigned_to)}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300"><span className="block max-w-[280px] truncate">{workOrder.action_taken || "-"}</span></td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300"><span className="block max-w-[280px] truncate">{workOrder.result || "-"}</span></td>
                    <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">{formatDateTime(workOrder.reported_at)}</td>
                    <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{formatNumber(workOrder.repair_minutes)} min</td>
                    <td className="px-5 py-4"><div className="flex flex-wrap justify-center gap-2">
                      {workOrder.status === "OPEN" ? <button className="secondary-button h-9 px-3" disabled={saving} onClick={(event) => { event.stopPropagation(); openActionModal("assign", workOrder); }} type="button">Assign</button> : null}
                      {workOrder.status === "ASSIGNED" ? <button className="secondary-button h-9 px-3" disabled={saving} onClick={(event) => { event.stopPropagation(); void startWorkOrder(workOrder); }} type="button">Start</button> : null}
                      {workOrder.status === "IN_PROGRESS" ? <button className="secondary-button h-9 px-3" disabled={saving} onClick={(event) => { event.stopPropagation(); openActionModal("complete", workOrder); }} type="button">Complete</button> : null}
                      {workOrder.status === "COMPLETED" ? <button className="secondary-button h-9 px-3" disabled={saving} onClick={(event) => { event.stopPropagation(); openActionModal("close", workOrder); }} type="button">Close</button> : null}
                      <button aria-label="Use sparepart" className="text-blue-light-500 transition-colors hover:text-blue-light-600" onClick={(event) => { event.stopPropagation(); openActionModal("sparepart", workOrder); }} type="button"><Icon className="h-5 w-5" name="box" /></button>
                      <button aria-label="Edit" className="text-warning-500 transition-colors hover:text-warning-600 dark:text-warning-400 dark:hover:text-warning-500" onClick={(event) => { event.stopPropagation(); openEdit(workOrder); }} type="button"><Icon className="h-5 w-5" name="edit" /></button>
                      <button aria-label="Delete" className="text-error-500 transition-colors hover:text-error-600 dark:text-error-400 dark:hover:text-error-500" onClick={(event) => { event.stopPropagation(); setWorkOrderToDelete(workOrder); }} type="button"><Icon className="h-5 w-5" name="trash" /></button>
                    </div></td>
                  </tr>
                ))
              ) : (
                <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={13}>No work orders found.</td></tr>
              )}
            </tbody>
          </table>
      </TablePanel>

      <Modal isOpen={Boolean(actionType && actionWorkOrder)} onClose={closeActionModal} className="mx-4 max-w-[680px] overflow-hidden p-0" showCloseButton={false}>
        <div className="border-b border-gray-100 px-6 py-5 dark:border-white/[0.05]">
          <div className="flex items-start gap-4">
            <div className="flex size-12 shrink-0 items-center justify-center rounded-lg bg-brand-500/10 text-brand-500 ring-1 ring-brand-500/20">
              <Icon className="size-6" name={actionType === "sparepart" ? "box" : actionType === "assign" ? "wrench" : "check"} />
            </div>
            <div className="min-w-0">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white/90">{actionModalTitle(actionType)}</h3>
              <p className="mt-1 text-sm leading-6 text-gray-500 dark:text-gray-400">{actionModalDescription(actionType)}</p>
              {actionWorkOrder ? (
                <div className="mt-3 rounded-lg border border-gray-100 bg-gray-50 px-3 py-2 dark:border-white/[0.05] dark:bg-white/[0.03]">
                  <p className="text-sm font-semibold text-gray-900 dark:text-white">{actionWorkOrder.wo_number}</p>
                  <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{actionWorkOrder.title}</p>
                </div>
              ) : null}
            </div>
          </div>
        </div>

        <form onSubmit={(event) => void submitActionModal(event)}>
          <div className="space-y-5 px-6 py-5">
            {actionType === "assign" ? (
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <SelectInput
                  label="Technician"
                  name="assigned_to"
                  onChange={(event) => setActionForm((current) => ({ ...current, assigned_to: event.target.value }))}
                  options={technicianOptions}
                  placeholder="Select technician"
                  value={actionForm.assigned_to}
                />
                <TextInput
                  label="Scheduled At"
                  name="scheduled_at"
                  onChange={(event) => setActionForm((current) => ({ ...current, scheduled_at: event.target.value }))}
                  type="datetime-local"
                  value={actionForm.scheduled_at}
                />
              </div>
            ) : null}

            {actionType === "sparepart" ? (
              <div className="grid grid-cols-1 gap-4 md:grid-cols-[1fr_160px]">
                <SelectInput
                  label="Sparepart"
                  name="sparepart_id"
                  onChange={(event) => setActionForm((current) => ({ ...current, sparepart_id: event.target.value }))}
                  options={sparepartOptions}
                  placeholder="Select sparepart"
                  value={actionForm.sparepart_id}
                />
                <TextInput
                  label="Qty Used"
                  name="qty_used"
                  onChange={(event) => setActionForm((current) => ({ ...current, qty_used: event.target.value }))}
                  type="number"
                  value={actionForm.qty_used}
                />
              </div>
            ) : null}

            {actionType === "complete" || actionType === "close" ? (
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
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
              </div>
            ) : null}
          </div>

          <div className="flex flex-col-reverse gap-3 border-t border-gray-100 px-6 py-4 dark:border-white/[0.05] sm:flex-row sm:justify-end">
            <button className="secondary-button h-11" disabled={saving} onClick={() => closeActionModal()} type="button">
              Cancel
            </button>
            <button className="primary-button h-11 min-w-32" disabled={saving} type="submit">
              {saving ? "Processing..." : actionSubmitLabel(actionType)}
            </button>
          </div>
        </form>
      </Modal>

      <ConfirmModal
        confirmText="Delete"
        isDestructive
        isLoading={deleting}
        isOpen={Boolean(workOrderToDelete)}
        message={`Work order "${workOrderToDelete?.wo_number || workOrderToDelete?.title || ""}" akan dihapus permanen.`}
        onClose={closeDeleteModal}
        onConfirm={() => void confirmDeleteWorkOrder()}
        title="Delete Work Order"
      />

      <WorkOrderDetailModal
        assetLabel={detailWorkOrder ? assetName(detailWorkOrder.asset_id) : undefined}
        isOpen={Boolean(detailWorkOrder)}
        onClose={() => setDetailWorkOrder(null)}
        technicianLabel={technicianName(detailWorkOrder?.assigned_to)}
        workOrder={detailWorkOrder}
      />
    </div>
  );
}
