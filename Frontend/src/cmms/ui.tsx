"use client";

import { useEffect, useId, useState, type ChangeEvent, type ChangeEventHandler, type ReactNode } from "react";
import DatePicker from "@/components/form/date-picker";
import { statusText } from "./format";
import type { SelectOption } from "./options";
import type { EntityValue } from "./types";

export function Badge({ value }: { value: EntityValue }) {
  const key = String(value || "").toUpperCase();
  const className = ["ACTIVE", "OPEN", "ASSIGNED", "IN_PROGRESS", "HIGH", "CRITICAL", "URGENT"].includes(key)
    ? "border-blue-light-500/20 bg-blue-light-50 text-blue-light-700 dark:border-blue-light-500/30 dark:bg-blue-light-500/10 dark:text-blue-light-500"
    : ["COMPLETED", "CLOSED", "LOW", "MECHANICAL", "PREVENTIVE", "QUALITY"].includes(key)
      ? "border-success-500/20 bg-success-50 text-success-700 dark:border-success-500/30 dark:bg-success-500/10 dark:text-success-500"
      : ["PENDING", "MEDIUM", "UNDER_MAINTENANCE", "ELECTRICAL", "CORRECTIVE", "SAFETY"].includes(key)
        ? "border-warning-500/25 bg-warning-50 text-warning-700 dark:border-warning-500/30 dark:bg-warning-500/10 dark:text-warning-500"
        : ["INACTIVE", "RETIRED", "CANCELLED", "BREAKDOWN", "DOWNTIME"].includes(key)
          ? "border-error-500/20 bg-error-50 text-error-700 dark:border-error-500/30 dark:bg-error-500/10 dark:text-error-500"
          : "border-gray-200 bg-gray-50 text-gray-600 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300";

  return <span className={`inline-flex w-fit items-center rounded-full border px-2.5 py-1 text-xs font-semibold ${className}`}>{statusText(value)}</span>;
}

export function Card({ children, className = "" }: { children: ReactNode; className?: string }) {
  return <section className={`rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-gray-800 dark:bg-gray-900 ${className}`}>{children}</section>;
}

export function CardHeader({ title, description, action }: { title: string; description?: string; action?: ReactNode }) {
  return (
    <div className="flex flex-col gap-3 border-b border-gray-100 px-5 py-4 dark:border-gray-800 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 className="text-lg font-semibold text-gray-900 dark:text-white">{title}</h2>
        {description ? <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{description}</p> : null}
      </div>
      {action}
    </div>
  );
}

export function TablePanel({
  children,
  footer,
  toolbarLeft,
  toolbarRight,
}: {
  children: ReactNode;
  footer?: ReactNode;
  toolbarLeft?: ReactNode;
  toolbarRight?: ReactNode;
}) {
  return (
    <section className="overflow-hidden rounded-md border border-gray-200 bg-white shadow-theme-sm dark:border-white/[0.05] dark:bg-white/[0.03]">
      {(toolbarLeft || toolbarRight) ? (
        <div className="border-b border-gray-100 px-4 py-4 dark:border-white/[0.05]">
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <div className="flex flex-wrap items-center gap-3">{toolbarLeft}</div>
            {toolbarRight ? <div className="flex flex-wrap items-center gap-3 lg:justify-end">{toolbarRight}</div> : null}
          </div>
        </div>
      ) : null}

      <div className="mx-4 mb-4 mt-2 overflow-hidden rounded-sm border border-gray-100 dark:border-white/[0.05]">
        <div className="max-w-full overflow-x-auto">{children}</div>
      </div>

      {footer ? <div className="border-t border-gray-100 px-4 py-4 dark:border-white/[0.05]">{footer}</div> : null}
    </section>
  );
}

export function EntriesSelect({ value, onChange }: { value: number; onChange: (value: number) => void }) {
  return (
    <label className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
      Show
      <select
        className="h-10 rounded-lg border border-gray-300 bg-transparent px-3 py-2 text-sm font-medium text-gray-800 outline-none focus:border-brand-300 focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90"
        onChange={(event) => onChange(Number(event.target.value))}
        value={value}
      >
        {[10, 25, 50, 100].map((limit) => (
          <option key={limit} value={limit}>{limit}</option>
        ))}
      </select>
      entries
    </label>
  );
}

