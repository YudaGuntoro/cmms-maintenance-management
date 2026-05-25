"use client";

import type { ApexOptions } from "apexcharts";
import dynamic from "next/dynamic";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Modal } from "@/components/ui/modal";
import { useTheme } from "@/context/ThemeContext";
import { apiGet } from "./api";
import { formatDateTime, formatNumber, statusText } from "./format";
import { Icon } from "./icons";
import { Badge, DataTableFooter, EntriesSelect, Feedback, InlineSearch, TablePanel } from "./ui";
import type { Asset, Technician, WorkOrder } from "./types";

const Chart = dynamic(() => import("react-apexcharts"), { ssr: false });

type PerformanceRow = {
  technician: Technician;
  assignedThisMonth: WorkOrder[];
  completedThisMonth: WorkOrder[];
  activeCases: WorkOrder[];
  urgentHandled: number;
  totalRepairMinutes: number;
  averageRepairMinutes: number;
  completionRate: number;
  lastHandledAt?: string | null;
};

const activeStatuses = ["OPEN", "ASSIGNED", "IN_PROGRESS", "PENDING"];
const doneStatuses = ["COMPLETED", "CLOSED"];
const priorityOrder = ["URGENT", "HIGH", "MEDIUM", "LOW"] as const;
const priorityColors: Record<string, string> = {
  URGENT: "#0EA5E9",
  HIGH: "#EF4444",
  MEDIUM: "#F59E0B",
  LOW: "#22C55E",
};

function chartTheme(isDark: boolean) {
  return {
    grid: isDark ? "rgba(255,255,255,0.08)" : "rgba(16,24,40,0.08)",
    muted: isDark ? "#98A2B3" : "#667085",
    panel: isDark ? "#111827" : "#FFFFFF",
    text: isDark ? "#F9FAFB" : "#101828",
    tooltip: (isDark ? "dark" : "light") as "dark" | "light",
  };
}

