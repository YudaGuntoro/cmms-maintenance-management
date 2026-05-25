"use client";

import { useEffect, useMemo, useState } from "react";
import { Modal } from "@/components/ui/modal";
import { apiBlob, apiGet } from "./api";
import { formatDateTime, formatNumber } from "./format";
import { Icon } from "./icons";
import { Badge, Feedback } from "./ui";
import type { WorkOrder, WorkOrderPhoto } from "./types";

type PhotoPreview = WorkOrderPhoto & {
  objectUrl?: string;
  error?: string;
};

function DetailItem({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <div className="rounded-lg border border-gray-100 bg-gray-50 px-4 py-3 dark:border-white/[0.05] dark:bg-white/[0.03]">
      <p className="text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">{label}</p>
      <p className="mt-2 text-sm font-medium text-gray-900 dark:text-white/90">{value || "-"}</p>
    </div>
  );
}

function formatSize(bytes?: number) {
  if (!bytes) {
    return "0 KB";
  }

  if (bytes < 1024 * 1024) {
    return `${formatNumber(bytes / 1024)} KB`;
  }

  return `${formatNumber(bytes / 1024 / 1024)} MB`;
}

export function WorkOrderDetailModal({
  assetLabel,
  isOpen,
  onClose,
  technicianLabel,
  workOrder,
}: {
  assetLabel?: string;
  isOpen: boolean;
  onClose: () => void;
  technicianLabel?: string;
  workOrder: WorkOrder | null;
}) {
  const [photos, setPhotos] = useState<PhotoPreview[]>([]);
  const [loadingPhotos, setLoadingPhotos] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const title = useMemo(() => workOrder ? `${workOrder.wo_number} - ${workOrder.title}` : "Work Order Detail", [workOrder]);

  useEffect(() => {
    if (!isOpen || !workOrder) {
      setPhotos([]);
      setError(null);
      return;
    }

    let active = true;
    const objectUrls: string[] = [];
    const currentWorkOrder = workOrder;

    async function loadPhotos() {
      setLoadingPhotos(true);
      setError(null);

      try {
        const metadata = await apiGet<WorkOrderPhoto[]>(`/api/work-orders/${currentWorkOrder.id}/photos`);
        const previews = await Promise.all((metadata || []).map(async (photo) => {
          try {
            const blob = await apiBlob(`/api/work-orders/${currentWorkOrder.id}/photos/${photo.id}/content`);
            const objectUrl = URL.createObjectURL(blob);
            objectUrls.push(objectUrl);
            return { ...photo, objectUrl };
          } catch {
            return { ...photo, error: "Image failed to load." };
          }
        }));

        if (active) {
          setPhotos(previews);
        }
      } catch (err) {
        if (active) {
          setError(err instanceof Error ? err.message : "Failed to load work order photos.");
        }
      } finally {
        if (active) {
          setLoadingPhotos(false);
        }
      }
    }

    void loadPhotos();

    return () => {
      active = false;
      objectUrls.forEach((url) => URL.revokeObjectURL(url));
    };
  }, [isOpen, workOrder]);

  return (
    <Modal isOpen={isOpen} onClose={onClose} className="mx-4 max-w-[980px] overflow-hidden p-0" showCloseButton={false}>
      <div className="border-b border-gray-100 px-6 py-5 dark:border-white/[0.05]">
        <div className="flex items-start justify-between gap-4">
          <div className="flex min-w-0 items-start gap-4">
            <div className="flex size-12 shrink-0 items-center justify-center rounded-lg bg-brand-500/10 text-brand-500 ring-1 ring-brand-500/20">
              <Icon className="size-6" name="wrench" />
            </div>
            <div className="min-w-0">
              <h3 className="truncate text-lg font-semibold text-gray-900 dark:text-white/90">Work Order Detail</h3>
              <p className="mt-1 truncate text-sm text-gray-500 dark:text-gray-400">{title}</p>
            </div>
          </div>
          <button aria-label="Close" className="icon-button" onClick={onClose} type="button">
            <Icon name="x" />
          </button>
        </div>
      </div>

      {workOrder ? (
        <div className="max-h-[78vh] space-y-6 overflow-y-auto px-6 py-5">
          <Feedback error={error} />

          <div className="flex flex-wrap items-center gap-2">
            <Badge value={workOrder.status} />
            <Badge value={workOrder.priority} />
            <Badge value={workOrder.maintenance_type} />
          </div>

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
            <DetailItem label="Asset" value={assetLabel || `Asset #${workOrder.asset_id}`} />
            <DetailItem label="Technician" value={technicianLabel || "-"} />
            <DetailItem label="Reported" value={formatDateTime(workOrder.reported_at || workOrder.created_at)} />
            <DetailItem label="Scheduled" value={formatDateTime(workOrder.scheduled_at)} />
            <DetailItem label="Started" value={formatDateTime(workOrder.started_at)} />
            <DetailItem label="Completed" value={formatDateTime(workOrder.completed_at)} />
            <DetailItem label="Failure Code" value={workOrder.failure_code} />
            <DetailItem label="Root Cause" value={workOrder.root_cause} />
            <DetailItem label="Repair Duration" value={`${formatNumber(workOrder.repair_minutes)} min`} />
          </div>

          <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
            <div className="rounded-lg border border-gray-100 bg-gray-50 p-4 dark:border-white/[0.05] dark:bg-white/[0.03] lg:col-span-3">
              <p className="text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">Description</p>
              <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-gray-700 dark:text-gray-300">{workOrder.description || "-"}</p>
            </div>
            <div className="rounded-lg border border-gray-100 bg-gray-50 p-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
              <p className="text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">Action Taken</p>
              <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-gray-700 dark:text-gray-300">{workOrder.action_taken || "-"}</p>
            </div>
            <div className="rounded-lg border border-gray-100 bg-gray-50 p-4 dark:border-white/[0.05] dark:bg-white/[0.03] lg:col-span-2">
              <p className="text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">Result</p>
              <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-gray-700 dark:text-gray-300">{workOrder.result || "-"}</p>
            </div>
          </div>

          <div>
            <div className="mb-3 flex items-center justify-between gap-3">
              <div>
                <h4 className="text-base font-semibold text-gray-900 dark:text-white">Photo History</h4>
                <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Loaded only when this detail is opened.</p>
              </div>
              <span className="rounded-full border border-gray-200 px-3 py-1 text-xs font-semibold text-gray-600 dark:border-gray-700 dark:text-gray-300">{photos.length} photos</span>
            </div>

            {loadingPhotos ? (
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {Array.from({ length: 3 }).map((_, index) => (
                  <div className="aspect-[4/3] animate-pulse rounded-lg bg-gray-100 dark:bg-white/[0.05]" key={index} />
                ))}
              </div>
            ) : photos.length ? (
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {photos.map((photo) => (
                  <div className="overflow-hidden rounded-lg border border-gray-100 bg-white shadow-theme-sm dark:border-white/[0.05] dark:bg-white/[0.03]" key={photo.id}>
                    <div className="aspect-[4/3] bg-gray-100 dark:bg-gray-900">
                      {photo.objectUrl ? (
                        <a href={photo.objectUrl} rel="noreferrer" target="_blank">
                          <img alt={photo.file_name || "Work order field photo"} className="h-full w-full object-cover" src={photo.objectUrl} />
                        </a>
                      ) : (
                        <div className="flex h-full flex-col items-center justify-center px-4 text-center text-sm text-gray-500 dark:text-gray-400">
                          <Icon className="mb-2 size-8" name="image" />
                          {photo.error || "Preview unavailable"}
                        </div>
                      )}
                    </div>
                    <div className="space-y-1 px-3 py-3">
                      <p className="truncate text-sm font-semibold text-gray-900 dark:text-white">{photo.file_name}</p>
                      <p className="text-xs text-gray-500 dark:text-gray-400">{formatDateTime(photo.uploaded_at)} - {formatSize(photo.size_bytes)}</p>
                      {photo.uploaded_by ? <p className="text-xs text-gray-500 dark:text-gray-400">By {photo.uploaded_by}</p> : null}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="flex min-h-40 flex-col items-center justify-center rounded-lg border border-gray-100 bg-gray-50 px-5 text-center dark:border-white/[0.05] dark:bg-white/[0.03]">
                <Icon className="size-9 text-gray-400" name="image" />
                <p className="mt-3 text-sm font-semibold text-gray-900 dark:text-white">No photos yet</p>
                <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Foto akan muncul setelah teknisi upload dari update MyTask.</p>
              </div>
            )}
          </div>
        </div>
      ) : null}
    </Modal>
  );
}
