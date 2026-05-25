"use client";

import type { ApexOptions } from "apexcharts";
import dynamic from "next/dynamic";
import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Icon } from "./icons";
import { apiGet, buildQuery } from "./api";
import { formatDateTime, formatNumber } from "./format";
import { Badge, Card, CardHeader, Feedback } from "./ui";
import type { DashboardSummaryResponse, NameValueSummary, PreventiveSchedule, ReliabilityKpiResponse, Sparepart, WorkOrder } from "./types";

const Chart = dynamic(() => import("react-apexcharts"), { ssr: false });

const entityColorMap: Record<string, string> = {
  ASSIGNED: "#38BDF8",
  CANCELLED: "#FB7185",
  CLOSED: "#94A3B8",
  COMPLETED: "#34D399",
  DRAFT: "#CBD5E1",
  ELECTRICAL: "#22D3EE",
  HIGH: "#FB7185",
  IN_PROGRESS: "#818CF8",
  LOW: "#34D399",
  MATERIAL: "#F472B6",
  MECHANICAL: "#A78BFA",
  MEDIUM: "#FBBF24",
  OPEN: "#60A5FA",
  OPERATIONAL: "#2DD4BF",
  OTHER: "#94A3B8",
  PENDING: "#FBBF24",
  PLANNED_STOP: "#C084FC",
  URGENT: "#06B6D4",
  UTILITY: "#FB923C",
};

const fallbackEntityColors = ["#60A5FA", "#2DD4BF", "#FBBF24", "#FB7185", "#A78BFA", "#22D3EE", "#34D399", "#94A3B8"];

function getEntityColor(name: string, index = 0) {
  return entityColorMap[name.toUpperCase()] || fallbackEntityColors[index % fallbackEntityColors.length];
}

function StatCard({ href, label, value, icon }: { href: string; label: string; value: number | string; icon: "activity" | "alert" | "box" | "calendar" | "wrench" }) {
  return (
    <Link
      className="group block overflow-hidden rounded-lg border border-gray-200 bg-white p-5 shadow-theme-sm transition duration-200 hover:-translate-y-0.5 hover:border-brand-300 hover:shadow-theme-md focus:outline-none focus:ring-3 focus:ring-brand-500/10 dark:border-gray-800 dark:bg-gray-900 dark:hover:border-brand-500/40"
      href={href}
    >
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-sm font-medium text-gray-500 transition-colors group-hover:text-brand-600 dark:text-gray-400 dark:group-hover:text-brand-400">{label}</p>
          <h2 className="mt-3 text-3xl font-semibold text-gray-900 dark:text-white">{typeof value === "number" ? formatNumber(value) : value}</h2>
        </div>
        <div className="flex h-11 w-11 items-center justify-center rounded-lg bg-brand-500/10 text-brand-600 ring-1 ring-brand-500/20 transition-colors group-hover:bg-brand-500 group-hover:text-white dark:text-brand-400 dark:group-hover:text-white">
          <Icon name={icon} />
        </div>
      </div>
      <span className="mt-4 inline-flex text-xs font-semibold text-brand-600 opacity-0 transition-opacity group-hover:opacity-100 group-focus:opacity-100 dark:text-brand-400">Lihat detail</span>
    </Link>
  );
}

function formatDateParam(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function getCurrentMonthPeriod() {
  const now = new Date();
  const startDate = new Date(now.getFullYear(), now.getMonth(), 1);
  const endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);
  const endExclusive = new Date(now.getFullYear(), now.getMonth() + 1, 1);

  return {
    endDate,
    endExclusive,
    endParam: formatDateParam(endDate),
    label: new Intl.DateTimeFormat("id-ID", { month: "long", year: "numeric" }).format(startDate),
    startDate,
    startParam: formatDateParam(startDate),
  };
}

function isInRange(value: string | null | undefined, start: Date, end: Date) {
  if (!value) {
    return false;
  }

  const date = new Date(value);
  return !Number.isNaN(date.getTime()) && date >= start && date < end;
}

function workOrderPeriodDate(workOrder: WorkOrder) {
  return workOrder.closed_at || workOrder.completed_at || workOrder.scheduled_at || workOrder.reported_at || workOrder.updated_at || workOrder.created_at;
}

