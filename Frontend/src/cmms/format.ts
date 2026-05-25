import type { EntityValue } from "./types";

const numberFormatter = new Intl.NumberFormat("id-ID");
const currencyFormatter = new Intl.NumberFormat("id-ID", {
  currency: "IDR",
  maximumFractionDigits: 0,
  style: "currency",
});

export function formatNumber(value: EntityValue) {
  if (value === null || value === undefined || value === "") {
    return "-";
  }

  const numberValue = Number(value);
  return Number.isFinite(numberValue) ? numberFormatter.format(numberValue) : String(value);
}

export function formatCurrency(value: EntityValue) {
  if (value === null || value === undefined || value === "") {
    return "-";
  }

  const numberValue = Number(value);
  return Number.isFinite(numberValue) ? currencyFormatter.format(numberValue) : String(value);
}

export function formatDateTime(value: EntityValue) {
  if (!value) {
    return "-";
  }

  const date = new Date(String(value));
  if (Number.isNaN(date.getTime())) {
    return String(value);
  }

  return new Intl.DateTimeFormat("id-ID", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

export function formatDate(value: EntityValue) {
  if (!value) {
    return "-";
  }

  const date = new Date(String(value));
  if (Number.isNaN(date.getTime())) {
    return String(value);
  }

  return new Intl.DateTimeFormat("id-ID", { dateStyle: "medium" }).format(date);
}

export function statusText(value: EntityValue) {
  if (value === null || value === undefined || value === "") {
    return "-";
  }

  return String(value).replaceAll("_", " ");
}

export function toDateInput(value: EntityValue) {
  if (!value) {
    return "";
  }

  const date = new Date(String(value));
  if (Number.isNaN(date.getTime())) {
    return String(value).slice(0, 10);
  }

  return date.toISOString().slice(0, 10);
}

export function toDateTimeLocalInput(value: EntityValue) {
  if (!value) {
    return "";
  }

  const date = new Date(String(value));
  if (Number.isNaN(date.getTime())) {
    return String(value).slice(0, 16);
  }

  const offsetMs = date.getTimezoneOffset() * 60 * 1000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

export function nowDateTimeLocal() {
  return toDateTimeLocalInput(new Date().toISOString());
}
