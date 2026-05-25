"use client";

import { useMemo, useState } from "react";
import { EntityPage } from "./EntityPage";
import { masterDataConfigs } from "./master-data-config";

export default function MasterDataPage() {
  const [activeIndex, setActiveIndex] = useState(0);
  const activeConfig = useMemo(() => masterDataConfigs[activeIndex] || masterDataConfigs[0], [activeIndex]);

  return (
    <div className="space-y-5">
      <div className="flex flex-col gap-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600 dark:text-brand-400">Master Data</p>
          <h1 className="mt-2 text-2xl font-semibold text-gray-900 dark:text-white">CMMS Master Setup</h1>
          <p className="mt-1 max-w-3xl text-sm leading-6 text-gray-500 dark:text-gray-400">
            Kelola master table yang dipakai sebagai referensi Work Order, Problem Report, Downtime, dan Preventive Schedule.
          </p>
        </div>
        <div className="flex max-w-full gap-2 overflow-x-auto rounded-lg border border-gray-200 bg-white p-2 shadow-theme-sm dark:border-gray-800 dark:bg-gray-900">
          {masterDataConfigs.map((config, index) => {
            const active = index === activeIndex;
            return (
              <button
                className={[
                  "h-10 shrink-0 rounded-md px-3 text-sm font-semibold transition-colors",
                  active
                    ? "bg-brand-500 text-white"
                    : "text-gray-600 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-300 dark:hover:bg-white/[0.06] dark:hover:text-white",
                ].join(" ")}
                key={config.endpoint}
                onClick={() => setActiveIndex(index)}
                type="button"
              >
                {config.title}
              </button>
            );
          })}
        </div>
      </div>

      <EntityPage config={activeConfig} key={activeConfig.endpoint} />
    </div>
  );
}