function monthKey(date = new Date()) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}`;
}

function parseMonth(value: string) {
  const [year, month] = value.split("-").map(Number);
  const start = new Date(year, (month || 1) - 1, 1);
  const end = new Date(year, month || 1, 1);
  return { end, start };
}

function isInRange(value: string | null | undefined, start: Date, end: Date) {
  if (!value) {
    return false;
  }

  const date = new Date(value);
  return !Number.isNaN(date.getTime()) && date >= start && date < end;
}

function minutesBetween(start?: string | null, end?: string | null) {
  if (!start || !end) {
    return 0;
  }

  const startTime = new Date(start).getTime();
  const endTime = new Date(end).getTime();
  if (!Number.isFinite(startTime) || !Number.isFinite(endTime) || endTime < startTime) {
    return 0;
  }

  return Math.round((endTime - startTime) / 60000);
}

function repairMinutes(workOrder: WorkOrder) {
  if (workOrder.repair_minutes && workOrder.repair_minutes > 0) {
    return workOrder.repair_minutes;
  }

  return minutesBetween(workOrder.repair_start || workOrder.started_at, workOrder.repair_end || workOrder.completed_at || workOrder.closed_at);
}

function formatDuration(minutes: number) {
  if (!minutes) {
    return "-";
  }

  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  if (!hours) {
    return `${rest} min`;
  }

  return `${hours}h ${rest}m`;
}

function formatDurationValue(minutes: number) {
  return minutes ? formatDuration(minutes) : "0 min";
}

function comparisonText(current: number, average: number, lowerIsBetter = false) {
  if (!current && !average) {
    return "No team average yet";
  }

  const diff = current - average;
  if (!diff) {
    return "Same as team average";
  }

  const amount = formatDurationValue(Math.abs(diff));
  if (lowerIsBetter) {
    return diff < 0 ? `${amount} faster than team avg` : `${amount} slower than team avg`;
  }

  return diff > 0 ? `${amount} above team avg` : `${amount} below team avg`;
}

function handledDate(workOrder: WorkOrder) {
  return workOrder.completed_at || workOrder.closed_at || workOrder.repair_end || workOrder.updated_at;
}

export default function TechnicianPerformancePage() {
  const [technicians, setTechnicians] = useState<Technician[]>([]);
  const [workOrders, setWorkOrders] = useState<WorkOrder[]>([]);
  const [assets, setAssets] = useState<Asset[]>([]);
  const [selectedMonth, setSelectedMonth] = useState(monthKey());
  const [monthDraft, setMonthDraft] = useState(monthKey());
  const [search, setSearch] = useState("");
  const [selectedRow, setSelectedRow] = useState<PerformanceRow | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [technicianData, workOrderData, assetData] = await Promise.all([
        apiGet<Technician[]>("/api/technicians"),
        apiGet<WorkOrder[]>("/api/work-orders"),
        apiGet<Asset[]>("/api/assets"),
      ]);
      setTechnicians(technicianData || []);
      setWorkOrders(workOrderData || []);
      setAssets(assetData || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load technician performance.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  useEffect(() => {
    setMonthDraft(selectedMonth);
  }, [selectedMonth]);

  useEffect(() => {
    if (monthDraft === selectedMonth) {
      return;
    }

    const timeoutId = window.setTimeout(() => {
      setSelectedMonth(monthDraft || monthKey());
    }, 300);

    return () => window.clearTimeout(timeoutId);
  }, [monthDraft, selectedMonth]);

  const assetName = useCallback((id: number) => {
    const asset = assets.find((item) => item.id === id);
    return asset ? `${asset.asset_code} - ${asset.asset_name}` : `Asset #${id}`;
  }, [assets]);

  const periodLabel = useMemo(() => {
    const { start } = parseMonth(selectedMonth);
    return new Intl.DateTimeFormat("id-ID", { month: "long", year: "numeric" }).format(start);
  }, [selectedMonth]);

  const performanceRows = useMemo<PerformanceRow[]>(() => {
    const { end, start } = parseMonth(selectedMonth);

    return technicians
      .map((technician) => {
        const assigned = workOrders.filter((workOrder) => workOrder.assigned_to === technician.id);
        const completedThisMonth = assigned.filter((workOrder) => doneStatuses.includes(workOrder.status) && isInRange(handledDate(workOrder), start, end));
        const assignedThisMonth = assigned.filter((workOrder) =>
          isInRange(workOrder.reported_at || workOrder.created_at, start, end) ||
          isInRange(workOrder.scheduled_at, start, end) ||
          isInRange(handledDate(workOrder), start, end));
        const activeCases = assigned.filter((workOrder) => activeStatuses.includes(workOrder.status));
        const totalRepairMinutes = completedThisMonth.reduce((total, workOrder) => total + repairMinutes(workOrder), 0);
        const averageRepairMinutes = completedThisMonth.length ? Math.round(totalRepairMinutes / completedThisMonth.length) : 0;
        const completionRate = assignedThisMonth.length ? Math.round((completedThisMonth.length / assignedThisMonth.length) * 100) : 0;
        const lastHandledAt = completedThisMonth
          .map(handledDate)
          .filter(Boolean)
          .sort((left, right) => new Date(String(right)).getTime() - new Date(String(left)).getTime())[0];

        return {
          technician,
          assignedThisMonth,
          completedThisMonth,
          activeCases,
          urgentHandled: completedThisMonth.filter((workOrder) => workOrder.priority === "URGENT").length,
          totalRepairMinutes,
          averageRepairMinutes,
          completionRate,
          lastHandledAt,
        };
      })
      .sort((left, right) => right.completedThisMonth.length - left.completedThisMonth.length || right.totalRepairMinutes - left.totalRepairMinutes);
  }, [selectedMonth, technicians, workOrders]);

  const summary = useMemo(() => {
    const totalHandled = performanceRows.reduce((total, row) => total + row.completedThisMonth.length, 0);
    const totalMinutes = performanceRows.reduce((total, row) => total + row.totalRepairMinutes, 0);
    const activeCases = performanceRows.reduce((total, row) => total + row.activeCases.length, 0);
    const techniciansWithHandledCases = performanceRows.filter((row) => row.completedThisMonth.length > 0).length;
    const avgMinutes = totalHandled ? Math.round(totalMinutes / totalHandled) : 0;
    const avgWorkMinutes = techniciansWithHandledCases ? Math.round(totalMinutes / techniciansWithHandledCases) : 0;

    return {
      activeCases,
      avgMinutes,
      avgWorkMinutes,
      techniciansWithHandledCases,
      totalHandled,
      totalMinutes,
    };
  }, [performanceRows]);

  const filteredRows = useMemo(() => {
    const normalized = search.trim().toLowerCase();
    if (!normalized) {
      return performanceRows;
    }

    return performanceRows.filter((row) => [
      row.technician.employee_no,
      row.technician.name,
      row.technician.skill_type,
      row.technician.shift,
      row.technician.email,
    ].some((value) => String(value || "").toLowerCase().includes(normalized)));
  }, [performanceRows, search]);

  useEffect(() => {
    setPage(1);
  }, [filteredRows.length, pageSize, search, selectedMonth]);

  const visibleRows = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredRows.slice(start, start + pageSize);
  }, [filteredRows, page, pageSize]);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Performance</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">Technician Performance</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Monthly maintenance productivity by technician for {periodLabel}.</p>
        </div>
        <button className="secondary-button" disabled={loading} onClick={() => void loadData()} type="button">
          <Icon name="refresh" />
          Refresh
        </button>
      </div>

      <Feedback error={error} />

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-5">
        <SummaryCard label="Cases Handled" value={formatNumber(summary.totalHandled)} />
        <SummaryCard label="Total Work Time" value={formatDuration(summary.totalMinutes)} />
        <SummaryCard label="Avg Work Time" value={formatDuration(summary.avgWorkMinutes)} />
        <SummaryCard label="Avg Repair Speed" value={formatDuration(summary.avgMinutes)} />
        <SummaryCard label="Active Load" value={formatNumber(summary.activeCases)} />
      </div>

      <TablePanel
        footer={<DataTableFooter onPageChange={setPage} page={page} pageSize={pageSize} total={filteredRows.length} />}
        toolbarLeft={(
          <>
            <EntriesSelect onChange={(value) => { setPageSize(value); setPage(1); }} value={pageSize} />
            <label className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
              Month
              <input
                className="h-10 rounded-lg border border-gray-300 bg-transparent px-3 py-2 text-sm font-medium text-gray-800 outline-none focus:border-brand-300 focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90"
                onChange={(event) => setMonthDraft(event.target.value)}
                type="month"
                value={monthDraft}
              />
            </label>
          </>
        )}
        toolbarRight={<InlineSearch onChange={setSearch} placeholder="Search technician" value={search} />}
      >
        <table className="min-w-full">
          <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
            <tr>
              <th className="min-w-52 px-5 py-3 text-left text-theme-xs font-semibold">Name</th>
              <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Position</th>
              <th className="min-w-28 px-5 py-3 text-left text-theme-xs font-semibold">Shift</th>
              <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">Work Time</th>
              <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">Avg Speed</th>
              <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Handled</th>
              <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Active Load</th>
              <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Urgent Done</th>
              <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">Completion</th>
              <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Last Handled</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
            {loading ? (
              Array.from({ length: 5 }).map((_, rowIndex) => (
                <tr key={rowIndex}>
                  {Array.from({ length: 10 }).map((__, cellIndex) => <td className="px-5 py-4" key={cellIndex}><div className="h-4 w-full animate-pulse rounded-md bg-gray-100 dark:bg-white/[0.05]" /></td>)}
                </tr>
              ))
            ) : visibleRows.length ? (
              visibleRows.map((row) => (
                <tr className="cursor-pointer hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={row.technician.id} onClick={() => setSelectedRow(row)}>
                  <td className="px-5 py-4">
                    <p className="text-theme-sm font-semibold text-gray-900 dark:text-white">{row.technician.name}</p>
                    <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">{row.technician.employee_no}</p>
                  </td>
                  <td className="px-5 py-4"><Badge value={row.technician.skill_type} /></td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{row.technician.shift}</td>
                  <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{formatDuration(row.totalRepairMinutes)}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{formatDuration(row.averageRepairMinutes)}</td>
                  <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{row.completedThisMonth.length}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{row.activeCases.length}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{row.urgentHandled}</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{row.completionRate}%</td>
                  <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">{formatDateTime(row.lastHandledAt)}</td>
                </tr>
              ))
            ) : (
              <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={10}>No technician performance found.</td></tr>
            )}
          </tbody>
        </table>
      </TablePanel>

      <Modal isOpen={Boolean(selectedRow)} onClose={() => setSelectedRow(null)} className="mx-4 max-w-[920px] overflow-hidden p-0" showCloseButton={false}>
        {selectedRow ? (
          <>
            <div className="border-b border-gray-100 px-6 py-5 dark:border-white/[0.05]">
              <div className="flex items-start gap-4">
                <div className="flex size-12 shrink-0 items-center justify-center rounded-lg bg-brand-500/10 text-brand-500 ring-1 ring-brand-500/20">
                  <Icon className="size-6" name="wrench" />
                </div>
                <div className="min-w-0 flex-1">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white/90">{selectedRow.technician.name}</h3>
                  <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{statusText(selectedRow.technician.skill_type)} technician, shift {selectedRow.technician.shift}</p>
                </div>
                <button className="icon-button" onClick={() => setSelectedRow(null)} type="button"><Icon name="x" /></button>
              </div>
            </div>
            <div className="max-h-[70vh] overflow-y-auto px-6 py-5">
              <div className="grid grid-cols-1 gap-3 md:grid-cols-4">
                <DetailMetric label="Handled" value={String(selectedRow.completedThisMonth.length)} />
                <DetailMetric label="Work Time" value={formatDuration(selectedRow.totalRepairMinutes)} />
                <DetailMetric label="Avg Speed" value={formatDuration(selectedRow.averageRepairMinutes)} />
                <DetailMetric label="Active Load" value={String(selectedRow.activeCases.length)} />
              </div>

              <div className="mt-5 grid grid-cols-1 gap-4 lg:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)]">
                <TechnicianPriorityPieChart workOrders={selectedRow.completedThisMonth} />
                <TechnicianComparisonChart
                  row={selectedRow}
                  teamAvgSpeedMinutes={summary.avgMinutes}
                  teamAvgWorkMinutes={summary.avgWorkMinutes}
                  technicianCount={summary.techniciansWithHandledCases}
                />
              </div>

              <div className="mt-6 overflow-hidden rounded-lg border border-gray-100 dark:border-white/[0.05]">
                <table className="min-w-full">
                  <thead className="bg-gray-50 dark:bg-white/[0.03]">
                    <tr>
                      <th className="px-4 py-3 text-left text-theme-xs font-semibold text-gray-500 dark:text-gray-400">WO Number</th>
                      <th className="px-4 py-3 text-left text-theme-xs font-semibold text-gray-500 dark:text-gray-400">Title</th>
                      <th className="px-4 py-3 text-left text-theme-xs font-semibold text-gray-500 dark:text-gray-400">Asset</th>
                      <th className="px-4 py-3 text-left text-theme-xs font-semibold text-gray-500 dark:text-gray-400">Priority</th>
                      <th className="px-4 py-3 text-left text-theme-xs font-semibold text-gray-500 dark:text-gray-400">Repair</th>
                      <th className="px-4 py-3 text-left text-theme-xs font-semibold text-gray-500 dark:text-gray-400">Completed</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                    {selectedRow.completedThisMonth.length ? selectedRow.completedThisMonth.map((workOrder) => (
                      <tr key={workOrder.id}>
                        <td className="px-4 py-3 text-sm font-semibold text-gray-900 dark:text-white">{workOrder.wo_number}</td>
                        <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{workOrder.title}</td>
                        <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{assetName(workOrder.asset_id)}</td>
                        <td className="px-4 py-3"><Badge value={workOrder.priority} /></td>
                        <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{formatDuration(repairMinutes(workOrder))}</td>
                        <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{formatDateTime(handledDate(workOrder))}</td>
                      </tr>
                    )) : (
                      <tr><td className="px-4 py-6 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={6}>No handled cases in this month.</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </>
        ) : null}
      </Modal>
    </div>
  );
}