export function InlineSelect({
  label,
  onChange,
  options,
  placeholder = "All",
  value,
}: {
  label?: string;
  onChange: (value: string) => void;
  options: SelectOption[];
  placeholder?: string;
  value: string;
}) {
  return (
    <label className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
      {label ? <span>{label}</span> : null}
      <select
        className="h-10 min-w-36 rounded-lg border border-gray-300 bg-transparent px-3 py-2 text-sm font-medium text-gray-800 outline-none focus:border-brand-300 focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90"
        onChange={(event) => onChange(event.target.value)}
        value={value}
      >
        <option value="">{placeholder}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>{option.label}</option>
        ))}
      </select>
    </label>
  );
}

export function InlineSearch({
  debounceMs = 300,
  onChange,
  placeholder = "Search",
  value,
}: {
  debounceMs?: number;
  onChange: (value: string) => void;
  placeholder?: string;
  value: string;
}) {
  const [draftValue, setDraftValue] = useState(value);

  useEffect(() => {
    setDraftValue(value);
  }, [value]);

  useEffect(() => {
    if (draftValue === value) {
      return;
    }

    const timeoutId = window.setTimeout(() => {
      onChange(draftValue);
    }, debounceMs);

    return () => window.clearTimeout(timeoutId);
  }, [debounceMs, draftValue, onChange, value]);

  return (
    <label className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
      Search
      <input
        className="h-10 w-full rounded-lg border border-gray-300 bg-transparent px-3 py-2 text-sm text-gray-800 outline-none placeholder:text-gray-400 focus:border-brand-300 focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 sm:w-64"
        onChange={(event) => setDraftValue(event.target.value)}
        placeholder={placeholder}
        value={draftValue}
      />
    </label>
  );
}

export function InlineInput({
  label,
  onChange,
  placeholder,
  type = "text",
  value,
}: {
  label: string;
  onChange: (value: string) => void;
  placeholder?: string;
  type?: string;
  value: string;
}) {
  const pickerId = `cmms-inline-${label}-${useId()}`.replace(/[^a-zA-Z0-9_-]/g, "");

  if (type === "date" || type === "datetime-local") {
    return (
      <label className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
        {label}
        <DatePicker
          className="h-10 w-40 bg-transparent pr-10 dark:bg-gray-900"
          dateFormat={type === "datetime-local" ? "Y-m-d H:i" : "Y-m-d"}
          defaultDate={value || undefined}
          enableTime={type === "datetime-local"}
          id={pickerId}
          onChange={([date]) => onChange(date ? formatPickerDate(date, type === "datetime-local") : "")}
          placeholder={placeholder || "Select date"}
        />
      </label>
    );
  }

  return (
    <label className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
      {label}
      <input
        className="h-10 w-36 rounded-lg border border-gray-300 bg-transparent px-3 py-2 text-sm text-gray-800 outline-none placeholder:text-gray-400 focus:border-brand-300 focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90"
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        type={type}
        value={value}
      />
    </label>
  );
}

export function DataTableFooter({
  page,
  pageSize,
  total,
  onPageChange,
}: {
  page: number;
  pageSize: number;
  total: number;
  onPageChange: (page: number) => void;
}) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const first = total > 0 ? (page - 1) * pageSize + 1 : 0;
  const last = total > 0 ? Math.min(page * pageSize, total) : 0;
  const pages = Array.from({ length: Math.min(3, totalPages) }, (_, index) => {
    const start = Math.min(Math.max(1, page - 1), Math.max(1, totalPages - 2));
    return start + index;
  }).filter((item) => item <= totalPages);

  return (
    <div className="flex flex-col gap-3 text-sm text-gray-500 dark:text-gray-400 sm:flex-row sm:items-center sm:justify-between">
      <span>Showing {first} to {last} of {total} entries</span>
      <div className="flex items-center gap-2">
        <button
          className="h-10 rounded-lg border border-gray-300 px-3 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-white/[0.04]"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
          type="button"
        >
          Prev
        </button>
        {pages.map((item) => (
          <button
            className={`h-10 min-w-10 rounded-lg border px-3 text-sm font-semibold transition-colors ${item === page ? "border-brand-500 bg-brand-500 text-white" : "border-gray-300 text-gray-700 hover:bg-gray-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-white/[0.04]"}`}
            key={item}
            onClick={() => onPageChange(item)}
            type="button"
          >
            {item}
          </button>
        ))}
        <button
          className="h-10 rounded-lg border border-gray-300 px-3 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-white/[0.04]"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
          type="button"
        >
          Next
        </button>
      </div>
    </div>
  );
}

