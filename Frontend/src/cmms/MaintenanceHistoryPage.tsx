"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { apiGet } from "./api";
import { formatDateTime, formatNumber, statusText } from "./format";
import { Icon } from "./icons";
import { options, type SelectOption } from "./options";
import { Badge, Card, DataTableFooter, EntriesSelect, Feedback, InlineInput, InlineSearch, InlineSelect, TablePanel } from "./ui";
import type { Asset, DowntimeLog, Technician, WorkOrder } from "./types";

type HistorySource = "WORK_ORDER" | "DOWNTIME";

type HistoryFilters = {
  asset_id: string;
  source: string;
  status: string;
  maintenance_type: string;
  date_from: string;
  date_to: string;
};

type HistoryRecord = {
  key: string;
  source: HistorySource;
  asset_id: number;
  asset_label: string;
  asset_area: string;
  date: string | null;
  end_date?: string | null;
  sort_time: number;
  reference: string;
  title: string;
  status?: string | null;
  priority?: string | null;
  maintenance_type?: string | null;
  category?: string | null;
  downtime_minutes?: number | null;
  repair_minutes?: number | null;
  technician_label?: string | null;
  failure_code?: string | null;
  root_cause?: string | null;
  action_taken?: string | null;
  result?: string | null;
  description?: string | null;
};

const sourceOptions: SelectOption[] = [
  { label: "Work Orders", value: "WORK_ORDER" },
  { label: "Downtime Logs", value: "DOWNTIME" },
];

const emptyFilters: HistoryFilters = {
  asset_id: "",
  source: "",
  status: "",
  maintenance_type: "",
  date_from: "",
  date_to: "",
};

function toTime(value?: string | null) {
  if (!value) {
    return 0;
  }

  const time = new Date(value).getTime();
  return Number.isNaN(time) ? 0 : time;
}

function startOfDate(value: string) {
  const time = new Date(`${value}T00:00:00`).getTime();
  return Number.isNaN(time) ? 0 : time;
}

function endOfDate(value: string) {
  const time = new Date(`${value}T23:59:59.999`).getTime();
  return Number.isNaN(time) ? 0 : time;
}

function workOrderHistoryDate(workOrder: WorkOrder) {
  return workOrder.closed_at
    || workOrder.completed_at
    || workOrder.started_at
    || workOrder.scheduled_at
    || workOrder.reported_at
    || workOrder.created_at
    || null;
}

function minutesLabel(value?: number | null) {
  return value === null || value === undefined ? "-" : `${formatNumber(value)} min`;
}

function compactText(values: Array<string | null | undefined>) {
  return values.filter((value) => value && value.trim()).join(" | ") || "-";
}

function matchesDateRange(record: HistoryRecord, filters: HistoryFilters) {
  if (!filters.date_from && !filters.date_to) {
    return true;
  }

  if (!record.sort_time) {
    return false;
  }

  if (filters.date_from && record.sort_time < startOfDate(filters.date_from)) {
    return false;
  }

  if (filters.date_to && record.sort_time > endOfDate(filters.date_to)) {
    return false;
  }

  return true;
}

function MetricCard({
  icon,
  label,
  value,
  helper,
}: {
  icon: "activity" | "box" | "calendar" | "check" | "wrench";
  label: string;
  value: string | number;
  helper: string;
}) {
  return (
    <Card className="p-5">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-sm font-medium text-gray-500 dark:text-gray-400">{label}</p>
          <p className="mt-3 text-2xl font-semibold text-gray-900 dark:text-white">{value}</p>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">{helper}</p>
        </div>
        <span className="flex size-11 items-center justify-center rounded-lg bg-brand-50 text-brand-500 dark:bg-brand-500/10 dark:text-brand-400">
          <Icon className="size-5" name={icon} />
        </span>
      </div>
    </Card>
  );
}

