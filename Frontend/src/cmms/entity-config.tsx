import type { ReactNode } from "react";
import { Badge } from "./ui";
import { formatCurrency, formatDate, formatDateTime, formatNumber, statusText } from "./format";
import { options, type SelectOption } from "./options";
import type { EntityRecord, EntityValue } from "./types";

export type EntityField = {
  name: string;
  label: string;
  type: "text" | "email" | "password" | "number" | "date" | "datetime-local" | "textarea" | "select" | "checkbox";
  required?: boolean;
  placeholder?: string;
  helper?: string;
  options?: SelectOption[];
  hideOnEdit?: boolean;
};

export type EntityFilter = {
  name: string;
  label: string;
  type: "text" | "number" | "select";
  options?: SelectOption[];
  placeholder?: string;
};

export type EntityColumn = {
  key: string;
  label: string;
  minWidth?: string;
  render?: (record: EntityRecord) => ReactNode;
};

export type EntityConfig = {
  title: string;
  eyebrow: string;
  description: string;
  endpoint: string;
  primaryKey?: string;
  fields: EntityField[];
  columns: EntityColumn[];
  filters?: EntityFilter[];
  defaultValues: EntityRecord;
  buildCreatePayload?: (values: EntityRecord) => EntityRecord;
  buildUpdatePayload?: (values: EntityRecord) => EntityRecord;
  allowDelete?: boolean;
};

function statusBadge(key: string) {
  return function StatusBadgeCell(record: EntityRecord) {
    return <Badge value={record[key]} />;
  };
}

function numberCell(key: string) {
  return function NumberCell(record: EntityRecord) {
    return <span className="font-medium">{formatNumber(record[key])}</span>;
  };
}

export const assetConfig: EntityConfig = {
  title: "Assets",
  eyebrow: "Master Data",
  description: "Machine and equipment registry",
  endpoint: "/api/assets",
  defaultValues: {
    asset_code: "",
    asset_name: "",
    asset_type: "",
    plant: "",
    area: "",
    production_line: "",
    location: "",
    manufacturer: "",
    model: "",
    serial_number: "",
    installation_date: "",
    criticality_level: "MEDIUM",
    status: "ACTIVE",
  },
  filters: [
    { name: "search", label: "Search", type: "text", placeholder: "Code or name" },
    { name: "status", label: "Status", type: "select", options: options.assetStatus },
  ],
  fields: [
    { name: "asset_code", label: "Asset Code", type: "text", required: true },
    { name: "asset_name", label: "Asset Name", type: "text", required: true },
    { name: "asset_type", label: "Asset Type", type: "text", required: true },
    { name: "plant", label: "Plant", type: "text", required: true },
    { name: "area", label: "Area", type: "text", required: true },
    { name: "production_line", label: "Production Line", type: "text", required: true },
    { name: "location", label: "Location", type: "text", required: true },
    { name: "manufacturer", label: "Manufacturer", type: "text" },
    { name: "model", label: "Model", type: "text" },
    { name: "serial_number", label: "Serial Number", type: "text" },
    { name: "installation_date", label: "Installation Date", type: "date" },
    { name: "criticality_level", label: "Criticality", type: "select", options: options.assetCriticality, required: true },
    { name: "status", label: "Status", type: "select", options: options.assetStatus, required: true },
  ],
  columns: [
    { key: "asset_code", label: "Code", minWidth: "min-w-36" },
    { key: "asset_name", label: "Asset", minWidth: "min-w-56" },
    { key: "asset_type", label: "Type", minWidth: "min-w-32" },
    { key: "production_line", label: "Line", minWidth: "min-w-44" },
    { key: "location", label: "Location", minWidth: "min-w-36" },
    { key: "criticality_level", label: "Criticality", render: statusBadge("criticality_level"), minWidth: "min-w-32" },
    { key: "status", label: "Status", render: statusBadge("status"), minWidth: "min-w-32" },
  ],
};