export function Feedback({ error, success }: { error?: string | null; success?: string | null }) {
  if (!error && !success) {
    return null;
  }

  return (
    <div
      className={`rounded-lg border px-4 py-3 text-sm ${
        error
          ? "border-error-500/20 bg-error-50 text-error-700 dark:border-error-500/30 dark:bg-error-500/10 dark:text-error-500"
          : "border-success-500/20 bg-success-50 text-success-700 dark:border-success-500/30 dark:bg-success-500/10 dark:text-success-500"
      }`}
      role="status"
    >
      {error || success}
    </div>
  );
}

function FieldShell({ label, children, required, helper }: { label: string; children: ReactNode; required?: boolean; helper?: string }) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">
        {label}
        {required ? <span className="text-error-500"> *</span> : null}
      </span>
      {children}
      {helper ? <span className="mt-1 block text-xs text-gray-500 dark:text-gray-400">{helper}</span> : null}
    </label>
  );
}

function formatPickerDate(date: Date, enableTime: boolean) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  if (!enableTime) {
    return `${year}-${month}-${day}`;
  }

  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function buildInputChangeEvent(name: string, value: string) {
  return {
    target: { name, value },
    currentTarget: { name, value },
  } as ChangeEvent<HTMLInputElement>;
}

export function TextInput({
  label,
  name,
  value,
  type = "text",
  placeholder,
  required,
  helper,
  onChange,
}: {
  label: string;
  name: string;
  value: string | number;
  type?: string;
  placeholder?: string;
  required?: boolean;
  helper?: string;
  onChange: ChangeEventHandler<HTMLInputElement>;
}) {
  const pickerId = `cmms-field-${name}-${useId()}`.replace(/[^a-zA-Z0-9_-]/g, "");
  const isDatePicker = type === "date" || type === "datetime-local";

  if (isDatePicker) {
    return (
      <FieldShell helper={helper} label={label} required={required}>
        <DatePicker
          className="control-input pr-10"
          dateFormat={type === "datetime-local" ? "Y-m-d H:i" : "Y-m-d"}
          defaultDate={value ? String(value) : undefined}
          enableTime={type === "datetime-local"}
          id={pickerId}
          onChange={([date]) => onChange(buildInputChangeEvent(name, date ? formatPickerDate(date, type === "datetime-local") : ""))}
          placeholder={placeholder || (type === "datetime-local" ? "Select date and time" : "Select date")}
        />
      </FieldShell>
    );
  }

  return (
    <FieldShell helper={helper} label={label} required={required}>
      <input className="control-input" name={name} onChange={onChange} placeholder={placeholder} required={required} type={type} value={value} />
    </FieldShell>
  );
}

export function SelectInput({
  label,
  name,
  value,
  options,
  required,
  helper,
  placeholder = "Select",
  onChange,
}: {
  label: string;
  name: string;
  value: string | number;
  options: SelectOption[];
  required?: boolean;
  helper?: string;
  placeholder?: string;
  onChange: ChangeEventHandler<HTMLSelectElement>;
}) {
  return (
    <FieldShell helper={helper} label={label} required={required}>
      <select className="control-input" name={name} onChange={onChange} required={required} value={value}>
        <option disabled={required} value="">{placeholder}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </FieldShell>
  );
}

export function TextareaInput({
  label,
  name,
  value,
  placeholder,
  required,
  helper,
  onChange,
}: {
  label: string;
  name: string;
  value: string | number;
  placeholder?: string;
  required?: boolean;
  helper?: string;
  onChange: ChangeEventHandler<HTMLTextAreaElement>;
}) {
  return (
    <FieldShell helper={helper} label={label} required={required}>
      <textarea className="control-textarea" name={name} onChange={onChange} placeholder={placeholder} required={required} value={value} />
    </FieldShell>
  );
}

export function CheckboxInput({
  label,
  name,
  checked,
  helper,
  onChange,
}: {
  label: string;
  name: string;
  checked: boolean;
  helper?: string;
  onChange: ChangeEventHandler<HTMLInputElement>;
}) {
  return (
    <label className="flex items-start gap-3 rounded-lg border border-gray-200 bg-white px-3 py-3 dark:border-gray-700 dark:bg-gray-900">
      <input checked={checked} className="mt-1 h-4 w-4 rounded border-gray-300 text-brand-500 focus:ring-brand-500 dark:border-gray-600" name={name} onChange={onChange} type="checkbox" />
      <span>
        <span className="block text-sm font-medium text-gray-700 dark:text-gray-300">{label}</span>
        {helper ? <span className="mt-1 block text-xs text-gray-500 dark:text-gray-400">{helper}</span> : null}
      </span>
    </label>
  );
}
