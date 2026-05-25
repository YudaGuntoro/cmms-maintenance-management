"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { getStoredUser } from "./auth";
import { apiGet, apiPost, getApiBaseUrl } from "./api";
import { formatDateTime, nowDateTimeLocal, statusText } from "./format";
import { Icon } from "./icons";
import {
  problemReportCategories,
  problemReportStatuses,
  workOrderPriorities,
  type Asset,
  type ProblemReport,
  type UserResponse,
} from "./types";
import { Badge, Feedback } from "./ui";

type ReportPayloadForm = {
  report_number: string;
  asset_id: string;
  title: string;
  description: string;
  category: string;
  priority: string;
  reported_by: string;
  reported_at: string;
  downtime_start: string;
};

type MasterLookup = {
  id: number;
  code: string;
  name: string;
  level?: number;
  sequence?: number;
  interval_days?: number;
};

type MasterReferences = {
  workOrderPriorities: MasterLookup[];
  workOrderStatuses: MasterLookup[];
  problemReportCategories: MasterLookup[];
};

const emptyMasters: MasterReferences = {
  workOrderPriorities: [],
  workOrderStatuses: [],
  problemReportCategories: [],
};

const endpointDocs = [
  {
    method: "POST",
    path: "/api/auth/login",
    title: "Get access token",
    note: "Use this first when testing from Postman or another app.",
  },
  {
    method: "GET",
    path: "/api/assets",
    title: "Read existing assets",
    note: "Use asset id from this endpoint as asset_id in report payload.",
  },
  {
    method: "GET",
    path: "/api/problem-reports/next-number",
    title: "Preview next report number",
    note: "Optional. If report_number is blank, backend generates it during create.",
  },
  {
    method: "POST",
    path: "/api/problem-reports",
    title: "Create problem report",
    note: "Status is forced to PENDING by backend on create.",
  },
];

const masterEndpointDocs = [
  {
    field: "category / category_id",
    path: "/api/problem-report-categories",
    usage: "Problem Report category. Pakai code untuk category atau id untuk category_id.",
    key: "problemReportCategories",
    fallback: problemReportCategories,
  },
  {
    field: "priority / priority_id",
    path: "/api/work-order-priorities",
    usage: "Priority untuk Problem Report. Pakai code untuk priority atau id untuk priority_id.",
    key: "workOrderPriorities",
    fallback: workOrderPriorities,
  },
  {
    field: "status / status_id",
    path: "/api/work-order-statuses",
    usage: "Master status. Untuk create Problem Report, backend tetap set PENDING.",
    key: "workOrderStatuses",
    fallback: problemReportStatuses,
  },
] satisfies Array<{
  field: string;
  path: string;
  usage: string;
  key: keyof MasterReferences;
  fallback: readonly string[];
}>;

function sampleForm(assetId = "", nextNumber = "", user?: UserResponse | null): ReportPayloadForm {
  return {
    report_number: nextNumber,
    asset_id: assetId,
    title: "Abnormal vibration on production machine",
    description: "Machine vibration is higher than normal and needs MTC follow-up.",
    category: "BREAKDOWN",
    priority: "MEDIUM",
    reported_by: user?.full_name || user?.username || "External Integration",
    reported_at: nowDateTimeLocal(),
    downtime_start: "",
  };
}

function buildProblemReportPayload(form: ReportPayloadForm) {
  return {
    report_number: form.report_number,
    asset_id: Number(form.asset_id),
    title: form.title,
    description: form.description || null,
    category: form.category,
    priority: form.priority,
    status: "PENDING",
    reported_by: form.reported_by || null,
    reported_at: form.reported_at || new Date().toISOString(),
    downtime_start: form.category === "DOWNTIME" ? form.downtime_start || form.reported_at : null,
    downtime_end: null,
  };
}

function stringify(value: unknown) {
  return JSON.stringify(value, null, 2);
}

function masterCodes(items: MasterLookup[], fallback: readonly string[]) {
  return items.length ? items.map((item) => item.code) : [...fallback];
}

function masterItems(items: MasterLookup[], fallback: readonly string[]) {
  return items.length ? items : fallback.map((code, index) => ({ id: index + 1, code, name: statusText(code) }));
}

function problemReportStatusMasters(items: MasterLookup[]) {
  return problemReportStatuses.map((code, index) =>
    items.find((item) => item.code === code) || { id: index + 1, code, name: statusText(code) }
  );
}