export default function MaintenanceHistoryPage() {
  const [assets, setAssets] = useState<Asset[]>([]);
  const [workOrders, setWorkOrders] = useState<WorkOrder[]>([]);
  const [downtimeLogs, setDowntimeLogs] = useState<DowntimeLog[]>([]);
  const [technicians, setTechnicians] = useState<Technician[]>([]);
  const [filters, setFilters] = useState<HistoryFilters>(emptyFilters);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const assetOptions = useMemo<SelectOption[]>(() => assets.map((asset) => ({
    label: `${asset.asset_code} - ${asset.asset_name}`,
    value: String(asset.id),
  })), [assets]);

  const assetById = useMemo(() => new Map(assets.map((asset) => [asset.id, asset])), [assets]);
  const technicianById = useMemo(() => new Map(technicians.map((technician) => [technician.id, technician])), [technicians]);

  const assetLabel = useCallback((id: number) => {
    const asset = assetById.get(id);
    return asset ? `${asset.asset_code} - ${asset.asset_name}` : `Asset #${id}`;
  }, [assetById]);

  const assetArea = useCallback((id: number) => {
    const asset = assetById.get(id);
    return [asset?.plant, asset?.area, asset?.production_line].filter(Boolean).join(" / ");
  }, [assetById]);

  const technicianLabel = useCallback((id?: number | null) => {
    if (!id) {
      return "-";
    }

    const technician = technicianById.get(id);
    return technician ? technician.name : `Technician #${id}`;
  }, [technicianById]);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [assetData, workOrderData, downtimeData, technicianData] = await Promise.all([
        apiGet<Asset[]>("/api/assets"),
        apiGet<WorkOrder[]>("/api/work-orders"),
        apiGet<DowntimeLog[]>("/api/downtime-logs"),
        apiGet<Technician[]>("/api/technicians"),
      ]);

      setAssets(assetData || []);
      setWorkOrders(workOrderData || []);
      setDowntimeLogs(downtimeData || []);
      setTechnicians(technicianData || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load maintenance history.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const historyRecords = useMemo<HistoryRecord[]>(() => {
    const workOrderRecords = workOrders.map((workOrder): HistoryRecord => {
      const date = workOrderHistoryDate(workOrder);
      return {
        key: `work-order-${workOrder.id}`,
        source: "WORK_ORDER",
        asset_id: workOrder.asset_id,
        asset_label: assetLabel(workOrder.asset_id),
        asset_area: assetArea(workOrder.asset_id),
        date,
        end_date: workOrder.closed_at || workOrder.completed_at || workOrder.repair_end || null,
        sort_time: toTime(date),
        reference: workOrder.wo_number,
        title: workOrder.title,
        status: workOrder.status,
        priority: workOrder.priority,
        maintenance_type: workOrder.maintenance_type,
        downtime_minutes: workOrder.downtime_minutes,
        repair_minutes: workOrder.repair_minutes,
        technician_label: technicianLabel(workOrder.assigned_to),
        failure_code: workOrder.failure_code,
        root_cause: workOrder.root_cause,
        action_taken: workOrder.action_taken,
        result: workOrder.result,
        description: workOrder.description,
      };
    });

    const downtimeRecords = downtimeLogs.map((record): HistoryRecord => ({
      key: `downtime-${record.id}`,
      source: "DOWNTIME",
      asset_id: record.asset_id,
      asset_label: assetLabel(record.asset_id),
      asset_area: assetArea(record.asset_id),
      date: record.start_time,
      end_date: record.end_time,
      sort_time: toTime(record.start_time),
      reference: record.work_order_id ? `WO #${record.work_order_id}` : `Downtime #${record.id}`,
      title: record.description || "Downtime event",
      category: record.downtime_category,
      downtime_minutes: record.duration_minutes,
      description: record.description,
    }));

    return [...workOrderRecords, ...downtimeRecords].sort((left, right) => right.sort_time - left.sort_time);
  }, [assetArea, assetLabel, downtimeLogs, technicianLabel, workOrders]);

  const filteredRecords = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return historyRecords.filter((record) => {
      if (filters.asset_id && String(record.asset_id) !== filters.asset_id) {
        return false;
      }

      if (filters.source && record.source !== filters.source) {
        return false;
      }

      if (filters.status && record.status !== filters.status) {
        return false;
      }

      if (filters.maintenance_type && record.maintenance_type !== filters.maintenance_type) {
        return false;
      }

      if (!matchesDateRange(record, filters)) {
        return false;
      }

      if (!normalizedSearch) {
        return true;
      }

      return [
        record.asset_label,
        record.asset_area,
        record.reference,
        record.title,
        record.status,
        record.priority,
        record.maintenance_type,
        record.category,
        record.technician_label,
        record.failure_code,
        record.root_cause,
        record.action_taken,
        record.result,
        record.description,
      ].some((value) => String(value || "").toLowerCase().includes(normalizedSearch));
    });
  }, [filters, historyRecords, search]);

  useEffect(() => {
    setPage(1);
  }, [filteredRecords.length, filters, pageSize, search]);

  const visibleRecords = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredRecords.slice(start, start + pageSize);
  }, [filteredRecords, page, pageSize]);

  const summary = useMemo(() => {
    const workOrderRecords = filteredRecords.filter((record) => record.source === "WORK_ORDER");
    const downtimeRecords = filteredRecords.filter((record) => record.source === "DOWNTIME");
    const downtimeSummaryRecords = filters.source === "WORK_ORDER"
      ? workOrderRecords
      : filters.source === "DOWNTIME"
        ? downtimeRecords
        : downtimeRecords.length
          ? downtimeRecords
          : workOrderRecords;

    const totalDowntime = downtimeSummaryRecords.reduce((total, record) => total + (record.downtime_minutes || 0), 0);
    const repairRecords = workOrderRecords.filter((record) => record.repair_minutes !== null && record.repair_minutes !== undefined);
    const totalRepair = repairRecords.reduce((total, record) => total + (record.repair_minutes || 0), 0);
    const completed = workOrderRecords.filter((record) => ["COMPLETED", "CLOSED"].includes(String(record.status))).length;

    return {
      affectedAssets: new Set(filteredRecords.map((record) => record.asset_id)).size,
      averageRepair: repairRecords.length ? Math.round(totalRepair / repairRecords.length) : 0,
      completed,
      downtimeEvents: downtimeRecords.length,
      totalDowntime,
      workOrders: workOrderRecords.length,
    };
  }, [filteredRecords, filters.source]);

  const hasActiveFilter = Object.values(filters).some(Boolean) || Boolean(search.trim());

  function resetFilters() {
    setFilters(emptyFilters);
    setSearch("");
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Operations</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">Maintenance History</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Work order and downtime records by asset</p>
        </div>
        <button className="secondary-button" disabled={loading} onClick={() => void loadData()} type="button">
          <Icon name="refresh" />
          Refresh
        </button>
      </div>

      <Feedback error={error} />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard helper={`${formatNumber(summary.completed)} completed or closed`} icon="wrench" label="Work Orders" value={formatNumber(summary.workOrders)} />
        <MetricCard helper={`${formatNumber(summary.downtimeEvents)} downtime records`} icon="activity" label="Downtime Minutes" value={minutesLabel(summary.totalDowntime)} />
        <MetricCard helper={`${formatNumber(summary.averageRepair)} min average`} icon="check" label="Average Repair" value={minutesLabel(summary.averageRepair)} />
        <MetricCard helper="Assets in current result" icon="box" label="Affected Assets" value={formatNumber(summary.affectedAssets)} />
      </div>

      <TablePanel
        footer={<DataTableFooter onPageChange={setPage} page={page} pageSize={pageSize} total={filteredRecords.length} />}
        toolbarLeft={(
          <>
            <EntriesSelect onChange={(value) => { setPageSize(value); setPage(1); }} value={pageSize} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, source: value }))} options={sourceOptions} placeholder="All Sources" value={filters.source} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, asset_id: value }))} options={assetOptions} placeholder="All Assets" value={filters.asset_id} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, status: value }))} options={options.workOrderStatus} placeholder="All Status" value={filters.status} />
            <InlineSelect onChange={(value) => setFilters((current) => ({ ...current, maintenance_type: value }))} options={options.maintenanceType} placeholder="All Types" value={filters.maintenance_type} />
            <InlineInput label="From" onChange={(value) => setFilters((current) => ({ ...current, date_from: value }))} type="date" value={filters.date_from} />
            <InlineInput label="To" onChange={(value) => setFilters((current) => ({ ...current, date_to: value }))} type="date" value={filters.date_to} />
            {hasActiveFilter ? (
              <button className="secondary-button h-10 px-3" onClick={resetFilters} type="button">
                <Icon className="size-4" name="x" />
                Reset
              </button>
            ) : null}
          </>
        )}
        toolbarRight={<InlineSearch onChange={setSearch} placeholder="Search history" value={search} />}
      >
        <table className="min-w-full">
          <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
            <tr>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Date</th>
              <th className="min-w-72 px-5 py-3 text-left text-theme-xs font-semibold">Asset</th>
              <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Source</th>
              <th className="min-w-72 px-5 py-3 text-left text-theme-xs font-semibold">Reference</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Type</th>
              <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Status</th>
              <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Downtime</th>
              <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Repair</th>
              <th className="min-w-80 px-5 py-3 text-left text-theme-xs font-semibold">Finding / Action</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Technician</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
            {loading ? (
              Array.from({ length: 6 }).map((_, rowIndex) => (
                <tr key={rowIndex}>
                  {Array.from({ length: 10 }).map((__, cellIndex) => (
                    <td className="px-5 py-4" key={cellIndex}>
                      <div className="h-4 w-full animate-pulse rounded-md bg-gray-100 dark:bg-white/[0.05]" />
                    </td>
                  ))}
                </tr>
              ))
            ) : visibleRecords.length ? (
              visibleRecords.map((record) => (
                <tr className="hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={record.key}>
                  <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">
                    <span className="block font-medium text-gray-700 dark:text-gray-300">{formatDateTime(record.date)}</span>
                    {record.end_date ? <span className="mt-1 block text-xs text-gray-400 dark:text-gray-500">End: {formatDateTime(record.end_date)}</span> : null}
                  </td>
                  <td className="px-5 py-4">
                    <span className="block text-theme-sm font-semibold text-gray-900 dark:text-white">{record.asset_label}</span>
                    <span className="mt-1 block max-w-[280px] truncate text-xs text-gray-500 dark:text-gray-400">{record.asset_area || "-"}</span>
                  </td>
                  <td className="px-5 py-4"><Badge value={record.source} /></td>
                  <td className="px-5 py-4">
                    <span className="block text-theme-sm font-semibold text-gray-900 dark:text-white">{record.reference}</span>
                    <span className="mt-1 block max-w-[320px] truncate text-theme-sm text-gray-500 dark:text-gray-400">{record.title}</span>
                  </td>
                  <td className="px-5 py-4">
                    <div className="flex flex-wrap gap-2">
                      {record.maintenance_type ? <Badge value={record.maintenance_type} /> : null}
                      {record.category ? <Badge value={record.category} /> : null}
                      {!record.maintenance_type && !record.category ? <span className="text-theme-sm text-gray-500 dark:text-gray-400">-</span> : null}
                    </div>
                  </td>
                  <td className="px-5 py-4">
                    <div className="flex flex-wrap gap-2">
                      {record.status ? <Badge value={record.status} /> : null}
                      {record.priority ? <Badge value={record.priority} /> : null}
                      {!record.status && !record.priority ? <span className="text-theme-sm text-gray-500 dark:text-gray-400">-</span> : null}
                    </div>
                  </td>
                  <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{minutesLabel(record.downtime_minutes)}</td>
                  <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{minutesLabel(record.repair_minutes)}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">
                    <span className="block max-w-[360px] truncate">{compactText([record.failure_code ? `Failure: ${statusText(record.failure_code)}` : null, record.root_cause ? `Root: ${statusText(record.root_cause)}` : null])}</span>
                    <span className="mt-1 block max-w-[360px] truncate text-gray-500 dark:text-gray-400">{compactText([record.action_taken, record.result, record.description])}</span>
                  </td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{record.technician_label || "-"}</td>
                </tr>
              ))
            ) : (
              <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={10}>No maintenance history found.</td></tr>
            )}
          </tbody>
        </table>
      </TablePanel>
    </div>
  );
}
