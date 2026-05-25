"use client";

import { useMemo, useState, type ChangeEvent, type FormEvent, type ReactNode } from "react";
import { getApiBaseUrl } from "./api";
import { FileDropzone } from "./FileDropzone";
import { formatDateTime, nowDateTimeLocal, toDateTimeLocalInput } from "./format";
import { Icon } from "./icons";
import { TextInput } from "./ui";
import type { ApiResponse, ContractorWorkPlan } from "./types";

type VendorForm = {
  vendor_name: string;
  vendor_pic_name: string;
  vendor_pic_phone: string;
  worker_count: string;
  internal_pic_name: string;
  department_area: string;
  work_title: string;
  work_description: string;
  work_area: string;
  work_location: string;
  asset_note: string;
  additional_notes: string;
  start_at: string;
  end_at: string;
  working_at_height: boolean;
  hot_work: boolean;
  welding: boolean;
  electrical_work: boolean;
  confined_space: boolean;
  heavy_equipment_activity: boolean;
  chemical_handling: boolean;
  shutdown_activity: boolean;
  loto_required: boolean;
  need_safety_standby: boolean;
};

type RiskKey =
  | "working_at_height"
  | "hot_work"
  | "welding"
  | "electrical_work"
  | "confined_space"
  | "heavy_equipment_activity"
  | "chemical_handling"
  | "shutdown_activity"
  | "loto_required"
  | "need_safety_standby";

const riskDefinitions: Array<{ key: RiskKey; label: string; description: string }> = [
  { key: "working_at_height", label: "Working at Height", description: "Pekerjaan di ketinggian" },
  { key: "hot_work", label: "Hot Work", description: "Pemotongan, api, grinding" },
  { key: "welding", label: "Welding", description: "Pekerjaan las" },
  { key: "electrical_work", label: "Electrical Work", description: "Tarik dan koneksi kabel" },
  { key: "confined_space", label: "Confined Space", description: "Ruang terbatas" },
  { key: "heavy_equipment_activity", label: "Heavy Equipment", description: "Forklift, crane, lifting" },
  { key: "chemical_handling", label: "Chemical Handling", description: "Bahan kimia atau limbah" },
  { key: "shutdown_activity", label: "Shutdown Activity", description: "Stop mesin atau line" },
  { key: "loto_required", label: "LOTO Required", description: "Isolasi energi dibutuhkan" },
  { key: "need_safety_standby", label: "Need Safety Standby", description: "Butuh standby safety" },
];

const allowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];

function defaultForm(): VendorForm {
  const start = nowDateTimeLocal();
  const endDate = new Date();
  endDate.setHours(endDate.getHours() + 2);

  return {
    vendor_name: "",
    vendor_pic_name: "",
    vendor_pic_phone: "",
    worker_count: "1",
    internal_pic_name: "",
    department_area: "",
    work_title: "",
    work_description: "",
    work_area: "",
    work_location: "",
    asset_note: "",
    additional_notes: "",
    start_at: start,
    end_at: toDateTimeLocalInput(endDate.toISOString()),
    working_at_height: false,
    hot_work: false,
    welding: false,
    electrical_work: false,
    confined_space: false,
    heavy_equipment_activity: false,
    chemical_handling: false,
    shutdown_activity: false,
    loto_required: false,
    need_safety_standby: false,
  };
}

async function publicApiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  const isFormData = typeof FormData !== "undefined" && init.body instanceof FormData;

  if (!headers.has("Content-Type") && init.body && !isFormData) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    ...init,
    headers,
  });
  const text = await response.text();
  const payload = text ? (JSON.parse(text) as ApiResponse<T>) : null;

  if (!response.ok || payload?.success === false) {
    throw new Error(payload?.message || `Request failed with status ${response.status}`);
  }

  return payload ? payload.data : (undefined as T);
}

function extensionOf(fileName: string) {
  const dotIndex = fileName.lastIndexOf(".");
  return dotIndex >= 0 ? fileName.slice(dotIndex).toLowerCase() : "";
}

