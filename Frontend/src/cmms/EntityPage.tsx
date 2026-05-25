"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import { ConfirmModal } from "@/components/ui/modal";
import { Icon } from "./icons";
import { apiDelete, apiGet, apiPost, apiPut, buildQuery } from "./api";
import { defaultRender, type EntityConfig, type EntityField } from "./entity-config";
import { toDateInput, toDateTimeLocalInput } from "./format";
import { Card, CardHeader, CheckboxInput, DataTableFooter, EntriesSelect, Feedback, InlineInput, InlineSearch, InlineSelect, SelectInput, TablePanel, TextareaInput, TextInput } from "./ui";
import type { EntityRecord, EntityValue } from "./types";

function valueToInput(value: EntityValue, field: EntityField) {
  if (field.type === "date") {
    return toDateInput(value);
  }

  if (field.type === "datetime-local") {
    return toDateTimeLocalInput(value);
  }

  return value ?? "";
}

function formFromRecord(config: EntityConfig, record?: EntityRecord) {
  const values: EntityRecord = { ...config.defaultValues };
  config.fields.forEach((field) => {
    values[field.name] = valueToInput(record ? record[field.name] : config.defaultValues[field.name], field);
  });
  return values;
}

function filtersFromSearchParams(config: EntityConfig, searchParams: Pick<URLSearchParams, "get">) {
  const values: EntityRecord = {};

  config.filters?.forEach((filter) => {
    const value = searchParams.get(filter.name);
    if (value) {
      values[filter.name] = value;
    }
  });

  return values;
}

function payloadFromForm(fields: EntityField[], values: EntityRecord, editing: boolean) {
  const payload: EntityRecord = {};
  fields.forEach((field) => {
    if (editing && field.hideOnEdit) {
      return;
    }

    const raw = values[field.name];
    if (field.type === "checkbox") {
      payload[field.name] = Boolean(raw);
      return;
    }

    if (field.type === "number") {
      payload[field.name] = raw === "" || raw === undefined || raw === null ? null : Number(raw);
      return;
    }

    payload[field.name] = !field.required && raw === "" ? null : raw;
  });
  return payload;
}

function recordLabel(record: EntityRecord | null, config: EntityConfig) {
  if (!record) {
    return "this data";
  }

  const firstColumnKey = config.columns[0]?.key;
  const preferredKeys = [firstColumnKey, "name", "title", "asset_name", "part_name", "username", "code"].filter(Boolean) as string[];
  const value = preferredKeys.map((key) => record[key]).find((item) => item !== undefined && item !== null && item !== "");
  return value ? String(value) : "this data";
}