function SummaryCard({ label, value }: { label: string; value: string }) {
  return (
    <section className="rounded-lg border border-gray-200 bg-white p-5 shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
      <p className="text-sm font-medium text-gray-500 dark:text-gray-400">{label}</p>
      <h2 className="mt-3 text-3xl font-semibold text-gray-900 dark:text-white">{value}</h2>
    </section>
  );
}

function DetailMetric({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-gray-100 bg-gray-50 px-4 py-3 dark:border-white/[0.05] dark:bg-white/[0.03]">
      <p className="text-xs font-semibold uppercase tracking-[0.14em] text-gray-500 dark:text-gray-400">{label}</p>
      <p className="mt-2 text-lg font-semibold text-gray-900 dark:text-white">{value}</p>
    </div>
  );
}

function TechnicianPriorityPieChart({ workOrders }: { workOrders: WorkOrder[] }) {
  const { theme } = useTheme();
  const isDark = theme === "dark";
  const colors = chartTheme(isDark);
  const slices = useMemo(() => {
    const orderedSlices = priorityOrder
      .map((priority) => ({
        color: priorityColors[priority],
        label: statusText(priority),
        value: workOrders.filter((workOrder) => workOrder.priority === priority).length,
      }))
      .filter((item) => item.value > 0);

    const otherCount = workOrders.filter((workOrder) => !priorityOrder.includes(workOrder.priority)).length;
    return otherCount
      ? [...orderedSlices, { color: "#64748B", label: "Other", value: otherCount }]
      : orderedSlices;
  }, [workOrders]);

  const total = slices.reduce((sum, item) => sum + item.value, 0);
  const options = useMemo<ApexOptions>(() => ({
    chart: {
      foreColor: colors.text,
      fontFamily: "Outfit, sans-serif",
      toolbar: { show: false },
      type: "donut",
    },
    colors: slices.map((item) => item.color),
    dataLabels: {
      enabled: false,
    },
    labels: slices.map((item) => item.label),
    legend: { show: false },
    plotOptions: {
      pie: {
        donut: {
          labels: {
            show: true,
            name: {
              color: colors.muted,
              fontSize: "12px",
              fontWeight: 700,
              offsetY: -4,
              show: true,
            },
            total: {
              color: colors.text,
              fontSize: "15px",
              fontWeight: 800,
              formatter: () => `${formatNumber(total)} WO`,
              label: "Completed",
              show: true,
              showAlways: true,
            },
            value: {
              color: colors.text,
              fontSize: "22px",
              fontWeight: 800,
              offsetY: 4,
              show: true,
              formatter: (value) => formatNumber(Number(value)),
            },
          },
          size: "64%",
        },
        expandOnClick: false,
      },
    },
    stroke: {
      colors: [colors.panel],
      width: 4,
    },
    tooltip: {
      fillSeriesColor: false,
      theme: colors.tooltip,
      y: {
        formatter: (value: number) => `${formatNumber(value)} WO`,
      },
    },
  }), [colors.muted, colors.panel, colors.text, colors.tooltip, slices, total]);

  return (
    <section className="rounded-lg border border-gray-100 bg-gray-50 p-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.14em] text-gray-500 dark:text-gray-400">Priority Donut Chart</p>
          <h4 className="mt-2 text-sm font-semibold text-gray-900 dark:text-white">Completed WO Mix</h4>
        </div>
        <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-gray-700 ring-1 ring-gray-200 dark:bg-white/[0.04] dark:text-gray-300 dark:ring-white/[0.08]">
          {formatNumber(total)} WO
        </span>
      </div>

      {total ? (
        <>
          <div className="mt-2 h-[250px]">
            <Chart key={`${theme}-${slices.map((item) => `${item.label}:${item.value}`).join("|")}`} height={250} options={options} series={slices.map((item) => item.value)} type="donut" />
          </div>
          <div className="grid gap-2 sm:grid-cols-2">
            {slices.map((item) => (
              <div className="flex items-center gap-2 text-xs text-gray-600 dark:text-gray-300" key={item.label}>
                <span className="size-2.5 rounded-full" style={{ backgroundColor: item.color }} />
                <span className="truncate">{item.label}</span>
                <span className="ml-auto font-semibold text-gray-900 dark:text-white">{item.value}</span>
              </div>
            ))}
          </div>
        </>
      ) : (
        <div className="flex h-[260px] items-center justify-center rounded-lg border border-dashed border-gray-200 text-sm text-gray-500 dark:border-white/[0.08] dark:text-gray-400">
          No completed WO data.
        </div>
      )}
    </section>
  );
}