export const technicianConfig: EntityConfig = {
  title: "Technicians",
  eyebrow: "Master Data",
  description: "Maintenance technician roster",
  endpoint: "/api/technicians",
  defaultValues: { employee_no: "", name: "", email: "", phone: "", skill_type: "GENERAL", shift: "", status: "ACTIVE" },
  filters: [
    { name: "status", label: "Status", type: "select", options: options.technicianStatus },
    { name: "skill_type", label: "Skill", type: "select", options: options.technicianSkill },
  ],
  fields: [
    { name: "employee_no", label: "Employee No", type: "text", required: true },
    { name: "name", label: "Name", type: "text", required: true },
    { name: "email", label: "Email", type: "email" },
    { name: "phone", label: "Phone", type: "text" },
    { name: "skill_type", label: "Skill Type", type: "select", options: options.technicianSkill, required: true },
    { name: "shift", label: "Shift", type: "text", required: true },
    { name: "status", label: "Status", type: "select", options: options.technicianStatus, required: true },
  ],
  columns: [
    { key: "employee_no", label: "Employee", minWidth: "min-w-36" },
    { key: "name", label: "Name", minWidth: "min-w-48" },
    { key: "skill_type", label: "Skill", render: statusBadge("skill_type"), minWidth: "min-w-32" },
    { key: "shift", label: "Shift", minWidth: "min-w-24" },
    { key: "email", label: "Email", minWidth: "min-w-56" },
    { key: "status", label: "Status", render: statusBadge("status"), minWidth: "min-w-32" },
  ],
};

export const sparepartConfig: EntityConfig = {
  title: "Spareparts",
  eyebrow: "Inventory",
  description: "Sparepart stock and critical material",
  endpoint: "/api/spareparts",
  defaultValues: { part_code: "", part_name: "", category: "", unit: "PCS", stock_qty: 0, minimum_stock: 0, location: "", supplier: "", lead_time_days: "", price: "", is_critical: false },
  filters: [
    { name: "search", label: "Search", type: "text", placeholder: "Code or name" },
    { name: "low_stock_only", label: "Stock", type: "select", options: [{ label: "Low stock only", value: "true" }] },
  ],
  fields: [
    { name: "part_code", label: "Part Code", type: "text", required: true },
    { name: "part_name", label: "Part Name", type: "text", required: true },
    { name: "category", label: "Category", type: "text" },
    { name: "unit", label: "Unit", type: "text", required: true },
    { name: "stock_qty", label: "Stock Qty", type: "number", required: true },
    { name: "minimum_stock", label: "Minimum Stock", type: "number", required: true },
    { name: "location", label: "Location", type: "text" },
    { name: "supplier", label: "Supplier", type: "text" },
    { name: "lead_time_days", label: "Lead Time Days", type: "number" },
    { name: "price", label: "Price", type: "number" },
    { name: "is_critical", label: "Critical Part", type: "checkbox" },
  ],
  columns: [
    { key: "part_code", label: "Part Code", minWidth: "min-w-36" },
    { key: "part_name", label: "Part", minWidth: "min-w-56" },
    { key: "category", label: "Category", minWidth: "min-w-32" },
    { key: "stock_qty", label: "Stock", render: numberCell("stock_qty"), minWidth: "min-w-28" },
    { key: "minimum_stock", label: "Min", render: numberCell("minimum_stock"), minWidth: "min-w-24" },
    { key: "price", label: "Price", render: (record) => formatCurrency(record.price), minWidth: "min-w-32" },
    { key: "is_critical", label: "Critical", render: (record) => <Badge value={record.is_critical ? "YES" : "NO"} />, minWidth: "min-w-28" },
  ],
};

export const userConfig: EntityConfig = {
  title: "Users",
  eyebrow: "System",
  description: "Application account and role access",
  endpoint: "/api/users",
  defaultValues: { username: "", password: "", full_name: "", email: "", phone: "", role: "VIEWER", status: "ACTIVE" },
  filters: [
    { name: "search", label: "Search", type: "text", placeholder: "Username or name" },
    { name: "role", label: "Role", type: "select", options: options.appUserRole },
    { name: "status", label: "Status", type: "select", options: options.appUserStatus },
  ],
  fields: [
    { name: "username", label: "Username", type: "text", required: true },
    { name: "password", label: "Password", type: "password", required: true, hideOnEdit: true },
    { name: "full_name", label: "Full Name", type: "text", required: true },
    { name: "email", label: "Email", type: "email" },
    { name: "phone", label: "Phone", type: "text" },
    { name: "role", label: "Role", type: "select", options: options.appUserRole, required: true },
    { name: "status", label: "Status", type: "select", options: options.appUserStatus, required: true },
  ],
  columns: [
    { key: "username", label: "Username", minWidth: "min-w-36" },
    { key: "full_name", label: "Full Name", minWidth: "min-w-52" },
    { key: "email", label: "Email", minWidth: "min-w-56" },
    { key: "role", label: "Role", render: statusBadge("role"), minWidth: "min-w-32" },
    { key: "status", label: "Status", render: statusBadge("status"), minWidth: "min-w-32" },
    { key: "last_login_at", label: "Last Login", render: (record) => formatDateTime(record.last_login_at), minWidth: "min-w-44" },
  ],
  buildUpdatePayload: (values) => {
    const result = { ...values };
    delete result.password;
    return result;
  },
};

