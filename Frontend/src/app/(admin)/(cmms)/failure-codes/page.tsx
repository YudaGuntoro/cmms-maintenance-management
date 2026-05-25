"use client";

import { EntityPage } from "@/cmms/EntityPage";
import { failureCodeConfig } from "@/cmms/entity-config";

export default function FailureCodes() {
  return <EntityPage config={failureCodeConfig} />;
}