function validateFile(file: File | null, label: string) {
  if (!file) {
    return;
  }

  if (file.size > 10_000_000) {
    throw new Error(`${label} maksimal 10 MB.`);
  }

  if (!allowedExtensions.includes(extensionOf(file.name))) {
    throw new Error(`${label} harus format PDF, JPG, atau PNG.`);
  }
}

function buildPayload(form: VendorForm) {
  const additionalNotes = [
    form.asset_note.trim() ? `Asset / mesin terkait: ${form.asset_note.trim()}` : "",
    form.additional_notes.trim(),
  ]
    .filter(Boolean)
    .join("\n\n");

  return {
    vendor_name: form.vendor_name.trim(),
    vendor_pic_name: form.vendor_pic_name.trim(),
    vendor_pic_phone: form.vendor_pic_phone.trim() || null,
    worker_count: Number(form.worker_count || 0),
    internal_pic_name: form.internal_pic_name.trim(),
    department_area: form.department_area.trim(),
    work_title: form.work_title.trim(),
    work_description: form.work_description.trim() || null,
    work_area: form.work_area.trim(),
    work_location: form.work_location.trim() || null,
    asset_id: null,
    additional_notes: additionalNotes || null,
    start_at: form.start_at,
    end_at: form.end_at,
    status: "WAITING_PERMIT_DOCUMENT",
    permit_document_status: "NOT_UPLOADED",
    working_at_height: form.working_at_height,
    hot_work: form.hot_work,
    welding: form.welding,
    electrical_work: form.electrical_work,
    confined_space: form.confined_space,
    heavy_equipment_activity: form.heavy_equipment_activity,
    chemical_handling: form.chemical_handling,
    shutdown_activity: form.shutdown_activity,
    loto_required: form.loto_required,
    need_safety_standby: form.need_safety_standby,
  };
}

async function uploadDocument(planId: number, file: File, documentType: "PERMIT" | "SAFETY_DOCUMENT", notes: string) {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("document_type", documentType);
  formData.append("document_status", "UPLOADED");
  formData.append("notes", notes);

  await publicApiRequest<unknown>(`/api/contractor-monitoring/public/${planId}/documents`, {
    method: "POST",
    body: formData,
  });
}