export const downtimeLogConfig: EntityConfig = {
  title: "Downtime Logs",
  eyebrow: "Operations",
  description: "Asset downtime history",
  endpoint: "/api/downtime-logs",
  defaultValues: { asset_id: "", work_order_id: "", downtime_category: "OTHER", start_time: "", end_time: "", duration_minutes: "", description: "" },
  filters: [
    { name: "asset_id", label: "Asset ID", type: "number" },
    { name: "work_order_id", label: "WO ID", type: "number" },
  ],
  fields: [
    { name: "asset_id", label: "Asset ID", type: "number", required: true },
    { name: "work_order_id", label: "Work Order ID", type: "number" },
    { name: "downtime_category", label: "Category", type: "select", options: options.downtimeCategory, required: true },
    { name: "start_time", label: "Start Time", type: "datetime-local", required: true },
    { name: "end_time", label: "End Time", type: "datetime-local" },
    { name: "duration_minutes", label: "Duration Minutes", type: "number" },
    { name: "description", label: "Description", type: "textarea" },
  ],
  columns: [
    { key: "asset_id", label: "Asset ID", render: numberCell("asset_id"), minWidth: "min-w-28" },
    { key: "work_order_id", label: "WO ID", render: numberCell("work_order_id"), minWidth: "min-w-28" },
    { key: "downtime_category", label: "Category", render: statusBadge("downtime_category"), minWidth: "min-w-40" },
    { key: "start_time", label: "Start", render: (record) => formatDateTime(record.start_time), minWidth: "min-w-44" },
    { key: "end_time", label: "End", render: (record) => formatDateTime(record.end_time), minWidth: "min-w-44" },
    { key: "duration_minutes", label: "Minutes", render: numberCell("duration_minutes"), minWidth: "min-w-28" },
    { key: "description", label: "Description", minWidth: "min-w-64" },
  ],
};

export const failureCodeConfig: EntityConfig = {
  title: "Failure Codes",
  eyebrow: "Master Data",
  description: "Failure classification used by work orders",
  endpoint: "/api/failure-codes",
  defaultValues: { code: "", name: "", category: "", description: "" },
  fields: [
    { name: "code", label: "Code", type: "text", required: true },
    { name: "name", label: "Name", type: "text", required: true },
    { name: "category", label: "Category", type: "text" },
    { name: "description", label: "Description", type: "textarea" },
  ],
  columns: [
    { key: "code", label: "Code", minWidth: "min-w-32" },
    { key: "name", label: "Name", minWidth: "min-w-56" },
    { key: "category", label: "Category", minWidth: "min-w-40" },
    { key: "description", label: "Description", minWidth: "min-w-72" },
  ],
};

export const rootCauseConfig: EntityConfig = {
  title: "Root Causes",
  eyebrow: "Master Data",
  description: "Root cause library for maintenance analysis",
  endpoint: "/api/root-causes",
  defaultValues: { code: "", name: "", description: "" },
  fields: [
    { name: "code", label: "Code", type: "text", required: true },
    { name: "name", label: "Name", type: "text", required: true },
    { name: "description", label: "Description", type: "textarea" },
  ],
  columns: [
    { key: "code", label: "Code", minWidth: "min-w-32" },
    { key: "name", label: "Name", minWidth: "min-w-56" },
    { key: "description", label: "Description", minWidth: "min-w-80" },
  ],
};

export function defaultRender(record: EntityRecord, key: string) {
  const value: EntityValue = record[key];

  if (typeof value === "boolean") {
    return value ? "Yes" : "No";
  }

  if (key.endsWith("_at") || key.endsWith("_time")) {
    return formatDateTime(value);
  }

  if (key.endsWith("_date")) {
    return formatDate(value);
  }

  return statusText(value);
}
