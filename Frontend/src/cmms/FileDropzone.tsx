"use client";

import { useRef, useState, type DragEvent, type KeyboardEvent } from "react";
import { Icon } from "./icons";

type FileDropzoneProps = {
  accept?: string;
  disabled?: boolean;
  file: File | null;
  helperText?: string;
  label: string;
  onChange: (file: File | null) => void;
  compact?: boolean;
};

function formatFileSize(size: number) {
  if (size < 1024) {
    return `${size} B`;
  }

  const kb = size / 1024;
  if (kb < 1024) {
    return `${kb.toFixed(1)} KB`;
  }

  return `${(kb / 1024).toFixed(1)} MB`;
}

export function FileDropzone({
  accept = ".pdf,.jpg,.jpeg,.png",
  compact,
  disabled,
  file,
  helperText = "PDF, JPG, PNG files up to 10 MB",
  label,
  onChange,
}: FileDropzoneProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [isDragging, setIsDragging] = useState(false);

  const openPicker = () => {
    if (!disabled) {
      inputRef.current?.click();
    }
  };

  const setFirstFile = (files: FileList | null) => {
    if (!disabled) {
      onChange(files?.[0] || null);
    }
  };

  const onDrop = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    event.stopPropagation();
    setIsDragging(false);
    setFirstFile(event.dataTransfer.files);
  };

  const onKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      openPicker();
    }
  };

  return (
    <div className="block">
      <span className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-300">{label}</span>
      <div
        aria-disabled={disabled}
        className={`relative flex cursor-pointer flex-col items-center justify-center rounded-lg border border-dashed px-4 text-center transition-colors focus:outline-none focus:ring-2 focus:ring-brand-500/60 disabled:cursor-not-allowed ${
          compact ? "min-h-[180px] py-6" : "min-h-[230px] py-8"
        } ${
          isDragging
            ? "border-brand-400 bg-brand-50 text-gray-900 dark:bg-brand-500/10 dark:text-white"
            : "border-gray-300 bg-white text-gray-800 hover:border-brand-400 hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-950 dark:text-white dark:hover:border-brand-500 dark:hover:bg-gray-900"
        } ${disabled ? "opacity-60" : ""}`}
        onClick={openPicker}
        onDragEnter={(event) => {
          event.preventDefault();
          if (!disabled) {
            setIsDragging(true);
          }
        }}
        onDragLeave={(event) => {
          event.preventDefault();
          setIsDragging(false);
        }}
        onDragOver={(event) => {
          event.preventDefault();
        }}
        onDrop={onDrop}
        onKeyDown={onKeyDown}
        role="button"
        tabIndex={disabled ? -1 : 0}
      >
        <input
          accept={accept}
          className="hidden"
          disabled={disabled}
          onChange={(event) => setFirstFile(event.target.files)}
          ref={inputRef}
          type="file"
        />
        <div className={`flex items-center justify-center rounded-full bg-gray-100 text-gray-500 dark:bg-white/10 dark:text-gray-300 ${compact ? "h-12 w-12" : "h-16 w-16"}`}>
          <Icon name="upload" className={compact ? "h-5 w-5" : "h-7 w-7"} />
        </div>
        <p className="mt-4 text-base font-semibold text-gray-900 dark:text-white sm:text-lg">Drag & Drop Files Here</p>
        <p className="mt-2 max-w-xs text-sm leading-5 text-gray-600 dark:text-gray-300">Drag and drop {helperText} here or browse</p>
        <button
          className="mt-4 text-sm font-semibold text-brand-600 underline underline-offset-2 transition-colors hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
          onClick={(event) => {
            event.stopPropagation();
            openPicker();
          }}
          type="button"
        >
          Browse File
        </button>

        {file ? (
          <div className="mt-5 flex w-full max-w-sm items-center justify-between gap-3 rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-left dark:border-white/10 dark:bg-white/5">
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-gray-900 dark:text-white">{file.name}</p>
              <p className="text-xs text-gray-500 dark:text-gray-400">{formatFileSize(file.size)}</p>
            </div>
            <button
              aria-label="Clear file"
              className="inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-lg border border-gray-200 text-gray-500 transition-colors hover:bg-gray-100 hover:text-gray-900 dark:border-white/10 dark:text-gray-300 dark:hover:bg-white/10 dark:hover:text-white"
              onClick={(event) => {
                event.stopPropagation();
                onChange(null);
                if (inputRef.current) {
                  inputRef.current.value = "";
                }
              }}
              type="button"
            >
              <Icon name="x" className="h-4 w-4" />
            </button>
          </div>
        ) : null}
      </div>
    </div>
  );
}