function TechnicianComparisonChart({
  row,
  teamAvgSpeedMinutes,
  teamAvgWorkMinutes,
  technicianCount,
}: {
  row: PerformanceRow;
  teamAvgSpeedMinutes: number;
  teamAvgWorkMinutes: number;
  technicianCount: number;
}) {
  const { theme } = useTheme();
  const isDark = theme === "dark";
  const colors = chartTheme(isDark);
  const series = useMemo(() => ([
    {
      data: [row.averageRepairMinutes, row.totalRepairMinutes],
      name: row.technician.name,
    },
    {
      data: [teamAvgSpeedMinutes, teamAvgWorkMinutes],
      name: "Team Avg",
    },
  ]), [row.averageRepairMinutes, row.technician.name, row.totalRepairMinutes, teamAvgSpeedMinutes, teamAvgWorkMinutes]);

  const options = useMemo<ApexOptions>(() => ({
    chart: {
      foreColor: colors.text,
      fontFamily: "Outfit, sans-serif",
      toolbar: { show: false },
      type: "bar",
    },
    colors: ["#465FFF", "#14B8A6"],
    dataLabels: { enabled: false },
    grid: {
      borderColor: colors.grid,
      strokeDashArray: 4,
    },
    legend: {
      fontFamily: "Outfit, sans-serif",
      fontSize: "12px",
      labels: { colors: colors.muted },
      markers: { size: 8 },
      position: "top",
    },
    plotOptions: {
      bar: {
        borderRadius: 5,
        borderRadiusApplication: "end",
        columnWidth: "46%",
      },
    },
    stroke: {
      colors: ["transparent"],
      show: true,
      width: 3,
    },
    tooltip: {
      theme: colors.tooltip,
      y: {
        formatter: (value: number) => formatDurationValue(value),
      },
    },
    xaxis: {
      axisBorder: { show: false },
      axisTicks: { show: false },
      categories: ["Avg Speed", "Work Time"],
      labels: {
        style: {
          colors: [colors.muted, colors.muted],
          fontSize: "12px",
        },
      },
    },
    yaxis: {
      labels: {
        formatter: (value: number) => formatDurationValue(Math.round(value)),
        style: {
          colors: [colors.muted],
          fontSize: "11px",
        },
      },
    },
  }), [colors.grid, colors.muted, colors.text, colors.tooltip]);

  return (
    <section className="rounded-lg border border-gray-100 bg-gray-50 p-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.14em] text-gray-500 dark:text-gray-400">Technician Performance</p>
          <h4 className="mt-2 text-sm font-semibold text-gray-900 dark:text-white">Selected vs Team Average</h4>
        </div>
        <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-gray-700 ring-1 ring-gray-200 dark:bg-white/[0.04] dark:text-gray-300 dark:ring-white/[0.08]">
          {formatNumber(technicianCount)} tech avg
        </span>
      </div>

      <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
        <ComparisonMetric
          label="Avg Speed"
          note={comparisonText(row.averageRepairMinutes, teamAvgSpeedMinutes, true)}
          value={formatDuration(row.averageRepairMinutes)}
        />
        <ComparisonMetric
          label="Work Time"
          note={comparisonText(row.totalRepairMinutes, teamAvgWorkMinutes)}
          value={formatDuration(row.totalRepairMinutes)}
        />
      </div>

      <div className="mt-2 h-[210px]">
        <Chart key={theme} height={210} options={options} series={series} type="bar" />
      </div>
    </section>
  );
}

function ComparisonMetric({ label, note, value }: { label: string; note: string; value: string }) {
  return (
    <div className="rounded-lg bg-white px-3 py-2 ring-1 ring-gray-200 dark:bg-white/[0.04] dark:ring-white/[0.08]">
      <p className="text-xs font-semibold text-gray-500 dark:text-gray-400">{label}</p>
      <p className="mt-1 text-sm font-semibold text-gray-900 dark:text-white">{value}</p>
      <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">{note}</p>
    </div>
  );
}