export default function IntegrationPage() {
  const [assets, setAssets] = useState<Asset[]>([]);
  const [masters, setMasters] = useState<MasterReferences>(emptyMasters);
  const [nextNumber, setNextNumber] = useState("");
  const [user, setUser] = useState<UserResponse | null>(null);
  const [form, setForm] = useState<ReportPayloadForm>(() => sampleForm());
  const [payloadText, setPayloadText] = useState(() => stringify(buildProblemReportPayload(sampleForm())));
  const [responseText, setResponseText] = useState("");
  const [loading, setLoading] = useState(true);
  const [testing, setTesting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const baseUrl = getApiBaseUrl();

  const refreshReferences = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const currentUser = getStoredUser();
      const [
        assetData,
        reportNumber,
        priorityData,
        statusData,
        problemReportCategoryData,
      ] = await Promise.all([
        apiGet<Asset[]>("/api/assets"),
        apiGet<string>("/api/problem-reports/next-number"),
        apiGet<MasterLookup[]>("/api/work-order-priorities"),
        apiGet<MasterLookup[]>("/api/work-order-statuses"),
        apiGet<MasterLookup[]>("/api/problem-report-categories"),
      ]);

      const firstAssetId = assetData?.[0]?.id ? String(assetData[0].id) : "";
      const nextForm = sampleForm(firstAssetId, reportNumber || "", currentUser);
      setAssets(assetData || []);
      setMasters({
        workOrderPriorities: priorityData || [],
        workOrderStatuses: statusData || [],
        problemReportCategories: problemReportCategoryData || [],
      });
      setNextNumber(reportNumber || "");
      setUser(currentUser);
      setForm(nextForm);
      setPayloadText(stringify(buildProblemReportPayload(nextForm)));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load integration references.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refreshReferences();
  }, [refreshReferences]);

  const curlCreateReport = useMemo(() => {
    return [
      `curl -X POST "${baseUrl}/api/problem-reports"`,
      '  -H "Authorization: Bearer <access_token>"',
      '  -H "Content-Type: application/json"',
      `  -d '${payloadText.replaceAll("'", "\\'")}'`,
    ].join(" \\\n");
  }, [baseUrl, payloadText]);

  const curlReadMasterData = useMemo(() => {
    return masterEndpointDocs.map((item) => [
      `# ${item.field}`,
      `curl -X GET "${baseUrl}${item.path}"`,
      '  -H "Authorization: Bearer <access_token>"',
    ].join(" \\\n")).join("\n\n");
  }, [baseUrl]);

  const curlReadAssets = useMemo(() => {
    return [
      `curl -X GET "${baseUrl}/api/assets"`,
      '  -H "Authorization: Bearer <access_token>"',
    ].join(" \\\n");
  }, [baseUrl]);

  const generatePayload = () => {
    setPayloadText(stringify(buildProblemReportPayload(form)));
    setResponseText("");
    setSuccess("Sample payload generated.");
    setError(null);
  };

  const submitPayload = async () => {
    setTesting(true);
    setError(null);
    setSuccess(null);
    setResponseText("");
    try {
      const parsedPayload = JSON.parse(payloadText);
      const created = await apiPost<ProblemReport>("/api/problem-reports", parsedPayload);
      setResponseText(stringify(created));
      setSuccess(`Problem report ${created.report_number} created.`);
      setNextNumber(await apiGet<string>("/api/problem-reports/next-number"));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to test problem report API.");
    } finally {
      setTesting(false);
    }
  };

  const copyText = async (text: string, label = "Copied") => {
    try {
      await navigator.clipboard.writeText(text);
      setSuccess(label);
      setError(null);
    } catch {
      setError("Clipboard copy failed.");
    }
  };

  const sampleAsset = assets.find((asset) => String(asset.id) === form.asset_id) || assets[0];
  const categoryOptions = masterCodes(masters.problemReportCategories, problemReportCategories);
  const priorityOptions = masterCodes(masters.workOrderPriorities, workOrderPriorities);
  const problemReportStatusItems = problemReportStatusMasters(masters.workOrderStatuses);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Integration</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">Problem Report API Documentation</h1>
          <p className="mt-1 max-w-3xl text-sm leading-6 text-gray-500 dark:text-gray-400">
            Dokumentasi singkat untuk developer yang ingin membuat Problem Report dari sistem eksternal. Work Order tetap dibuat dan dikelola dari aplikasi utama.
          </p>
        </div>
        <button className="secondary-button" disabled={loading} onClick={() => void refreshReferences()} type="button">
          <Icon name="refresh" />
          Refresh Data
        </button>
      </div>

      <Feedback error={error} success={success} />

      <div className="grid gap-4 lg:grid-cols-3">
        <InfoCard label="Base URL" value={baseUrl} />
        <InfoCard label="Auth" value="Bearer token required" />
        <InfoCard label="Next Report No" value={nextNumber || "-"} />
      </div>

      <section className="rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
        <div className="border-b border-gray-100 px-5 py-4 dark:border-white/[0.05]">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Endpoint Summary</h2>
        </div>
        <div className="grid gap-3 p-5 xl:grid-cols-5">
          {endpointDocs.map((item) => (
            <div className="rounded-lg border border-gray-200 bg-gray-50 p-4 dark:border-gray-800 dark:bg-white/[0.03]" key={item.path}>
              <span className="inline-flex rounded-md bg-brand-50 px-2 py-1 text-xs font-semibold text-brand-600 dark:bg-brand-500/10 dark:text-brand-400">{item.method}</span>
              <p className="mt-3 text-sm font-semibold text-gray-900 dark:text-white">{item.title}</p>
              <p className="mt-2 break-all font-mono text-xs text-gray-600 dark:text-gray-300">{item.path}</p>
              <p className="mt-2 text-xs leading-5 text-gray-500 dark:text-gray-400">{item.note}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
        <div className="border-b border-gray-100 px-5 py-4 dark:border-white/[0.05]">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Master Data Reference</h2>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Value seperti category, priority, dan status diambil dari endpoint GET berikut. Payload lama tetap boleh pakai code string, dan payload baru boleh pakai field *_id.
          </p>
        </div>
        <div className="max-w-full overflow-x-auto p-5">
          <table className="min-w-full text-left text-sm">
            <thead className="bg-gray-50 text-xs uppercase tracking-wide text-gray-500 dark:bg-white/[0.03] dark:text-gray-400">
              <tr>
                <th className="px-4 py-3 font-semibold">Payload Field</th>
                <th className="px-4 py-3 font-semibold">GET Endpoint</th>
                <th className="px-4 py-3 font-semibold">Usage</th>
                <th className="px-4 py-3 font-semibold">Codes</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
              {masterEndpointDocs.map((item) => (
                <MasterReferenceRow
                  field={item.field}
                  items={item.key === "workOrderStatuses" ? problemReportStatusItems : masterItems(masters[item.key], item.fallback)}
                  key={item.path}
                  path={item.path}
                  usage={item.usage}
                />
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.1fr)_minmax(380px,0.9fr)]">
        <section className="rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
          <div className="border-b border-gray-100 px-5 py-4 dark:border-white/[0.05]">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Create Problem Report Tester</h2>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Tester ini memakai token login user saat ini.</p>
          </div>
          <div className="grid gap-5 p-5 lg:grid-cols-2">
            <label className="block">
              <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Report Number</span>
              <input className="control-input" onChange={(event) => setForm((current) => ({ ...current, report_number: event.target.value }))} value={form.report_number} />
              <span className="mt-1 block text-xs text-gray-500 dark:text-gray-400">Kosongkan untuk auto-generate backend.</span>
            </label>
            <label className="block">
              <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Asset</span>
              <select className="control-input" onChange={(event) => setForm((current) => ({ ...current, asset_id: event.target.value }))} value={form.asset_id}>
                <option value="">Select Asset</option>
                {assets.map((asset) => (
                  <option key={asset.id} value={asset.id}>{asset.asset_code} - {asset.asset_name}</option>
                ))}
              </select>
            </label>
            <label className="block lg:col-span-2">
              <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Title</span>
              <input className="control-input" onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))} value={form.title} />
            </label>
            <label className="block">
              <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Category</span>
              <select className="control-input" onChange={(event) => setForm((current) => ({ ...current, category: event.target.value, downtime_start: event.target.value === "DOWNTIME" ? current.downtime_start || current.reported_at : "" }))} value={form.category}>
                {categoryOptions.map((item) => <option key={item} value={item}>{statusText(item)}</option>)}
              </select>
            </label>
            <label className="block">
              <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Priority</span>
              <select className="control-input" onChange={(event) => setForm((current) => ({ ...current, priority: event.target.value }))} value={form.priority}>
                {priorityOptions.map((item) => <option key={item} value={item}>{statusText(item)}</option>)}
              </select>
            </label>
            <label className="block">
              <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Reported By</span>
              <input className="control-input" onChange={(event) => setForm((current) => ({ ...current, reported_by: event.target.value }))} value={form.reported_by} />
            </label>
            <label className="block">
              <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Reported At</span>
              <input className="control-input" onChange={(event) => setForm((current) => ({ ...current, reported_at: event.target.value }))} type="datetime-local" value={form.reported_at} />
            </label>
            {form.category === "DOWNTIME" ? (
              <label className="block lg:col-span-2">
                <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Downtime Start</span>
                <input className="control-input" onChange={(event) => setForm((current) => ({ ...current, downtime_start: event.target.value }))} type="datetime-local" value={form.downtime_start} />
              </label>
            ) : null}
            <label className="block lg:col-span-2">
              <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">Description</span>
              <textarea className="control-textarea" onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} value={form.description} />
            </label>
          </div>
          <div className="flex flex-col gap-3 border-t border-gray-100 px-5 py-4 dark:border-white/[0.05] sm:flex-row sm:justify-end">
            <button className="secondary-button h-11" onClick={generatePayload} type="button">
              <Icon name="refresh" />
              Generate Payload
            </button>
            <button className="primary-button h-11" disabled={testing || loading} onClick={() => void submitPayload()} type="button">
              <Icon name="check" />
              {testing ? "Testing..." : "POST Test"}
            </button>
          </div>
        </section>

        <section className="rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
          <div className="border-b border-gray-100 px-5 py-4 dark:border-white/[0.05]">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Reference Data</h2>
          </div>
          <div className="space-y-5 p-5">
            <ReferenceGroup endpoint="/api/problem-report-categories" title="Problem Report Category" values={categoryOptions} />
            <ReferenceGroup endpoint="/api/work-order-priorities" title="Priority" values={priorityOptions} />
            <ReferenceGroup endpoint="/api/work-order-statuses" title="Problem Report Status" values={problemReportStatusItems.map((item) => item.code)} />
            {sampleAsset ? (
              <div className="rounded-lg border border-gray-200 bg-gray-50 p-4 dark:border-gray-800 dark:bg-white/[0.03]">
                <p className="text-xs font-semibold uppercase tracking-[0.14em] text-gray-500 dark:text-gray-400">Sample Asset</p>
                <p className="mt-2 text-sm font-semibold text-gray-900 dark:text-white">{sampleAsset.asset_code} - {sampleAsset.asset_name}</p>
                <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">asset_id: {sampleAsset.id} | area: {sampleAsset.area || "-"} | status: {sampleAsset.status}</p>
              </div>
            ) : null}
          </div>
        </section>
      </div>

      <div className="grid gap-5 xl:grid-cols-2">
        <PayloadEditor onChange={setPayloadText} onCopy={() => void copyText(payloadText, "Payload copied.")} title="Editable Problem Report Payload" value={payloadText} />
        <CodeBlock code={responseText || "// API response will appear here after POST Test."} onCopy={() => void copyText(responseText, "Response copied.")} title="POST Test Response" />
      </div>

      <div className="grid gap-5 xl:grid-cols-2">
        <CodeBlock code={curlCreateReport} onCopy={() => void copyText(curlCreateReport, "cURL copied.")} title="cURL: Create Problem Report" />
        <CodeBlock code={curlReadMasterData} onCopy={() => void copyText(curlReadMasterData, "Master data cURL copied.")} title="cURL: Read Master Data" />
      </div>

      <div className="grid gap-5 xl:grid-cols-2">
        <CodeBlock code={curlReadAssets} onCopy={() => void copyText(curlReadAssets, "Assets cURL copied.")} title="cURL: Read Assets" />
        <CodeBlock code={stringify(buildProblemReportPayload(form))} onCopy={() => void copyText(stringify(buildProblemReportPayload(form)), "Example payload copied.")} title="Problem Report Payload Shape" />
      </div>

      <section className="rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
        <div className="border-b border-gray-100 px-5 py-4 dark:border-white/[0.05]">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Existing Assets</h2>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Gunakan kolom id sebagai asset_id di payload API.</p>
        </div>
        <div className="max-w-full overflow-x-auto p-5">
          <table className="min-w-full text-left text-sm">
            <thead className="bg-gray-50 text-xs uppercase tracking-wide text-gray-500 dark:bg-white/[0.03] dark:text-gray-400">
              <tr>
                <th className="px-4 py-3 font-semibold">ID</th>
                <th className="px-4 py-3 font-semibold">Asset Code</th>
                <th className="px-4 py-3 font-semibold">Asset Name</th>
                <th className="px-4 py-3 font-semibold">Area</th>
                <th className="px-4 py-3 font-semibold">Criticality</th>
                <th className="px-4 py-3 font-semibold">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
              {assets.length ? assets.slice(0, 12).map((asset) => (
                <tr key={asset.id}>
                  <td className="px-4 py-3 font-semibold text-gray-900 dark:text-white">{asset.id}</td>
                  <td className="px-4 py-3 text-gray-700 dark:text-gray-300">{asset.asset_code}</td>
                  <td className="px-4 py-3 text-gray-700 dark:text-gray-300">{asset.asset_name}</td>
                  <td className="px-4 py-3 text-gray-700 dark:text-gray-300">{asset.area || "-"}</td>
                  <td className="px-4 py-3"><Badge value={asset.criticality_level} /></td>
                  <td className="px-4 py-3"><Badge value={asset.status} /></td>
                </tr>
              )) : (
                <tr><td className="px-4 py-8 text-center text-gray-500 dark:text-gray-400" colSpan={6}>No assets found.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}

function InfoCard({ label, value }: { label: string; value: string }) {
  return (
    <section className="rounded-lg border border-gray-200 bg-white p-5 shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
      <p className="text-xs font-semibold uppercase tracking-[0.14em] text-gray-500 dark:text-gray-400">{label}</p>
      <p className="mt-2 break-all text-sm font-semibold text-gray-900 dark:text-white">{value}</p>
    </section>
  );
}

function ReferenceGroup({ endpoint, title, values }: { endpoint?: string; title: string; values: readonly string[] }) {
  return (
    <div>
      <p className="mb-2 text-xs font-semibold uppercase tracking-[0.14em] text-gray-500 dark:text-gray-400">{title}</p>
      {endpoint ? <p className="mb-2 font-mono text-xs text-gray-500 dark:text-gray-400">{endpoint}</p> : null}
      <div className="flex flex-wrap gap-2">
        {values.map((value) => <Badge key={value} value={value} />)}
      </div>
    </div>
  );
}

function MasterReferenceRow({ field, items, path, usage }: { field: string; items: MasterLookup[]; path: string; usage: string }) {
  return (
    <tr>
      <td className="px-4 py-4 align-top font-semibold text-gray-900 dark:text-white">{field}</td>
      <td className="px-4 py-4 align-top font-mono text-xs text-brand-600 dark:text-brand-400">{path}</td>
      <td className="max-w-xs px-4 py-4 align-top text-gray-600 dark:text-gray-300">{usage}</td>
      <td className="px-4 py-4 align-top">
        <div className="flex flex-wrap gap-2">
          {items.map((item) => (
            <span className="inline-flex items-center gap-1 rounded-md border border-gray-200 px-2 py-1 text-xs font-semibold text-gray-700 dark:border-gray-700 dark:text-gray-200" key={`${field}-${item.id}-${item.code}`}>
              <span className="text-gray-400">#{item.id}</span>
              {item.code}
            </span>
          ))}
        </div>
      </td>
    </tr>
  );
}

function CodeBlock({ code, onCopy, title }: { code: string; onCopy: () => void; title: string }) {
  return (
    <section className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
      <div className="flex items-center justify-between gap-3 border-b border-gray-100 px-5 py-4 dark:border-white/[0.05]">
        <h2 className="text-sm font-semibold text-gray-900 dark:text-white">{title}</h2>
        <button className="secondary-button h-9 px-3" disabled={!code || code.startsWith("//")} onClick={onCopy} type="button">
          <Icon name="copy" className="h-4 w-4" />
          Copy
        </button>
      </div>
      <pre className="max-h-[360px] overflow-auto bg-gray-950 p-5 text-xs leading-6 text-gray-100">
        <code>{code}</code>
      </pre>
    </section>
  );
}

function PayloadEditor({ onChange, onCopy, title, value }: { onChange: (value: string) => void; onCopy: () => void; title: string; value: string }) {
  return (
    <section className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
      <div className="flex items-center justify-between gap-3 border-b border-gray-100 px-5 py-4 dark:border-white/[0.05]">
        <h2 className="text-sm font-semibold text-gray-900 dark:text-white">{title}</h2>
        <button className="secondary-button h-9 px-3" onClick={onCopy} type="button">
          <Icon name="copy" className="h-4 w-4" />
          Copy
        </button>
      </div>
      <textarea
        className="h-[360px] w-full resize-y bg-gray-950 p-5 font-mono text-xs leading-6 text-gray-100 outline-none placeholder:text-gray-500"
        onChange={(event) => onChange(event.target.value)}
        spellCheck={false}
        value={value}
      />
    </section>
  );
}