export function EntityPage({ config }: { config: EntityConfig }) {
  const searchParams = useSearchParams();
  const queryKey = searchParams.toString();
  const primaryKey = config.primaryKey || "id";
  const [records, setRecords] = useState<EntityRecord[]>([]);
  const [filters, setFilters] = useState<EntityRecord>(() => filtersFromSearchParams(config, searchParams));
  const [formValues, setFormValues] = useState<EntityRecord>(() => formFromRecord(config));
  const [editingRecord, setEditingRecord] = useState<EntityRecord | null>(null);
  const [recordToDelete, setRecordToDelete] = useState<EntityRecord | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const listPath = useMemo(() => `${config.endpoint}${buildQuery(filters)}`, [config.endpoint, filters]);

  useEffect(() => {
    setFilters(filtersFromSearchParams(config, new URLSearchParams(queryKey)));
  }, [config, queryKey]);

  const loadRecords = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setRecords((await apiGet<EntityRecord[]>(listPath)) || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load data.");
    } finally {
      setLoading(false);
    }
  }, [listPath]);

  useEffect(() => {
    void loadRecords();
  }, [loadRecords]);

  useEffect(() => {
    setPage(1);
  }, [records.length, pageSize, filters]);

  const visibleRecords = useMemo(() => {
    const start = (page - 1) * pageSize;
    return records.slice(start, start + pageSize);
  }, [page, pageSize, records]);

  const searchFilter = config.filters?.find((filter) => filter.name === "search");
  const toolbarFilters = config.filters?.filter((filter) => filter.name !== "search") || [];

  function updateField(name: string, value: EntityValue) {
    setFormValues((current) => ({ ...current, [name]: value }));
  }

  function openCreate() {
    setEditingRecord(null);
    setFormValues(formFromRecord(config));
    setFormOpen(true);
    setError(null);
    setSuccess(null);
  }

  function openEdit(record: EntityRecord) {
    setEditingRecord(record);
    setFormValues(formFromRecord(config, record));
    setFormOpen(true);
    setError(null);
    setSuccess(null);
  }

  async function saveRecord(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      const editing = Boolean(editingRecord);
      const basePayload = payloadFromForm(config.fields, formValues, editing);
      const payload = editing ? config.buildUpdatePayload?.(basePayload) || basePayload : config.buildCreatePayload?.(basePayload) || basePayload;
      if (editingRecord) {
        await apiPut<EntityRecord>(`${config.endpoint}/${editingRecord[primaryKey]}`, payload);
        setSuccess("Data updated successfully.");
      } else {
        await apiPost<EntityRecord>(config.endpoint, payload);
        setSuccess("Data created successfully.");
      }
      setFormOpen(false);
      setEditingRecord(null);
      await loadRecords();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save data.");
    } finally {
      setSaving(false);
    }
  }

  function closeDeleteModal() {
    if (deleting) {
      return;
    }

    setRecordToDelete(null);
  }

  async function confirmDeleteRecord() {
    if (!recordToDelete) {
      return;
    }

    setDeleting(true);
    setError(null);
    setSuccess(null);
    try {
      await apiDelete<string>(`${config.endpoint}/${recordToDelete[primaryKey]}`);
      setSuccess("Data deleted successfully.");
      setRecordToDelete(null);
      await loadRecords();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete data.");
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">{config.eyebrow}</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">{config.title}</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{config.description}</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <button className="secondary-button" disabled={loading} onClick={() => void loadRecords()} type="button">
            <Icon name="refresh" />
            Refresh
          </button>
        </div>
      </div>

      <Feedback error={error} success={success} />

      {formOpen ? (
        <Card>
          <CardHeader action={<button className="icon-button" onClick={() => setFormOpen(false)} type="button"><Icon name="x" /></button>} description={editingRecord ? "Update existing record" : "Create new record"} title={editingRecord ? `Edit ${config.title}` : `Add ${config.title}`} />
          <form className="grid grid-cols-1 gap-4 p-5 md:grid-cols-2 xl:grid-cols-3" onSubmit={(event) => void saveRecord(event)}>
            {config.fields
              .filter((field) => !(editingRecord && field.hideOnEdit))
              .map((field) => {
                const value = formValues[field.name] ?? "";
                if (field.type === "textarea") {
                  return <div className="md:col-span-2 xl:col-span-3" key={field.name}><TextareaInput helper={field.helper} label={field.label} name={field.name} onChange={(event) => updateField(field.name, event.target.value)} placeholder={field.placeholder} required={field.required} value={String(value)} /></div>;
                }
                if (field.type === "select") {
                  return <SelectInput helper={field.helper} key={field.name} label={field.label} name={field.name} onChange={(event) => updateField(field.name, event.target.value)} options={field.options || []} required={field.required} value={String(value)} />;
                }
                if (field.type === "checkbox") {
                  return <CheckboxInput checked={Boolean(value)} helper={field.helper} key={field.name} label={field.label} name={field.name} onChange={(event) => updateField(field.name, event.target.checked)} />;
                }
                return <TextInput helper={field.helper} key={field.name} label={field.label} name={field.name} onChange={(event) => updateField(field.name, event.target.value)} placeholder={field.placeholder} required={field.required} type={field.type} value={String(value)} />;
              })}
            <div className="flex flex-wrap items-center gap-2 md:col-span-2 xl:col-span-3">
              <button className="primary-button" disabled={saving} type="submit"><Icon name="check" />Save</button>
              <button className="secondary-button" onClick={() => setFormOpen(false)} type="button">Cancel</button>
            </div>
          </form>
        </Card>
      ) : null}

      <TablePanel
        footer={<DataTableFooter onPageChange={setPage} page={page} pageSize={pageSize} total={records.length} />}
        toolbarLeft={(
          <>
            <EntriesSelect onChange={(value) => { setPageSize(value); setPage(1); }} value={pageSize} />
            {toolbarFilters.map((filter) => {
              const value = String(filters[filter.name] ?? "");
              if (filter.type === "select") {
                return <InlineSelect key={filter.name} onChange={(nextValue) => setFilters((current) => ({ ...current, [filter.name]: nextValue }))} options={filter.options || []} placeholder={`All ${filter.label}`} value={value} />;
              }
              return <InlineInput key={filter.name} label={filter.label} onChange={(nextValue) => setFilters((current) => ({ ...current, [filter.name]: nextValue }))} placeholder={filter.placeholder} type={filter.type} value={value} />;
            })}
            <button className="primary-button" onClick={openCreate} type="button">
              <Icon name="plus" />
              Create
            </button>
          </>
        )}
        toolbarRight={searchFilter ? (
          <InlineSearch onChange={(value) => setFilters((current) => ({ ...current, [searchFilter.name]: value }))} placeholder={searchFilter.placeholder || `Search ${config.title.toLowerCase()}`} value={String(filters[searchFilter.name] ?? "")} />
        ) : null}
      >
          <table className="min-w-full">
            <thead className="bg-[#6D8AF3] text-white dark:bg-[#6D8AF3]/90">
              <tr>
                {config.columns.map((column) => <th className={`${column.minWidth || "min-w-32"} px-5 py-3 text-left text-theme-xs font-semibold`} key={column.key}>{column.label}</th>)}
                <th className="min-w-28 px-5 py-3 text-center text-theme-xs font-semibold">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
              {loading ? (
                Array.from({ length: 5 }).map((_, rowIndex) => (
                  <tr key={rowIndex}>
                    {config.columns.map((column) => <td className="px-5 py-4" key={column.key}><div className="h-4 w-full animate-pulse rounded-md bg-gray-100 dark:bg-white/[0.05]" /></td>)}
                    <td className="px-5 py-4"><div className="mx-auto h-4 w-16 animate-pulse rounded-md bg-gray-100 dark:bg-white/[0.05]" /></td>
                  </tr>
                ))
              ) : records.length ? (
                visibleRecords.map((record) => (
                  <tr className="hover:bg-gray-50 dark:hover:bg-white/[0.02]" key={String(record[primaryKey])}>
                    {config.columns.map((column) => <td className="px-5 py-4 text-theme-sm text-gray-700 dark:text-gray-300" key={column.key}>{column.render ? column.render(record) : defaultRender(record, column.key)}</td>)}
                    <td className="px-5 py-4"><div className="flex justify-center gap-3"><button aria-label="Edit" className="text-warning-500 transition-colors hover:text-warning-600 dark:text-warning-400 dark:hover:text-warning-500" onClick={() => openEdit(record)} type="button"><Icon className="h-5 w-5" name="edit" /></button>{config.allowDelete === false ? null : <button aria-label="Delete" className="text-error-500 transition-colors hover:text-error-600 dark:text-error-400 dark:hover:text-error-500" onClick={() => setRecordToDelete(record)} type="button"><Icon className="h-5 w-5" name="trash" /></button>}</div></td>
                  </tr>
                ))
              ) : (
                <tr><td className="px-5 py-8 text-center text-sm text-gray-500 dark:text-gray-400" colSpan={config.columns.length + 1}>No data found.</td></tr>
              )}
            </tbody>
          </table>
      </TablePanel>

      <ConfirmModal
        confirmText="Delete"
        isDestructive
        isLoading={deleting}
        isOpen={Boolean(recordToDelete)}
        message={`Data "${recordLabel(recordToDelete, config)}" akan dihapus permanen dari ${config.title}.`}
        onClose={closeDeleteModal}
        onConfirm={() => void confirmDeleteRecord()}
        title={`Delete ${config.title}`}
      />
    </div>
  );
}