function touchesPeriod(workOrder: WorkOrder, start: Date, end: Date) {
  return [
    workOrder.created_at,
    workOrder.reported_at,
    workOrder.scheduled_at,
    workOrder.completed_at,
    workOrder.closed_at,
    workOrder.updated_at,
  ].some((value) => isInRange(value, start, end));
}

function SummaryBars({ items, emptyLabel }: { items: NameValueSummary[]; emptyLabel: string }) {
  const max = Math.max(...items.map((item) => item.value), 1);
  if (!items.length) {
    return <p className="px-5 py-6 text-sm text-gray-500 dark:text-gray-400">{emptyLabel}</p>;
  }

  return (
    <div className="space-y-4 p-5">
      {items.map((item, index) => {
        const color = getEntityColor(item.name, index);

        return (
        <div key={item.name}>
          <div className="mb-2 flex items-center justify-between gap-3">
            <span className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
              <span className="size-2.5 rounded-full" style={{ backgroundColor: color }} />
              {item.name.replaceAll("_", " ")}
            </span>
            <span className="text-sm font-semibold text-gray-900 dark:text-white">{formatNumber(item.value)}</span>
          </div>
          <div className="h-2 overflow-hidden rounded-full bg-gray-100 dark:bg-gray-800">
            <div className="h-full rounded-full" style={{ backgroundColor: color, width: `${Math.max(6, (item.value / max) * 100)}%` }} />
          </div>
        </div>
        );
      })}
    </div>
  );
}

function DowntimeColumnChart({ emptyLabel, items }: { emptyLabel: string; items: NameValueSummary[] }) {
  const chartItems = useMemo(() => items.filter((item) => item.value > 0), [items]);
  const options = useMemo<ApexOptions>(() => ({
    chart: {
      fontFamily: "Outfit, sans-serif",
      toolbar: { show: false },
      type: "bar",
    },
    colors: chartItems.map((item, index) => getEntityColor(item.name, index)),
    dataLabels: {
      enabled: true,
      formatter: (value: number) => formatNumber(value),
      offsetY: -18,
      style: {
        colors: ["#98A2B3"],
        fontSize: "13px",
        fontWeight: 800,
      },
    },
    grid: {
      borderColor: "rgba(148, 163, 184, 0.18)",
      strokeDashArray: 4,
    },
    plotOptions: {
      bar: {
        borderRadius: 5,
        borderRadiusApplication: "end",
        columnWidth: "46%",
        distributed: true,
        dataLabels: {
          position: "top",
        },
      },
    },
    stroke: {
      colors: ["transparent"],
      show: true,
      width: 3,
    },
    tooltip: {
      y: {
        formatter: (value: number) => `${formatNumber(value)} min`,
      },
    },
    xaxis: {
      axisBorder: { show: false },
      axisTicks: { show: false },
      categories: chartItems.map((item) => item.name.replaceAll("_", " ")),
      labels: {
        rotate: -20,
        rotateAlways: false,
        style: {
          colors: "#98A2B3",
          fontSize: "12px",
          fontWeight: 700,
        },
        trim: true,
      },
    },
    yaxis: {
      labels: {
        formatter: (value: number) => formatNumber(Math.round(value)),
        style: {
          colors: ["#98A2B3"],
          fontSize: "12px",
        },
      },
    },
  }), [chartItems]);

  if (!chartItems.length) {
    return <p className="px-5 py-6 text-sm text-gray-500 dark:text-gray-400">{emptyLabel}</p>;
  }

  return (
    <div className="p-5">
      <Chart height={310} options={options} series={[{ data: chartItems.map((item) => item.value), name: "Downtime" }]} type="bar" />
    </div>
  );
}