export default function VendorMonitoringPublicPage() {
  const [form, setForm] = useState<VendorForm>(() => defaultForm());
  const [permitFile, setPermitFile] = useState<File | null>(null);
  const [supportingFile, setSupportingFile] = useState<File | null>(null);
  const [submittedPlan, setSubmittedPlan] = useState<ContractorWorkPlan | null>(null);
  const [uploadWarnings, setUploadWarnings] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const selectedRiskCount = useMemo(() => riskDefinitions.filter((risk) => form[risk.key]).length, [form]);

  const onChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const target = event.target;
    const name = target.name as keyof VendorForm;
    const value = target instanceof HTMLInputElement && target.type === "checkbox" ? target.checked : target.value;
    setForm((current) => ({ ...current, [name]: value }));
  };

  const submitForm = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    setUploadWarnings([]);

    try {
      if (new Date(form.end_at) <= new Date(form.start_at)) {
        throw new Error("Tanggal selesai harus lebih besar dari tanggal mulai.");
      }

      validateFile(permitFile, "Dokumen permit");
      validateFile(supportingFile, "Dokumen pendukung");

      const plan = await publicApiRequest<ContractorWorkPlan>("/api/contractor-monitoring/public", {
        method: "POST",
        body: JSON.stringify(buildPayload(form)),
      });

      const warnings: string[] = [];
      if (permitFile) {
        try {
          await uploadDocument(plan.id, permitFile, "PERMIT", "Uploaded from vendor public form");
        } catch (err) {
          warnings.push(err instanceof Error ? `Permit gagal diupload: ${err.message}` : "Permit gagal diupload.");
        }
      }

      if (supportingFile) {
        try {
          await uploadDocument(plan.id, supportingFile, "SAFETY_DOCUMENT", "Supporting document from vendor public form");
        } catch (err) {
          warnings.push(err instanceof Error ? `Dokumen pendukung gagal diupload: ${err.message}` : "Dokumen pendukung gagal diupload.");
        }
      }

      setSubmittedPlan(plan);
      setUploadWarnings(warnings);
      setForm(defaultForm());
      setPermitFile(null);
      setSupportingFile(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Data gagal dikirim.");
    } finally {
      setSubmitting(false);
    }
  };

  const startNew = () => {
    setSubmittedPlan(null);
    setUploadWarnings([]);
    setError(null);
  };

  return (
    <main className="min-h-screen bg-gray-100 text-gray-900 dark:bg-gray-950 dark:text-white">
      <div className="border-b border-gray-200 bg-white dark:border-white/[0.08] dark:bg-gray-900">
        <div className="mx-auto flex max-w-6xl flex-col gap-3 px-4 py-5 sm:px-6 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-brand-600 dark:text-brand-400">CMMS Vendor Form</p>
            <h1 className="mt-1 text-2xl font-semibold text-gray-950 dark:text-white">Vendor Monitoring</h1>
          </div>
          <div className="grid grid-cols-3 gap-2 text-xs font-semibold text-gray-600 dark:text-gray-300">
            <span className="rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-center dark:border-gray-800 dark:bg-white/[0.03]">Plan</span>
            <span className="rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-center dark:border-gray-800 dark:bg-white/[0.03]">Permit</span>
            <span className="rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-center dark:border-gray-800 dark:bg-white/[0.03]">MTC Check</span>
          </div>
        </div>
      </div>

      <div className="mx-auto grid max-w-6xl gap-5 px-4 py-6 sm:px-6 lg:grid-cols-[minmax(0,1fr)_320px]">
        <section className="rounded-lg border border-gray-200 bg-white shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
          {submittedPlan ? (
            <div className="p-6 sm:p-8">
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-success-50 text-success-600 dark:bg-success-500/10 dark:text-success-400">
                <Icon name="check" className="h-6 w-6" />
              </div>
              <h2 className="mt-5 text-xl font-semibold text-gray-950 dark:text-white">Data pekerjaan vendor sudah terkirim.</h2>
              <p className="mt-2 text-sm leading-6 text-gray-600 dark:text-gray-300">Tim Maintenance akan melihat rencana ini di dashboard Contractor Monitoring.</p>
              <div className="mt-6 grid gap-3 rounded-lg border border-gray-200 bg-gray-50 p-4 text-sm dark:border-gray-800 dark:bg-white/[0.03]">
                <SummaryRow label="Vendor" value={submittedPlan.vendor_name} />
                <SummaryRow label="Pekerjaan" value={submittedPlan.work_title} />
                <SummaryRow label="Area" value={submittedPlan.work_area} />
                <SummaryRow label="Mulai" value={formatDateTime(submittedPlan.start_at)} />
              </div>
              {uploadWarnings.length ? (
                <div className="mt-5 rounded-lg border border-warning-500/25 bg-warning-50 px-4 py-3 text-sm text-warning-700 dark:border-warning-500/30 dark:bg-warning-500/10 dark:text-warning-400">
                  {uploadWarnings.map((warning) => (
                    <p key={warning}>{warning}</p>
                  ))}
                </div>
              ) : null}
              <button className="mt-7 inline-flex h-11 items-center justify-center gap-2 rounded-lg bg-brand-500 px-5 text-sm font-semibold text-white transition-colors hover:bg-brand-600" onClick={startNew} type="button">
                <Icon name="plus" className="h-4 w-4" />
                Isi Data Baru
              </button>
            </div>
          ) : (
            <form className="p-5 sm:p-7" onSubmit={submitForm}>
              <div className="grid gap-5 lg:grid-cols-2">
                <FormSection className="lg:col-span-2" title="Informasi Vendor">
                  <div className="grid gap-4 md:grid-cols-2">
                    <Field label="Nama Vendor" name="vendor_name" onChange={onChange} required value={form.vendor_name} />
                    <Field label="PIC Vendor" name="vendor_pic_name" onChange={onChange} required value={form.vendor_pic_name} />
                    <Field label="Nomor HP PIC Vendor" name="vendor_pic_phone" onChange={onChange} required value={form.vendor_pic_phone} />
                    <Field label="Jumlah Pekerja" min="0" name="worker_count" onChange={onChange} required type="number" value={form.worker_count} />
                    <Field label="PIC MTC / Internal" name="internal_pic_name" onChange={onChange} required value={form.internal_pic_name} />
                    <Field label="Departemen / Area Terkait" name="department_area" onChange={onChange} required value={form.department_area} />
                  </div>
                </FormSection>

                <FormSection className="lg:col-span-2" title="Rencana Pekerjaan">
                  <div className="grid gap-4 md:grid-cols-2">
                    <Field label="Judul Pekerjaan" name="work_title" onChange={onChange} required value={form.work_title} />
                    <Field label="Area Kerja" name="work_area" onChange={onChange} required value={form.work_area} />
                    <Field label="Detail Lokasi" name="work_location" onChange={onChange} value={form.work_location} />
                    <Field label="Asset / Mesin Terkait" name="asset_note" onChange={onChange} value={form.asset_note} />
                    <Textarea className="md:col-span-2" label="Deskripsi Pekerjaan" name="work_description" onChange={onChange} required value={form.work_description} />
                    <Textarea className="md:col-span-2" label="Catatan Tambahan" name="additional_notes" onChange={onChange} value={form.additional_notes} />
                  </div>
                </FormSection>

                <FormSection title="Jadwal">
                  <div className="grid gap-4">
                    <TextInput label="Tanggal & Jam Mulai" name="start_at" onChange={onChange} required type="datetime-local" value={form.start_at} />
                    <TextInput label="Tanggal & Jam Selesai" name="end_at" onChange={onChange} required type="datetime-local" value={form.end_at} />
                  </div>
                </FormSection>

                <FormSection title="Upload Dokumen">
                  <div className="grid gap-4">
                    <FileDropzone compact file={permitFile} label="Permit Kerja" onChange={setPermitFile} />
                    <FileDropzone compact file={supportingFile} label="Dokumen Pendukung" onChange={setSupportingFile} />
                  </div>
                </FormSection>
              </div>

              <div className="mt-6 rounded-lg border border-gray-200 bg-gray-50 p-4 dark:border-gray-800 dark:bg-white/[0.03]">
                <div className="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
                  <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Checklist Risiko / Aktivitas</h2>
                  <span className="text-xs font-semibold text-gray-500 dark:text-gray-400">{selectedRiskCount} selected</span>
                </div>
                <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                  {riskDefinitions.map((risk) => (
                    <label className="flex cursor-pointer gap-3 rounded-lg border border-gray-200 bg-white p-3 transition-colors hover:border-brand-300 dark:border-gray-800 dark:bg-gray-900 dark:hover:border-brand-500/60" key={risk.key}>
                      <input checked={form[risk.key]} className="mt-1 h-4 w-4 rounded border-gray-300 text-brand-500 focus:ring-brand-500" name={risk.key} onChange={onChange} type="checkbox" />
                      <span>
                        <span className="block text-sm font-semibold text-gray-800 dark:text-gray-100">{risk.label}</span>
                        <span className="mt-0.5 block text-xs text-gray-500 dark:text-gray-400">{risk.description}</span>
                      </span>
                    </label>
                  ))}
                </div>
              </div>

              {error ? (
                <div className="mt-5 rounded-lg border border-error-500/20 bg-error-50 px-4 py-3 text-sm font-medium text-error-700 dark:border-error-500/30 dark:bg-error-500/10 dark:text-error-400">
                  {error}
                </div>
              ) : null}

              <div className="mt-7 flex flex-col-reverse gap-3 sm:flex-row sm:items-center sm:justify-between">
                <p className="text-xs leading-5 text-gray-500 dark:text-gray-400">Data ini akan masuk ke Contractor Monitoring untuk dicek oleh tim Maintenance.</p>
                <button className="inline-flex h-11 items-center justify-center gap-2 rounded-lg bg-brand-500 px-5 text-sm font-semibold text-white transition-colors hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-60" disabled={submitting} type="submit">
                  <Icon name="check" className="h-4 w-4" />
                  {submitting ? "Mengirim..." : "Submit Rencana"}
                </button>
              </div>
            </form>
          )}
        </section>

        <aside className="space-y-5">
          <section className="rounded-lg border border-gray-200 bg-white p-5 shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-brand-50 text-brand-600 dark:bg-brand-500/10 dark:text-brand-400">
              <Icon name="calendar" className="h-5 w-5" />
            </div>
            <h2 className="mt-4 text-base font-semibold text-gray-950 dark:text-white">Sebelum Datang</h2>
            <div className="mt-4 space-y-3 text-sm leading-6 text-gray-600 dark:text-gray-300">
              <p>Isi jadwal sesuai rencana aktual pekerjaan.</p>
              <p>Upload permit atau dokumen safety bila sudah tersedia.</p>
              <p>Centang semua aktivitas berisiko agar area bisa dipersiapkan.</p>
            </div>
          </section>
          <section className="rounded-lg border border-warning-500/25 bg-warning-50 p-5 text-warning-800 shadow-theme-sm dark:border-warning-500/30 dark:bg-warning-500/10 dark:text-warning-300">
            <div className="flex items-center gap-2 text-sm font-semibold">
              <Icon name="alert" className="h-4 w-4" />
              Catatan Permit
            </div>
            <p className="mt-3 text-sm leading-6">Form ini untuk informasi awal dan monitoring. Keputusan boleh mulai kerja tetap mengikuti arahan PIC MTC dan aturan safety di area.</p>
          </section>
        </aside>
      </div>
    </main>
  );
}

