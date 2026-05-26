"use client";

import Input from "@/components/form/input/InputField";
import Label from "@/components/form/Label";
import { EyeCloseIcon, EyeIcon, LockIcon, UserIcon } from "@/icons";
import Image from "next/image";
import Link from "next/link";
import { useEffect, useId, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { apiPost } from "./api";
import { hasValidAuthSession, saveAuthSession } from "./auth";
import { Icon } from "./icons";
import type { LoginResponse } from "./types";

function safeNextPath(value: string | null) {
  return value?.startsWith("/") && !value.startsWith("//") && !value.startsWith("/signin") ? value : "/";
}

export default function SignInPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const nextPath = safeNextPath(searchParams.get("next"));
  const usernameFieldId = useId().replace(/:/g, "");
  const passwordFieldId = useId().replace(/:/g, "");
  const [showPassword, setShowPassword] = useState(false);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [autofillLocked, setAutofillLocked] = useState(true);
  const [fieldNonce, setFieldNonce] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (hasValidAuthSession()) {
      router.replace(nextPath);
    }
  }, [nextPath, router]);

  useEffect(() => {
    const clearAutofill = () => {
      setUsername("");
      setPassword("");
      document.querySelectorAll<HTMLInputElement>("[data-cmms-login-field]").forEach((field) => {
        field.value = "";
      });
    };

    setFieldNonce(crypto.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(36).slice(2)}`);
    clearAutofill();
    const timers = [150, 600, 1200, 2400].map((delay) => window.setTimeout(clearAutofill, delay));
    return () => timers.forEach(window.clearTimeout);
  }, []);

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!username.trim() || !password) {
      setError("Username dan password wajib diisi.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const response = await apiPost<LoginResponse>("/api/auth/login", { username: username.trim(), password });
      saveAuthSession(response);
      router.push(nextPath);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="relative flex min-h-screen w-full items-center justify-center px-4 py-10 sm:px-6 lg:w-1/2 lg:px-10">
      <div className="relative w-full max-w-[500px]">
        <div className="mb-6 flex items-center gap-3 lg:hidden">
          <Image
            alt="Computerized Maintenance Management System Logo"
            className="size-[64px] object-contain"
            height={64}
            priority
            src="/images/logo/logo.png"
            width={64}
          />
          <div>
            <p className="text-xs font-semibold text-gray-800 dark:text-white/90">Computerized Maintenance Management System (CMMS)</p>
            <p className="text-sm font-semibold text-gray-500 dark:text-gray-400">Nusakarya Digital Solution</p>
          </div>
        </div>

        <div className="rounded-lg border border-gray-200 bg-white/95 p-6 shadow-theme-xl backdrop-blur dark:border-white/[0.08] dark:bg-gray-900/90 sm:p-8">
          <div className="mb-8">
            <Link className="mb-6 inline-flex items-center gap-2 text-sm font-semibold text-brand-500" href="/signin">
              <span className="flex size-9 items-center justify-center rounded-lg bg-brand-500 text-white shadow-theme-xs">
                <Icon className="size-5" name="wrench" />
              </span>
              MaintenanceApp
            </Link>
            <h1 className="mb-3 text-title-sm font-bold text-gray-900 dark:text-white/90 sm:text-title-md">Sign in</h1>
            <p className="text-sm leading-6 text-gray-500 dark:text-gray-400">Masuk untuk mengakses dashboard Computerized Maintenance Management System (CMMS).</p>
          </div>

          {error ? (
            <div className="mb-6 rounded-lg border border-error-500/20 bg-error-50 px-4 py-3 text-sm text-error-600 dark:border-error-500/30 dark:bg-error-500/10 dark:text-error-400">
              {error}
            </div>
          ) : null}

          <form autoComplete="off" data-form-type="other" onSubmit={(event) => void submit(event)}>
            <div className="space-y-6">
              <div>
                <Label htmlFor={`cmms-user-${usernameFieldId}`}>
                  Username <span className="text-error-500">*</span>
                </Label>
                <div className="relative">
                  <span className="pointer-events-none absolute left-4 top-1/2 z-10 -translate-y-1/2 text-gray-400 dark:text-gray-500">
                    <UserIcon className="size-5 fill-current" />
                  </span>
                  <Input
                    autoCapitalize="none"
                    autoComplete="new-password"
                    className="h-12 pl-12"
                    data-1p-ignore="true"
                    data-cmms-login-field="true"
                    data-form-type="other"
                    data-lpignore="true"
                    disabled={loading}
                    id={`cmms-user-${usernameFieldId}`}
                    inputMode="text"
                    name={fieldNonce ? `cmms-operator-${fieldNonce}` : `cmms-operator-${usernameFieldId}`}
                    onChange={(event) => setUsername(event.target.value)}
                    onFocus={() => {
                      setAutofillLocked(false);
                      setUsername((current) => (current === "demo" ? "" : current));
                    }}
                    placeholder="Masukkan username"
                    readOnly={autofillLocked}
                    spellCheck={false}
                    type="search"
                    value={username}
                  />
                </div>
              </div>

              <div>
                <Label htmlFor={`cmms-secret-${passwordFieldId}`}>
                  Password <span className="text-error-500">*</span>
                </Label>
                <div className="relative">
                  <span className="pointer-events-none absolute left-4 top-1/2 z-10 -translate-y-1/2 text-gray-400 dark:text-gray-500">
                    <LockIcon className="size-5 fill-current" />
                  </span>
                  <Input
                    autoComplete="new-password"
                    className="h-12 pl-12 pr-12"
                    data-1p-ignore="true"
                    data-cmms-login-field="true"
                    data-form-type="other"
                    data-lpignore="true"
                    disabled={loading}
                    id={`cmms-secret-${passwordFieldId}`}
                    name={fieldNonce ? `cmms-key-${fieldNonce}` : `cmms-key-${passwordFieldId}`}
                    onChange={(event) => setPassword(event.target.value)}
                    onFocus={() => setAutofillLocked(false)}
                    placeholder="Masukkan password"
                    readOnly={autofillLocked}
                    type={showPassword ? "text" : "password"}
                    value={password}
                  />
                  <button
                    aria-label={showPassword ? "Hide password" : "Show password"}
                    className="absolute right-4 top-1/2 z-30 -translate-y-1/2 cursor-pointer disabled:cursor-not-allowed disabled:opacity-50"
                    disabled={loading}
                    onClick={() => setShowPassword((current) => !current)}
                    type="button"
                  >
                    {showPassword ? (
                      <EyeIcon className="fill-gray-500 dark:fill-gray-400" />
                    ) : (
                      <EyeCloseIcon className="fill-gray-500 dark:fill-gray-400" />
                    )}
                  </button>
                </div>
              </div>

              <button
                className="inline-flex h-12 w-full items-center justify-center gap-2 rounded-lg bg-[#465FFF] px-4 text-sm font-semibold text-white shadow-theme-md transition-colors hover:bg-[#3641F5] focus:outline-none focus:ring-3 focus:ring-[#465FFF]/20 disabled:cursor-not-allowed disabled:opacity-60"
                disabled={loading}
                type="submit"
              >
                <Icon className="size-5" name="login" />
                {loading ? "Memproses..." : "Sign in"}
              </button>
            </div>
          </form>
        </div>

        <p className="mt-6 text-center text-xs text-gray-500 dark:text-gray-500">Nusakarya Digital Solution</p>
      </div>
    </div>
  );
}