export default function DashboardPage() {
  const [summary, setSummary] = useState<DashboardSummaryResponse | null>(null);
  const [workOrders, setWorkOrders] = useState<WorkOrder[]>([]);
  const [lowStock, setLowStock] = useState<Sparepart[]>([]);
  const [schedules, setSchedules] = useState<PreventiveSchedule[]>([]);
  const [kpi, setKpi] = useState<ReliabilityKpiResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const dashboardPeriod = useMemo(() => getCurrentMonthPeriod(), []);

  const loadDashboard = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const kpiQuery = buildQuery({
        end_date: dashboardPeriod.endParam,
        start_date: dashboardPeriod.startParam,
      });
      const [summaryData, workOrderData, sparepartData, scheduleData, kpiData] = await Promise.all([
        apiGet<DashboardSummaryResponse>("/api/dashboard/summary"),
        apiGet<WorkOrder[]>("/api/work-orders"),
        apiGet<Sparepart[]>("/api/spareparts?low_stock_only=true"),
        apiGet<PreventiveSchedule[]>("/api/preventive-schedules?is_active=true"),
        apiGet<ReliabilityKpiResponse>(`/api/kpi/reliability${kpiQuery}`),
      ]);
      setSummary(summaryData);
      setWorkOrders(workOrderData || []);
      setLowStock(sparepartData || []);
      setSchedules(scheduleData || []);
      setKpi(kpiData);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load dashboard.");
    } finally {
      setLoading(false);
    }
  }, [dashboardPeriod.endParam, dashboardPeriod.startParam]);

  useEffect(() => {
    void loadDashboard();
  }, [loadDashboard]);

  const recentWorkOrders = useMemo(() => workOrders
    .filter((workOrder) => touchesPeriod(workOrder, dashboardPeriod.startDate, dashboardPeriod.endExclusive))
    .sort((left, right) => new Date(workOrderPeriodDate(right) || "").getTime() - new Date(workOrderPeriodDate(left) || "").getTime())
    .slice(0, 8), [dashboardPeriod.endExclusive, dashboardPeriod.startDate, workOrders]);
  const dueSchedules = useMemo(() => schedules
    .filter((schedule) => isInRange(schedule.next_due_date, dashboardPeriod.startDate, dashboardPeriod.endExclusive))
    .sort((left, right) => new Date(left.next_due_date).getTime() - new Date(right.next_due_date).getTime())
    .slice(0, 5), [dashboardPeriod.endExclusive, dashboardPeriod.startDate, schedules]);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">MaintenanceApp</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">Overview Dashboard</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Monthly overview for {dashboardPeriod.label}</p>
        </div>
        <button className="secondary-button" disabled={loading} onClick={() => void loadDashboard()} type="button">
          <Icon name="refresh" />
          Refresh
        </button>
      </div>

      <Feedback error={error} />

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
        <StatCard href="/assets" icon="activity" label="Total Assets" value={summary?.total_assets ?? "-"} />
        <StatCard href="/work-orders?view=open" icon="wrench" label="Open Work Orders" value={summary?.open_work_orders ?? "-"} />
        <StatCard href="/preventive-schedules?view=overdue&is_active=true" icon="calendar" label="Overdue PM" value={summary?.overdue_preventive_maintenance ?? "-"} />
        <StatCard href="/spareparts?low_stock_only=true" icon="box" label="Low Stock Spareparts" value={summary?.low_stock_spareparts ?? "-"} />
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <Card className="xl:col-span-2">
          <CardHeader description="Current maintenance workflow" title="Work Order Status" />
          <SummaryBars emptyLabel="No work order status data." items={summary?.work_order_status_summary || []} />
        </Card>

        <Card>
          <CardHeader description="All assets, selected period" title="Reliability KPI" />
          <div className="space-y-4 p-5">
            <div>
              <p className="text-sm text-gray-500 dark:text-gray-400">Availability</p>
              <h3 className="mt-2 text-4xl font-semibold text-gray-900 dark:text-white">{kpi ? `${Number(kpi.availability_percent).toFixed(1)}%` : "-"}</h3>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="rounded-lg border border-gray-200 p-3 dark:border-gray-800">
                <p className="text-xs text-gray-500 dark:text-gray-400">MTTR</p>
                <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">{kpi ? formatNumber(kpi.mttr_minutes) : "-"}</p>
              </div>
              <div className="rounded-lg border border-gray-200 p-3 dark:border-gray-800">
                <p className="text-xs text-gray-500 dark:text-gray-400">MTBF</p>
                <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">{kpi ? formatNumber(kpi.mtbf_minutes) : "-"}</p>
              </div>
            </div>
            <div className="h-2 overflow-hidden rounded-full bg-gray-100 dark:bg-gray-800">
              <div className="h-full rounded-full bg-success-500" style={{ width: `${Math.min(100, Number(kpi?.availability_percent || 0))}%` }} />
            </div>
          </div>
        </Card>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
        <Card>
          <CardHeader description={`Minutes by category in ${dashboardPeriod.label}`} title="Downtime Category" />
          <DowntimeColumnChart emptyLabel="No downtime data." items={summary?.downtime_category_summary || []} />
        </Card>

        <Card>
          <CardHeader description="Highest downtime minutes" title="Top Assets by Downtime" />
          <div className="divide-y divide-gray-100 dark:divide-gray-800">
            {(summary?.top_assets_by_downtime || []).slice(0, 5).map((asset) => (
              <div className="flex items-center justify-between gap-4 px-5 py-4" key={asset.asset_id}>
                <div>
                  <p className="text-sm font-semibold text-gray-900 dark:text-white">{asset.asset_code}</p>
                  <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{asset.asset_name}</p>
                </div>
                <span className="text-sm font-semibold text-gray-900 dark:text-white">{formatNumber(asset.value)} min</span>
              </div>
            ))}
            {!summary?.top_assets_by_downtime?.length ? <p className="px-5 py-6 text-sm text-gray-500 dark:text-gray-400">No asset downtime data.</p> : null}
          </div>
        </Card>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <section className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-white/[0.05] dark:bg-white/[0.03] xl:col-span-2">
          <CardHeader description={`${recentWorkOrders.length} latest records`} title="Recent Work Orders" />
          <div className="mx-4 mb-4 mt-2 overflow-hidden rounded-lg border border-gray-100 dark:border-white/[0.05]">
            <div className="max-w-full overflow-x-auto">
              <table className="min-w-full">
                <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
                  <tr>
                    <th className="min-w-40 px-5 py-3 text-left text-theme-xs font-semibold">WO Number</th>
                    <th className="min-w-64 px-5 py-3 text-left text-theme-xs font-semibold">Title</th>
                    <th className="min-w-32 px-5 py-3 text-left text-theme-xs font-semibold">Priority</th>
                    <th className="min-w-36 px-5 py-3 text-left text-theme-xs font-semibold">Status</th>
                    <th className="min-w-44 px-5 py-3 text-left text-theme-xs font-semibold">Reported</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                  {recentWorkOrders.map((workOrder) => (
                    <tr className="hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={workOrder.id}>
                      <td className="px-5 py-4 text-theme-sm font-semibold text-gray-900 dark:text-white">{workOrder.wo_number}</td>
                      <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300">{workOrder.title}</td>
                      <td className="px-5 py-4"><Badge value={workOrder.priority} /></td>
                      <td className="px-5 py-4"><Badge value={workOrder.status} /></td>
                      <td className="px-5 py-4 text-theme-sm text-gray-500 dark:text-gray-400">{formatDateTime(workOrder.reported_at)}</td>
                    </tr>
                  ))}
                  {!recentWorkOrders.length ? <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={5}>No work orders found.</td></tr> : null}
                </tbody>
              </table>
            </div>
          </div>
        </section>

        <div className="space-y-6">
          <Card>
            <CardHeader description={`${lowStock.length} items`} title="Low Stock" />
            <div className="divide-y divide-gray-100 dark:divide-gray-800">
              {lowStock.slice(0, 5).map((part) => (
                <div className="flex items-center justify-between gap-4 px-5 py-4" key={part.id}>
                  <div>
                    <p className="text-sm font-semibold text-gray-900 dark:text-white">{part.part_code}</p>
                    <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{part.part_name}</p>
                  </div>
                  <span className="text-sm font-semibold text-error-700 dark:text-error-500">{formatNumber(part.stock_qty)} / {formatNumber(part.minimum_stock)}</span>
                </div>
              ))}
              {!lowStock.length ? <p className="px-5 py-6 text-sm text-gray-500 dark:text-gray-400">No low stock items.</p> : null}
            </div>
          </Card>

          <Card>
            <CardHeader description="Nearest due date" title="Active PM" />
            <div className="divide-y divide-gray-100 dark:divide-gray-800">
              {dueSchedules.map((schedule) => (
                <div className="px-5 py-4" key={schedule.id}>
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="text-sm font-semibold text-gray-900 dark:text-white">{schedule.schedule_name}</p>
                      <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Asset #{schedule.asset_id}</p>
                    </div>
                    <Badge value={schedule.frequency_type} />
                  </div>
                  <p className="mt-2 text-xs text-gray-500 dark:text-gray-400">{formatDateTime(schedule.next_due_date)}</p>
                </div>
              ))}
              {!dueSchedules.length ? <p className="px-5 py-6 text-sm text-gray-500 dark:text-gray-400">No active PM schedules.</p> : null}
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