function FormSection({ children, className = "", title }: { children: ReactNode; className?: string; title: string }) {
  return (
    <section className={`rounded-lg border border-gray-200 bg-gray-50 p-4 dark:border-gray-800 dark:bg-white/[0.03] ${className}`}>
      <h2 className="mb-4 text-sm font-semibold text-gray-900 dark:text-white">{title}</h2>
      {children}
    </section>
  );
}

function Field({
  label,
  name,
  onChange,
  required,
  type = "text",
  value,
  ...props
}: {
  label: string;
  name: string;
  onChange: (event: ChangeEvent<HTMLInputElement>) => void;
  required?: boolean;
  type?: string;
  value: string;
} & Omit<React.InputHTMLAttributes<HTMLInputElement>, "name" | "onChange" | "required" | "type" | "value">) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">{label}</span>
      <input className="control-input" name={name} onChange={onChange} required={required} type={type} value={value} {...props} />
    </label>
  );
}

function Textarea({
  className = "",
  label,
  name,
  onChange,
  required,
  value,
}: {
  className?: string;
  label: string;
  name: string;
  onChange: (event: ChangeEvent<HTMLTextAreaElement>) => void;
  required?: boolean;
  value: string;
}) {
  return (
    <label className={`block ${className}`}>
      <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">{label}</span>
      <textarea className="control-input min-h-24 resize-y" name={name} onChange={onChange} required={required} value={value} />
    </label>
  );
}

function SummaryRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="grid gap-1 sm:grid-cols-[120px_1fr]">
      <span className="font-semibold text-gray-500 dark:text-gray-400">{label}</span>
      <span className="text-gray-900 dark:text-white">{value}</span>
    </div>
  );
}
