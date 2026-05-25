import type { EntityConfig } from "./entity-config";
import type { EntityRecord } from "./types";

function normalizeCodePayload(values: EntityRecord) {
  return {
    ...values,
    code: String(values.code || "").trim().replace(/\s+/g, "_").toUpperCase(),
    name: String(values.name || "").trim(),
  };
}

const baseFields = [
  { name: "code", label: "Code", type: "text" as const, required: true, helper: "Contoh: BREAKDOWN, MEDIUM, OPEN" },
  { name: "name", label: "Name", type: "text" as const, required: true },
  { name: "is_active", label: "Active", type: "checkbox" as const },
];

const baseColumns = [
  { key: "code", label: "Code", minWidth: "min-w-44" },
  { key: "name", label: "Name", minWidth: "min-w-52" },
  { key: "is_active", label: "Active", minWidth: "min-w-28" },
];

export const masterDataConfigs: EntityConfig[] = [
  {
    title: "Maintenance Types",
    eyebrow: "CMMS Master",
    description: "Master jenis maintenance untuk Work Order.",
    endpoint: "/api/maintenance-types",
    defaultValues: { code: "", name: "", is_active: true },
    fields: baseFields,
    columns: baseColumns,
    buildCreatePayload: normalizeCodePayload,
    buildUpdatePayload: normalizeCodePayload,
  },
  {
    title: "Work Order Priorities",
    eyebrow: "CMMS Master",
    description: "Master priority untuk Problem Report dan Work Order.",
    endpoint: "/api/work-order-priorities",
    defaultValues: { code: "", name: "", level: 1, is_active: true },
    fields: [
      { name: "code", label: "Code", type: "text", required: true, helper: "Contoh: LOW, MEDIUM, HIGH, URGENT" },
      { name: "name", label: "Name", type: "text", required: true },
      { name: "level", label: "Level", type: "number", required: true },
      { name: "is_active", label: "Active", type: "checkbox" },
    ],
    columns: [
      { key: "code", label: "Code", minWidth: "min-w-36" },
      { key: "name", label: "Name", minWidth: "min-w-52" },
      { key: "level", label: "Level", minWidth: "min-w-24" },
      { key: "is_active", label: "Active", minWidth: "min-w-28" },
    ],
    buildCreatePayload: normalizeCodePayload,
    buildUpdatePayload: normalizeCodePayload,
  },
  {
    title: "Work Order Statuses",
    eyebrow: "CMMS Master",
    description: "Master status untuk flow Work Order dan Problem Report.",
    endpoint: "/api/work-order-statuses",
    defaultValues: { code: "", name: "", sequence: 1, is_active: true },
    fields: [
      { name: "code", label: "Code", type: "text", required: true, helper: "Contoh: OPEN, ASSIGNED, IN_PROGRESS" },
      { name: "name", label: "Name", type: "text", required: true },
      { name: "sequence", label: "Sequence", type: "number", required: true },
      { name: "is_active", label: "Active", type: "checkbox" },
    ],
    columns: [
      { key: "code", label: "Code", minWidth: "min-w-40" },
      { key: "name", label: "Name", minWidth: "min-w-52" },
      { key: "sequence", label: "Sequence", minWidth: "min-w-28" },
      { key: "is_active", label: "Active", minWidth: "min-w-28" },
    ],
    buildCreatePayload: normalizeCodePayload,
    buildUpdatePayload: normalizeCodePayload,
  },
  {
    title: "Downtime Categories",
    eyebrow: "CMMS Master",
    description: "Master category untuk Downtime Log.",
    endpoint: "/api/downtime-categories",
    defaultValues: { code: "", name: "", is_active: true },
    fields: baseFields,
    columns: baseColumns,
    buildCreatePayload: normalizeCodePayload,
    buildUpdatePayload: normalizeCodePayload,
  },
  {
    title: "Problem Report Categories",
    eyebrow: "CMMS Master",
    description: "Master category untuk Problem Report.",
    endpoint: "/api/problem-report-categories",
    defaultValues: { code: "", name: "", is_active: true },
    fields: baseFields,
    columns: baseColumns,
    buildCreatePayload: normalizeCodePayload,
    buildUpdatePayload: normalizeCodePayload,
  },
  {
    title: "Preventive Schedule Types",
    eyebrow: "CMMS Master",
    description: "Master schedule type untuk Preventive Schedule.",
    endpoint: "/api/preventive-schedule-types",
    defaultValues: { code: "", name: "", is_active: true },
    fields: baseFields,
    columns: baseColumns,
    buildCreatePayload: normalizeCodePayload,
    buildUpdatePayload: normalizeCodePayload,
  },
  {
    title: "Frequency Types",
    eyebrow: "CMMS Master",
    description: "Master frequency untuk Preventive Schedule.",
    endpoint: "/api/frequency-types",
    defaultValues: { code: "", name: "", interval_days: 1, is_active: true },
    fields: [
      { name: "code", label: "Code", type: "text", required: true, helper: "Contoh: DAILY, WEEKLY, MONTHLY" },
      { name: "name", label: "Name", type: "text", required: true },
      { name: "interval_days", label: "Interval Days", type: "number", required: true },
      { name: "is_active", label: "Active", type: "checkbox" },
    ],
    columns: [
      { key: "code", label: "Code", minWidth: "min-w-36" },
      { key: "name", label: "Name", minWidth: "min-w-52" },
      { key: "interval_days", label: "Interval Days", minWidth: "min-w-32" },
      { key: "is_active", label: "Active", minWidth: "min-w-28" },
    ],
    buildCreatePayload: normalizeCodePayload,
    buildUpdatePayload: normalizeCodePayload,
  },
];
